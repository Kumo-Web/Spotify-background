using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence;

/// <summary>
/// Typed document operations over the single Cosmos container. Repositories depend on this so
/// Cosmos plumbing lives in one place rather than being repeated per aggregate.
/// </summary>
internal interface ICosmosStore
{
    Task<(T Value, string? ETag)?> ReadAsync<T>(string partitionKey, string id, CancellationToken ct = default);

    Task UpsertAsync<T>(string partitionKey, string id, string type, T data, int? ttlSeconds = null,
        string? ifMatchETag = null, CancellationToken ct = default);

    /// <summary>
    /// Deletes a document. When <paramref name="ifMatchETag"/> is supplied the delete only
    /// succeeds if the document is unchanged, which is how single-use semantics are enforced:
    /// two concurrent consumers race on the ETag and exactly one wins.
    /// </summary>
    /// <returns>False if the document was absent or had already changed.</returns>
    Task<bool> DeleteAsync(string partitionKey, string id, string? ifMatchETag = null,
        CancellationToken ct = default);

    IAsyncEnumerable<T> QueryByTypeAsync<T>(string partitionKey, string type, CancellationToken ct = default);
}

internal sealed class CosmosStore : ICosmosStore
{
    private readonly Container _container;
    private readonly ILogger<CosmosStore> _logger;

    public CosmosStore(CosmosClient client, IOptions<CosmosSettings> settings, ILogger<CosmosStore> logger)
    {
        var s = settings.Value;
        _container = client.GetContainer(s.DatabaseName, s.ContainerName);
        _logger = logger;
    }

    public async Task<(T Value, string? ETag)?> ReadAsync<T>(
        string partitionKey, string id, CancellationToken ct = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<CosmosDocument<T>>(
                id, new PartitionKey(partitionKey), cancellationToken: ct);

            return (response.Resource.Data, response.Resource.ETag);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // A missing document is an expected outcome, not an error.
            return null;
        }
    }

    public async Task UpsertAsync<T>(
        string partitionKey, string id, string type, T data, int? ttlSeconds = null,
        string? ifMatchETag = null, CancellationToken ct = default)
    {
        var document = new CosmosDocument<T>
        {
            Id = id,
            PartitionKey = partitionKey,
            Type = type,
            Data = data,
            TimeToLive = ttlSeconds,
        };

        var options = ifMatchETag is null ? null : new ItemRequestOptions { IfMatchEtag = ifMatchETag };

        await _container.UpsertItemAsync(
            document, new PartitionKey(partitionKey), options, ct);
    }

    public async Task<bool> DeleteAsync(
        string partitionKey, string id, string? ifMatchETag = null, CancellationToken ct = default)
    {
        var options = ifMatchETag is null ? null : new ItemRequestOptions { IfMatchEtag = ifMatchETag };

        try
        {
            await _container.DeleteItemAsync<CosmosDocument<object>>(
                id, new PartitionKey(partitionKey), options, ct);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode is HttpStatusCode.NotFound
                                                       or HttpStatusCode.PreconditionFailed)
        {
            // NotFound: already gone. PreconditionFailed: someone else got there first.
            // Both mean "this caller did not win", which is what the caller needs to know.
            return false;
        }
    }

    public async IAsyncEnumerable<T> QueryByTypeAsync<T>(
        string partitionKey, string type,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        // Parameterised, and scoped to one partition so this never fans out across the account.
        var query = new QueryDefinition("SELECT * FROM c WHERE c.type = @type")
            .WithParameter("@type", type);

        using var iterator = _container.GetItemQueryIterator<CosmosDocument<T>>(
            query,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) });

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(ct);
            foreach (var document in page)
            {
                yield return document.Data;
            }
        }
    }
}

/// <summary>
/// Builds the <see cref="CosmosClient"/> and ensures the database and container exist.
/// </summary>
internal static class CosmosClientFactory
{
    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static CosmosClient Create(CosmosSettings settings, Azure.Core.TokenCredential credential)
    {
        var options = new CosmosClientOptions
        {
            // System.Text.Json rather than the SDK's default Newtonsoft, so serialisation
            // behaviour matches the rest of the app and we carry one JSON library, not two.
            UseSystemTextJsonSerializerWithOptions = SerializerOptions,
            // Gateway keeps outbound connections to a single HTTPS port, which suits a
            // scale-to-zero container better than Direct mode's connection pool. It is also the
            // only mode the Linux emulator supports.
            ConnectionMode = ConnectionMode.Gateway,
        };

        // The emulator serves HTTPS with a self-signed certificate nothing trusts, and the .NET
        // SDK has no HTTP mode to fall back to, so without this every local call fails TLS
        // validation. Scoped to loopback deliberately: a real account is always reached over a
        // *.documents.azure.com host, so this cannot silently weaken a deployed environment.
        if (IsLoopback(settings.Endpoint))
        {
            options.ServerCertificateCustomValidationCallback = (_, _, _) => true;
        }

        // A key is only expected for the local emulator; in Azure we use managed identity.
        return string.IsNullOrWhiteSpace(settings.Key)
            ? new CosmosClient(settings.Endpoint, credential, options)
            : new CosmosClient(settings.Endpoint, settings.Key, options);
    }

    private static bool IsLoopback(string endpoint)
        => Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && uri.IsLoopback;

    /// <summary>
    /// Creates the database and container if absent. Called once at startup.
    /// </summary>
    public static async Task EnsureCreatedAsync(
        CosmosClient client, CosmosSettings settings, CancellationToken ct = default)
    {
        var database = await client.CreateDatabaseIfNotExistsAsync(
            settings.DatabaseName, throughput: settings.ThroughputRu, cancellationToken: ct);

        await database.Database.CreateContainerIfNotExistsAsync(
            new ContainerProperties(settings.ContainerName, "/spotifyUserId")
            {
                // Opt in to per-document TTL without expiring documents that don't set one.
                // -1 means "TTL enabled, no container-wide default".
                DefaultTimeToLive = -1,
            },
            cancellationToken: ct);
    }
}
