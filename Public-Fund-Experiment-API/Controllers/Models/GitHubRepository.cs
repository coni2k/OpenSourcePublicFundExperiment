namespace PublicFundExperimentAPI.Controllers.Models
{
    public class GitHubRepository
    {
        public string User { get; private set; }
        public string? Url { get; set; }
        public string Status { get; set; } = string.Empty;

        public GitHubRepository(string user)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                throw new ArgumentException($"'{nameof(user)}' cannot be null or whitespace.", nameof(user));
            }

            User = user;
        }
    }
}
