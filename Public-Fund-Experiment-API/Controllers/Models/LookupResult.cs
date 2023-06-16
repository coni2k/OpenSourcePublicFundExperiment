namespace PublicFundExperimentAPI.Controllers.Models
{
    public record LookupResult(int requestCount, int uniqueCount, int okCount, int noLookupCount, int errorCount, DateTime startedOn, DateTime endedOn, List<Lookup> data);

    public record Lookup(
        string RepositoryUrl,
        string? ecosystem,
        string? registry_url,
        long? dependent_packages_count,
        long? dependent_repos_count,
        long? docker_dependents_count,
        long? docker_downloads_count,
        long? downloads,
        string? downloads_period,
        DateTime? last_synced_at,
        string Status
    );
}
