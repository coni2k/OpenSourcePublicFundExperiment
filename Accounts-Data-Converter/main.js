import accountsData from './input/accounts.json' assert { type: 'json' };
import countries from './input/countries.json' assert { type: 'json' };
import repositoryStatuses from './input/repositoryStatuses.json' assert { type: 'json' };

window.convert = function convert() {
  const output = accountsData.data.accounts.nodes
    .filter((item) => item.repositoryUrl !== null)
    .map((item) => {
      const repositoryUrl = new URL(item.repositoryUrl);
      const paths = repositoryUrl.pathname.split('/');
      const isGitHub = repositoryUrl.host === 'github.com';
      const gitHubUserPath = paths[1] || '';
      const gitHubRepoPath = paths[2] || '';
      const gitHubUser = isGitHub && gitHubUserPath !== ''
          ? `https://github.com/${gitHubUserPath}`
          : '';
      const gitHubRepositoryInput = isGitHub && gitHubUserPath !== '' && gitHubRepoPath !== ''
          ? `https://github.com/${gitHubUserPath}/${gitHubRepoPath}`
          : '';
      const repositoryStatus = repositoryStatuses.find((item) => item.urlInput === gitHubRepositoryInput) || null;
      const gitHubRepository = repositoryStatus !== null ? repositoryStatus.status === 'Repo not found' ? '' : repositoryStatus.urlGitHub : '';
      const gitHubStatus = repositoryStatus !== null ? repositoryStatus.status === 'Repo not found' ? 'Repo not found' : 'OK' : 'No status';

      return {
        Id: item.id,
        Slug: item.slug,
        Type: item.type,
        Name: item.name,
        OpenCollective: `https://opencollective.com/${item.slug}`,
        Twitter:
          item.twitterHandle !== null
            ? `https://twitter.com/${item.twitterHandle}`
            : '', // TODO Other social Links?
        CodeRepository: item.repositoryUrl,
        GitHubUser: gitHubUser,
        GithubRepository: gitHubRepository,
        GitHubStatus: gitHubStatus,
        LocationCountry:
          item.location !== null && item.location.country !== null
            ? countries[Object.keys(countries).find(key => key == item.location.country)]
            : '',
        IsFrozen: item.isFrozen,
        IsActive: item.isActive,
        CreatedAt: item.createdAt,
        UpdatedAt: item.updatedAt,
        Budget: item.stats.yearlyBudget.value,
        Currency: item.stats.yearlyBudget.currency,
      };
    });
    
    navigator.clipboard.writeText(JSON.stringify(output));

    alert('The output was copied to the clipboard! ✅');
}
