namespace PublicFundExperimentAPI.Controllers.Models
{
    public record GetRepositoryDetailsResult(int requestCount, int uniqueCount, int okCount, int noResponseCount, int errorCount, DateTime startedOn, DateTime endedOn, List<RepositorySummaryResult> data);
}
