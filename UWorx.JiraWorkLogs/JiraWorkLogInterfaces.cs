using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UWorx.JiraWorkLogs;

public class ActivityEventArgs : EventArgs
{
    public Activity MessageActivity { get; set; }
}

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

public interface IWebAppMessagingService
{
    void TriggerJiraSync();
}

public interface IServiceMessagingService
{
    event EventHandler<ActivityEventArgs> OnMessageReceived;
    void StartConsumer();
}
