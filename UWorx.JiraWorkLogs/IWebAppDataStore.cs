using System;
using System.Threading.Tasks;

namespace UWorx.JiraWorkLogs;

public interface IWebAppDataStore
{
    Task InitializeAsync();
    Task SaveHtmlAsync(int page, string html);
    Task<string> GetHtmlAsync(int page);
    Task<string> GetLastUpdateAsync();
}
