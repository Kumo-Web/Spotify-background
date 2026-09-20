using System.Text.Json.Serialization;

namespace Infrastructure.Persistence;

/// <summary>
/// Storage envelope for every document in the single Cosmos container.
/// <para>
/// One container, partitioned by <see cref="PartitionKey"/>, with a <see cref="Type"/>
/// discriminator. Every access pattern in this app is a point read or a single-partition query,
/// so this shape keeps request-unit consumption comfortably inside the 1000 RU/s free tier.
/// </para>
/// </summary>
internal sealed class CosmosDocument<T>
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Partition key. Normally the Spotify user id, so a user's documents live together and
    /// every read is single-partition. See <see cref="PartitionKeys.PreAuth"/> for the one
    /// case where no user is known yet.
    /// </summary>
    [JsonPropertyName("spotifyUserId")]
    public required string PartitionKey { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("data")]
    public required T Data { get; init; }

    /// <summary>
    /// Cosmos native TTL in seconds. Null means "never expire". Used so short-lived documents
    /// (OAuth state, seen-album history) delete themselves without a cleanup job.
    /// </summary>
    [JsonPropertyName("ttl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TimeToLive { get; init; }

    /// <summary>Set by Cosmos. Used for optimistic concurrency on token refresh.</summary>
    [JsonPropertyName("_etag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ETag { get; init; }
}

internal static class PartitionKeys
{
    /// <summary>
    /// Reserved partition for documents created before the user is known.
    /// <para>
    /// OAuth state is written at the moment we redirect to Spotify — we cannot know who is
    /// logging in until the callback returns, so these documents have no user to partition by.
    /// They all share one logical partition, which is fine because each lives for five minutes
    /// and then Cosmos deletes it. The underscore prefix cannot collide with a real Spotify id.
    /// </para>
    /// </summary>
    public const string PreAuth = "_preauth";
}

internal static class DocumentTypes
{
    public const string User = "user";
    public const string Token = "token";
    public const string OAuthState = "oauthState";
    public const string TasteProfile = "tasteProfile";
    public const string WatchedArtist = "watchedArtist";
    public const string SeenAlbum = "seenAlbum";
    public const string GenerationRun = "generationRun";
    public const string JobRun = "jobRun";
}

internal static class DocumentIds
{
    public const string User = "user";
    public const string Token = "token";
    public const string TasteProfile = "profile:v1";

    public static string OAuthState(string state) => $"state:{state}";
    public static string WatchedArtist(string artistId) => $"artist:{artistId}";
    public static string SeenAlbum(string albumId) => $"album:{albumId}";
    public static string GenerationRun(string runId) => $"run:{runId}";
    public static string JobRun(string isoWeek) => $"job:{isoWeek}";
}
