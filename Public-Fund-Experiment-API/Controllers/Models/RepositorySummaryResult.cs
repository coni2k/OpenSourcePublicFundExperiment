namespace PublicFundExperimentAPI.Controllers.Models
{
    public class RepositorySummaryResult
    {
        public Uri Url { get; private set; }
        public string Uuid { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public bool Archived { get; set; }
        public string Language { get; set; } = string.Empty;
        public object License { get; set; } = string.Empty;
        public int Stars { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastSyncedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        public RepositorySummaryResult(Uri url)
        {
            Url = url;
        }
    }
}
