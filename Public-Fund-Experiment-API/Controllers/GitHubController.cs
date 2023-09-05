using Microsoft.AspNetCore.Mvc;
using Octokit;
using PublicFundExperimentAPI.Controllers.Models;
using System.Net;
using System.Text.Json;

namespace PublicFundExperimentAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GitHubController : ControllerBase
    {
        private readonly GitHubClient _client;

        public GitHubController(GitHubClient client)
        {
            _client = client;
        }

        [HttpPost(Name = "GetRepositoryByUrl")]
        public async Task<GetRepositoryByUrlResult> GetRepositoryByUrl([FromBody] List<Uri> repositoryUrls, CancellationToken cancellationToken)
        {
            if (repositoryUrls is null || !repositoryUrls.Any()) throw new ArgumentException($"'{nameof(repositoryUrls)}' cannot be null or empty.", nameof(repositoryUrls));

            var uniqueRepositoryUrls = repositoryUrls
                .Where(item => !string.IsNullOrWhiteSpace(item.AbsoluteUri))
                .DistinctBy(item => item)
                .ToList();

            int okCount = 0, errorCount = 0;
            var startedOn = DateTime.UtcNow;
            Console.WriteLine($"{DateTime.UtcNow:T} - Starting the process.\r\n" +
                $"Request: {repositoryUrls.Count}\r\n" +
                $"Unique: {uniqueRepositoryUrls.Count}");

            var repositoryResults = new List<GitHubRepositoryResult>();

            foreach (var url in uniqueRepositoryUrls)
            {
                Console.WriteLine($"{DateTime.UtcNow:T} - Url: {url}");

                var result = new GitHubRepositoryResult(url);
                repositoryResults.Add(result);

                var owner = url.Segments[1].Replace("/", "");
                var name = url.Segments[2].Replace("/", "");

                if (owner == string.Empty || name == string.Empty)
                {
                    result.Status = "Error: owner or name cannot be null";

                    errorCount++;
                    continue;
                }

                try
                {
                    var repository = await _client.Repository.Get(owner, name);

                    result.UrlGitHub = new Uri(repository.HtmlUrl);
                    result.Status = url.AbsoluteUri == repository.HtmlUrl ? "OK" : "Redirect";
                    okCount++;
                }
                catch (NotFoundException)
                {
                    result.Status = "Repo not found";

                    errorCount++;
                }
            }

            var endedOn = DateTime.UtcNow;
            var operationTimeMinutes = (int)(endedOn - startedOn).TotalMinutes;
            var operationTimeSeconds = (int)(endedOn - startedOn).TotalSeconds;
            var operationTimeText = operationTimeMinutes > 1 ? $"{operationTimeMinutes} minutes" : $"{operationTimeSeconds} seconds";

            var results = new GetRepositoryByUrlResult(repositoryUrls.Count, uniqueRepositoryUrls.Count, okCount, errorCount, startedOn, endedOn, repositoryResults);

            Console.WriteLine($"{DateTime.UtcNow:T} - Finishing the process.\r\n" +
                $"Request: {results.requestCount}\r\n" +
                $"Unique: {results.uniqueCount}\r\n" +
                $"Ok: {results.okCount}\r\n" +
                $"Error: {results.errorCount}\r\n" +
                $"Duration: {operationTimeText}");

            var json = JsonSerializer.Serialize(results);
            var fileName = @$".\output\{nameof(GetRepositoryByUrl)}.json";
            System.IO.File.WriteAllText(fileName, json);

            return results;
        }

        [HttpPost(Name = "GetTopRepositoryByUser")]
        public async Task<List<GitHubRepository>> GetTopRepositoryByUser([FromBody] List<string> users, CancellationToken cancellationToken)
        {
            if (users is null || !users.Any()) throw new ArgumentException($"'{nameof(users)}' cannot be null or empty.", nameof(users));

            users = users
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.ToLowerInvariant().Replace("https://github.com/", ""))
                .DistinctBy(item => item)
                .ToList();

            var startedOn = DateTime.UtcNow;
            Console.WriteLine($"{DateTime.UtcNow:T} - Starting the process with {users.Count} users");

            var repositories = new List<GitHubRepository>();

            foreach (var user in users)
            {
                Console.WriteLine($"{DateTime.UtcNow:T} - User: {user}");

                var repository = new GitHubRepository($"https://github.com/{user}");
                repositories.Add(repository);

                try
                {
                    // Exceptional case: GitHub API returns repositories for "-" user, but it doesn't refer to an actual user on GitHub: https://github.com/-
                    if (user == "-")
                        throw new NotFoundException("Invalid user", HttpStatusCode.NotFound);

                    var allRepos = await _client.Repository.GetAllForUser(user);
                    if (allRepos.Any())
                    {
                        var topRepo = allRepos.OrderByDescending(item => item.StargazersCount).FirstOrDefault();
                        repository.Url = topRepo?.HtmlUrl;
                        repository.Status = "Has top repo";
                    }
                    else
                        repository.Status = "No repositories";
                }
                catch (NotFoundException)
                {
                    repository.Status = "User not found";
                }
                catch (ApiValidationException ex)
                {
                    repository.Status = ex.Message;
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    repository.Status = ex.Message;
                    Console.WriteLine(ex.Message);
                }

                // Rate limits
                // https://octokitnet.readthedocs.io/en/latest/getting-started/#too-much-of-a-good-thing-dealing-with-api-rate-limits
                // https://docs.github.com/en/rest/search?apiVersion=2022-11-28
                var rateLimits = await _client.RateLimit.GetRateLimits();
                var coreLimits = rateLimits.Resources.Core;
                if (coreLimits?.Remaining == 0)
                {
                    var resetTime = coreLimits.Reset.ToUniversalTime().AddMicroseconds(500);
                    var waitTime = (int)(resetTime - DateTime.UtcNow).TotalMilliseconds;

                    if (waitTime > 0)
                    {
                        Console.WriteLine($"{DateTime.UtcNow:T} - The rate limit was reached, waiting until {resetTime}");
                        Task.Delay(waitTime, cancellationToken).Wait(cancellationToken);
                    }
                }
            }

            var operationTimeMinutes = (int)(DateTime.UtcNow - startedOn).TotalMinutes;
            var operationTimeSeconds = (int)(DateTime.UtcNow - startedOn).TotalSeconds;
            var operationTimeText = operationTimeMinutes > 1 ? $"{operationTimeMinutes} minutes" : $"{operationTimeSeconds} seconds";
            Console.WriteLine($"{DateTime.UtcNow:T} - Finishing the process, took {operationTimeText}");

            var json = JsonSerializer.Serialize(repositories);
            var fileName = @$".\output\{nameof(GetTopRepositoryByUser)}.json";
            System.IO.File.WriteAllText(fileName, json);

            return repositories;
        }

        [HttpGet(Name = "GetUser")]
        public async Task<User> GetUser(CancellationToken cancellationToken)
        {
            return await _client.User.Current();
        }
    }
}
