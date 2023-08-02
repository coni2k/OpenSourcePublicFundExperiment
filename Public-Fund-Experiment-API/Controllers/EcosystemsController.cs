using Microsoft.AspNetCore.Mvc;
using PublicFundExperimentAPI.Controllers.Models;
using PublicFundExperimentAPI.Services;

namespace PublicFundExperimentAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class EcosystemsController : ControllerBase
    {
        private readonly EcosystemsClient _client;

        public EcosystemsController(EcosystemsClient client)
        {
            _client = client;
        }

        [HttpPost(Name = "GetTopLookupsByRepositoryUrl")]
        public async Task<LookupResult> GetTopLookupsByRepositoryUrl([FromBody] List<string> repositoryUrls, CancellationToken cancellationToken)
        {
            if (repositoryUrls is null || !repositoryUrls.Any()) throw new ArgumentException($"'{nameof(repositoryUrls)}' cannot be null or empty.", nameof(repositoryUrls));

            var uniqueRepositoryUrls = repositoryUrls
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .DistinctBy(item => item)
                .ToList();

            int okCount = 0, noLookupCount = 0, errorCount = 0;
            var startedOn = DateTime.UtcNow;
            Console.WriteLine($"{DateTime.UtcNow:T} - Starting the process.\r\n" +
                $"Request: {repositoryUrls.Count}\r\n" +
                $"Unique: {uniqueRepositoryUrls.Count}");

            var lookups = new List<Lookup>();

            foreach (var url in uniqueRepositoryUrls)
            {
                Console.WriteLine($"{DateTime.UtcNow:T} - User: {url}");

                var status = "";
                Lookup? topLookup = null;

                try
                {
                    var requestUrl = new Uri($"https://packages.ecosyste.ms/api/v1/packages/lookup?repository_url={url}&sort=dependent_repos_count&order=desc&per_page=1");
                    
                    var allRepos = await _client.GetDataAsync<Lookup[]?>(requestUrl, cancellationToken);
                    if (allRepos != null && allRepos.Any())
                    {
                        topLookup = allRepos.FirstOrDefault();
                        status = "OK";
                        okCount++;
                    }
                    else
                    {
                        status = "No lookups";
                        noLookupCount++;
                    }
                }
                catch (Exception ex)
                {
                    status = ex.Message;
                    errorCount++;
                    Console.WriteLine(ex.Message);
                }

                lookups.Add(new Lookup(
                    url,
                    topLookup?.ecosystem,
                    topLookup?.registry_url,
                    topLookup?.dependent_packages_count,
                    topLookup?.dependent_repos_count,
                    topLookup?.docker_dependents_count,
                    topLookup?.docker_downloads_count,
                    topLookup?.downloads,
                    topLookup?.downloads_period,
                    topLookup?.last_synced_at,
                    status
                ));
            }

            var endedOn = DateTime.UtcNow;
            var operationTimeMinutes = (int)(endedOn - startedOn).TotalMinutes;
            var operationTimeSeconds = (int)(endedOn - startedOn).TotalSeconds;
            var operationTimeText = operationTimeMinutes > 1 ? $"{operationTimeMinutes} minutes": $"{operationTimeSeconds} seconds";

            var result = new LookupResult(repositoryUrls.Count, uniqueRepositoryUrls.Count, okCount, noLookupCount, errorCount, startedOn, endedOn, lookups);

            Console.WriteLine($"{DateTime.UtcNow:T} - Finishing the process.\r\n" +
                $"Request: {result.requestCount}\r\n" +
                $"Unique: {result.uniqueCount}\r\n" +
                $"Ok: {result.okCount}\r\n" +
                $"No lookup: {result.noLookupCount}\r\n" +
                $"Error: {result.errorCount}\r\n" +
                $"Duration: {operationTimeText}");

            return result;
        }
    }
}
