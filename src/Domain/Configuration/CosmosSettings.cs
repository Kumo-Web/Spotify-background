namespace Domain.Configuration;

/// <summary>
/// Azure Cosmos DB connection settings.
/// <para>
/// One container holds every document type, partitioned by Spotify user id, with a <c>type</c>
/// discriminator. Every access pattern is a point read or single-partition query, which keeps
/// consumption inside the 1000 RU/s lifetime free tier.
/// </para>
/// </summary>
public sealed class CosmosSettings
{
    public const string SectionName = "Cosmos";

    /// <summary>Account endpoint. Preferred over a connection string so managed identity can be used.</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Account key. Only for the local emulator — in Azure, leave unset and authenticate with
    /// managed identity instead.
    /// </summary>
    public string? Key { get; set; }

    public string DatabaseName { get; set; } = "spotify";

    public string ContainerName { get; set; } = "app";

    /// <summary>
    /// Throughput to provision when creating the database. Kept at the free-tier ceiling so the
    /// account cannot bill; also cap total account throughput in the portal as a second guard.
    /// </summary>
    public int ThroughputRu { get; set; } = 1000;
}
