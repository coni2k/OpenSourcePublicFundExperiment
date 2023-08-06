namespace PublicFundExperimentAPI.Controllers.Models
{
    public record GetTopLookupsResult(int requestCount, int uniqueCount, int okCount, int noLookupCount, int errorCount, DateTime startedOn, DateTime endedOn, List<LookupResponseExtended> data);
}
