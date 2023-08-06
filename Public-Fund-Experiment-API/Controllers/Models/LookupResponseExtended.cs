using PublicFundExperimentAPI.Services.Models;

namespace PublicFundExperimentAPI.Controllers.Models
{
    public class LookupResponseExtended : LookupResponse
    {
        public string RepositoryUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public LookupResponseExtended(string repositoryUrl)
        {
            RepositoryUrl = repositoryUrl;
        }
    }
}
