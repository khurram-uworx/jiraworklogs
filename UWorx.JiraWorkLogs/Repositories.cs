using System.Threading.Tasks;

namespace UWorx.JiraWorkLogs;

public interface IWebAppRepository
{
    Task InitializeAsync();
    Task<string> GetHtmlAsync(int page);
    Task<string> GetLastUpdateAsync();
}

public interface IServiceRepository
{
    Task SaveHtmlAsync(int page, string html);
}