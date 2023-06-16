namespace PublicFundExperimentAPI.Controllers.Models
{
    public record GitHubRepository(string User, string? Url, string? Language, string? License, int? Stars, string? Status);
}
