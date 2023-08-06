namespace PublicFundExperimentAPI.Controllers.Models
{
    public record GetRepositoryByUrlResult(int requestCount, int uniqueCount, int okCount, int errorCount, DateTime startedOn, DateTime endedOn, List<GitHubRepositoryResult> data);
}
