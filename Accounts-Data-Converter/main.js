import accountsData from './input/accounts.json' assert { type: 'json' };
import countries from './input/countries.json' assert { type: 'json' };

window.convert = function convert() {
  const output = accountsData.data.accounts.nodes
    .filter((item) => item.repositoryUrl !== null)
    .map((item) => {

      const repositoryUrl = item.repositoryUrl !== '' ? new URL(item.repositoryUrl) : null;
      const paths = repositoryUrl.pathname.split('/');
      const gitHubUser = repositoryUrl === null
        ? ''
        : repositoryUrl.host === 'github.com' && paths[1] !== ''
          ? `https://github.com/${paths[1].toLowerCase()}`
          : '';

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
        Country:
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
