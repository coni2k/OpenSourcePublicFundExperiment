using Microsoft.AspNetCore.Mvc;
using Octokit;
using PublicFundExperimentAPI.Controllers.Models;

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

        [HttpGet(Name = "GetUser")]
        public async Task<User> GetUser(CancellationToken cancellationToken)
        {
            return await _client.User.Current();
        }

        [HttpPost(Name = "GetTopRepositoryBySearch")]
        public async Task<List<GitHubRepository>> GetTopRepositoryBySearch([FromBody] List<string> users, CancellationToken cancellationToken)
        {
            if (users is null || !users.Any()) throw new ArgumentException($"'{nameof(users)}' cannot be null or empty.", nameof(users));

            users = users
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.ToLowerInvariant().Replace("https://github.com/", ""))
                .DistinctBy(item => item)
                .ToList();

            var repositories = new List<GitHubRepository>();

            foreach (var user in users)
            {
                Console.WriteLine($"user: {user}");

                var request = new SearchRepositoriesRequest
                {
                    User = user,
                    SortField = RepoSearchSort.Stars,
                    Order = SortDirection.Descending,
                    PerPage = 1
                };

                SearchRepositoryResult? searchResult = null;

                var status = "OK";

                try
                {
                    searchResult = await _client.Search.SearchRepo(request);
                }
                catch (ApiValidationException ex)
                {
                    ApiErrorDetail? error = ex.ApiError.Errors.Any() ? ex.ApiError.Errors[0] : null;
                    if (error?.Message == "The listed users and repositories cannot be searched either because the resources do not exist or you do not have permission to view them.")
                        status = "User not found";
                    //else if (error?.Message == "API Rate limit?")
                    //    repository.Status = "Rate limit...";
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    // Retry?
                    Console.WriteLine(ex.Message);
                }

                Repository? topRepo = null;
                if (searchResult != null)
                {
                    if (searchResult.Items.Any())
                        topRepo = searchResult.Items[0];
                    else
                        status = "No repositories";
                }

                repositories.Add(new GitHubRepository(
                    $"https://github.com/{user}",
                    topRepo?.HtmlUrl,
                    topRepo?.Language,
                    topRepo?.License?.Name,
                    topRepo?.StargazersCount,
                    status
                ));

                // Rate limits
                // https://octokitnet.readthedocs.io/en/latest/getting-started/#too-much-of-a-good-thing-dealing-with-api-rate-limits
                // https://docs.github.com/en/rest/search?apiVersion=2022-11-28
                var rateLimits = await _client.RateLimit.GetRateLimits();
                var searchLimits = rateLimits.Resources.Search;
                if (searchLimits != null && searchLimits.Remaining == 0)
                {
                    var resetTime = searchLimits.Reset.ToUniversalTime().AddMicroseconds(500);
                    var waitTime = (int)(resetTime - DateTime.UtcNow).TotalMilliseconds;

                    if (waitTime > 0)
                    {
                        Console.WriteLine($"Waiting until {resetTime}");
                        Task.Delay(waitTime, cancellationToken).Wait(cancellationToken);
                    }
                }
            }



            return repositories;
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

                var status = "OK";
                Repository? topRepo = null;

                try
                {
                    var allRepos = await _client.Repository.GetAllForUser(user);
                    if (allRepos.Any())
                        topRepo = allRepos.OrderByDescending(item => item.StargazersCount).FirstOrDefault();
                    else
                        status = "No repositories";
                }
                catch (NotFoundException)
                {
                    status = "User not found";
                }
                catch (ApiValidationException ex)
                {
                    status = ex.Message;
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    status = ex.Message;
                    Console.WriteLine(ex.Message);
                }

                repositories.Add(new GitHubRepository(
                    $"https://github.com/{user}",
                    topRepo?.HtmlUrl,
                    topRepo?.Language,
                    topRepo?.License?.Name,
                    topRepo?.StargazersCount,
                    status
                ));

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
            var operationTimeText = operationTimeMinutes > 1 ? $"{operationTimeMinutes} minutes": $"{operationTimeSeconds} seconds";
            Console.WriteLine($"{DateTime.UtcNow:T} - Finishing the process, took {operationTimeText}");

            return repositories;
        }
    }
}
