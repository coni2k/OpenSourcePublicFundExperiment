using System.Text.Json;

namespace PublicFundExperimentAPI.Services
{
    public class EcosystemsClient
    {
        private readonly HttpClient _httpClient;

        public EcosystemsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = new TimeSpan(0, 0, 500);
        }

        public async Task<T?> GetDataAsync<T>(Uri uri, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "OpenSourcePublicFundExperiment");

            HttpResponseMessage? response = default;
            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
            }
            catch (HttpRequestException exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
            catch (JsonException exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }
    }
}
