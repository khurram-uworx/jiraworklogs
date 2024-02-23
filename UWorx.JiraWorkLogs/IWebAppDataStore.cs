using System;
using System.Threading.Tasks;

namespace UWorx.JiraWorkLogs;

public interface IWebAppDataStore
{
    Task InitializeAsync();
    Task<string> GetHtmlAsync(int page);
    Task<string> GetLastUpdateAsync();
}
