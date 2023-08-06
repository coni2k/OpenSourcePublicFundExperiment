namespace PublicFundExperimentAPI.Controllers.Models
{
    public class GitHubRepositoryResult
    {
        public Uri UrlInput { get; private set; }
        public Uri? UrlGitHub { get; set; }
        public string Status { get; set; } = string.Empty;

        public GitHubRepositoryResult(Uri urlInput)
        {
            UrlInput = urlInput;
        }
    }
}
