namespace PublicFundExperimentAPI.Services.Models
{
    public class RepositorySummaryResponse
    {
        public Repository repository { get; set; }
    }

    public class Repository
    {
        public string uuid { get; set; }
        public string owner { get; set; }
        public bool archived { get; set; }
        public int stargazers_count { get; set; }
        public DateTime last_synced_at { get; set; }
        public string language { get; set; }
        public object license { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
