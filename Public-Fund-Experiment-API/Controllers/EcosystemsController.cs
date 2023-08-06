using Microsoft.AspNetCore.Mvc;
using PublicFundExperimentAPI.Controllers.Models;
using PublicFundExperimentAPI.Services;
using PublicFundExperimentAPI.Services.Models;
using System.Text.Json;

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

        [HttpPost(Name = "GetRepositoryDetails")]
        public async Task<GetRepositoryDetailsResult> GetRepositoryDetails([FromBody] List<Uri> repositoryUrls, CancellationToken cancellationToken)
        {
            if (repositoryUrls is null || !repositoryUrls.Any()) throw new ArgumentException($"'{nameof(repositoryUrls)}' cannot be null or empty.", nameof(repositoryUrls));

            var uniqueRepositoryUrls = repositoryUrls
                .Where(item => !string.IsNullOrWhiteSpace(item.AbsoluteUri))
                .DistinctBy(item => item)
                .ToList();

            int okCount = 0, noResponseCount = 0, errorCount = 0;
            var startedOn = DateTime.UtcNow;
            Console.WriteLine($"{DateTime.UtcNow:T} - Starting the process.\r\n" +
                $"Request: {repositoryUrls.Count}\r\n" +
                $"Unique: {uniqueRepositoryUrls.Count}");

            var repositoryResults = new List<RepositorySummaryResult>();

            foreach (var url in uniqueRepositoryUrls)
            {
                Console.WriteLine($"{DateTime.UtcNow:T} - Url: {url}");

                var repository = new RepositorySummaryResult(url);
                repositoryResults.Add(repository);

                try
                {
                    var requestUrl = new Uri($"https://summary.ecosyste.ms/api/v1/projects/lookup?url={url}");

                    var response = await _client.GetDataAsync<RepositorySummaryResponse>(requestUrl, cancellationToken);
                    if (response != null && response.repository != null)
                    {
                        repository.Uuid = response.repository.uuid;
                        repository.Owner = response.repository.owner;
                        repository.Archived = response.repository.archived;
                        repository.Language = response.repository.language;
                        repository.License = response.repository.license;
                        repository.Stars = response.repository.stargazers_count;
                        repository.CreatedAt = response.repository.created_at;
                        repository.UpdatedAt = response.repository.updated_at;
                        repository.LastSyncedAt = response.repository.last_synced_at;
                        repository.Status = "OK";
                        okCount++;
                    }
                    else
                    {
                        repository.Status = "No response";
                        noResponseCount++;
                    }
                }
                catch (Exception ex)
                {
                    repository.Status = ex.Message;
                    errorCount++;
                    Console.WriteLine(ex.Message);
                }
            }

            var endedOn = DateTime.UtcNow;
            var operationTimeMinutes = (int)(endedOn - startedOn).TotalMinutes;
            var operationTimeSeconds = (int)(endedOn - startedOn).TotalSeconds;
            var operationTimeText = operationTimeMinutes > 1 ? $"{operationTimeMinutes} minutes" : $"{operationTimeSeconds} seconds";

            var result = new GetRepositoryDetailsResult(repositoryUrls.Count, uniqueRepositoryUrls.Count, okCount, noResponseCount, errorCount, startedOn, endedOn, repositoryResults);

            Console.WriteLine($"{DateTime.UtcNow:T} - Finishing the process.\r\n" +
                $"Request: {result.requestCount}\r\n" +
                $"Unique: {result.uniqueCount}\r\n" +
                $"Ok: {result.okCount}\r\n" +
                $"No response: {result.noResponseCount}\r\n" +
                $"Error: {result.errorCount}\r\n" +
                $"Duration: {operationTimeText}");

            var json = JsonSerializer.Serialize(result);
            var fileName = @$".\output\{nameof(GetRepositoryDetails)}.json";
            System.IO.File.WriteAllText(fileName, json);

            return result;
        }

        [HttpPost(Name = "GetTopLookupsByRepositoryUrl")]
        public async Task<GetTopLookupsResult> GetTopLookupsByRepositoryUrl([FromBody] List<string> repositoryUrls, CancellationToken cancellationToken)
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

            var lookups = new List<LookupResponseExtended>();

            foreach (var url in uniqueRepositoryUrls)
            {
                Console.WriteLine($"{DateTime.UtcNow:T} - Url: {url}");

                var extended = new LookupResponseExtended(url);
                lookups.Add(extended);

                try
                {
                    var requestUrl = new Uri($"https://packages.ecosyste.ms/api/v1/packages/lookup?repository_url={url}&sort=dependent_repos_count&order=desc&per_page=1");
                    
                    var allRepos = await _client.GetDataAsync<LookupResponse[]?>(requestUrl, cancellationToken);
                    if (allRepos != null && allRepos.Any())
                    {
                        var topLookup = allRepos.FirstOrDefault();
                        extended.ecosystem = topLookup?.ecosystem;
                        extended.registry_url = topLookup?.registry_url;
                        extended.dependent_packages_count = topLookup?.dependent_packages_count;
                        extended.dependent_repos_count = topLookup?.dependent_repos_count;
                        extended.docker_dependents_count = topLookup?.docker_dependents_count;
                        extended.docker_downloads_count = topLookup?.docker_downloads_count;
                        extended.downloads = topLookup?.downloads;
                        extended.downloads_period = topLookup?.downloads_period;
                        extended.last_synced_at = topLookup?.last_synced_at;

                        extended.Status = "OK";
                        okCount++;
                    }
                    else
                    {
                        extended.Status = "No lookups";
                        noLookupCount++;
                    }
                }
                catch (Exception ex)
                {
                    extended.Status = ex.Message;
                    errorCount++;
                    Console.WriteLine(ex.Message);
                }
            }

            var endedOn = DateTime.UtcNow;
            var operationTimeMinutes = (int)(endedOn - startedOn).TotalMinutes;
            var operationTimeSeconds = (int)(endedOn - startedOn).TotalSeconds;
            var operationTimeText = operationTimeMinutes > 1 ? $"{operationTimeMinutes} minutes": $"{operationTimeSeconds} seconds";

            var result = new GetTopLookupsResult(repositoryUrls.Count, uniqueRepositoryUrls.Count, okCount, noLookupCount, errorCount, startedOn, endedOn, lookups);

            Console.WriteLine($"{DateTime.UtcNow:T} - Finishing the process.\r\n" +
                $"Request: {result.requestCount}\r\n" +
                $"Unique: {result.uniqueCount}\r\n" +
                $"Ok: {result.okCount}\r\n" +
                $"No lookup: {result.noLookupCount}\r\n" +
                $"Error: {result.errorCount}\r\n" +
                $"Duration: {operationTimeText}");

            var json = JsonSerializer.Serialize(result);
            var fileName = @$".\output\{nameof(GetTopLookupsByRepositoryUrl)}.json";
            System.IO.File.WriteAllText(fileName, json);

            return result;
        }
    }
}
