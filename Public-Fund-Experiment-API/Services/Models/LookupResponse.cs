#pragma warning disable IDE1006 // Naming Styles

namespace PublicFundExperimentAPI.Services.Models
{
    public class LookupResponse
    {
        public string? ecosystem { get; set; }
        public string? registry_url { get; set; }
        public long? dependent_packages_count { get; set; }
        public long? dependent_repos_count { get; set; }
        public long? docker_dependents_count { get; set; }
        public long? docker_downloads_count { get; set; }
        public long? downloads { get; set; }
        public string? downloads_period { get; set; }
        public DateTime? last_synced_at { get; set; }
    }
}
