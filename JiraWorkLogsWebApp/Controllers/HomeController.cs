using JiraWorkLogsWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using UWorx.JiraWorkLogs;
using UWorx.JiraWorkLogs.RabbitMQ;

namespace JiraWorkLogsWebApp.Controllers
{
    public class HomeController : Controller
    {
        const string CacheKey = "Connection";
        readonly ILogger<HomeController> logger;
        readonly IMemoryCache cache;
        readonly IWebAppRepository dataStore;

        public HomeController(ILogger<HomeController> logger,
            IMemoryCache cache,
            IWebAppRepository dataStore)
        {
            this.logger = logger;
            this.cache = cache;
            this.dataStore = dataStore;
        }

        public async Task<IActionResult> Index()
        {
            var remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString();

            var activityName = $"Home/Index";
            using var activity = JiraWorkLogsWebApp.JiraActivitySource.StartActivity(activityName)?
                .AddBaggage("RemoteIpAddress", remoteIpAddress);

            if (this.cache.TryGetValue(CacheKey, out bool isAvailable) && !isAvailable)
                return View(new IndexViewModel { IsError = true });

            try
            {
                var model = new IndexViewModel();

                activity?.AddEvent(new ActivityEvent("page1"));
                model.Page1 = await this.dataStore.GetHtmlAsync(1);

                activity?.AddEvent(new ActivityEvent("page2"));
                model.Page2 = await this.dataStore.GetHtmlAsync(2);

                activity?.AddEvent(new ActivityEvent("page3"));
                model.Page3 = await this.dataStore.GetHtmlAsync(3);
                
                activity?.AddEvent(new ActivityEvent("lastupdatetime"));
                model.LastUpdateTime = await this.dataStore.GetLastUpdateAsync();

                this.logger.LogInformation("Incrementing greeting");
                JiraWorkLogsWebApp.CountGreetings.Add(1);

                activity?.SetTag("greeting", JiraWorkLogsWebApp.CountGreetings);

                return View(model);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to fetch values from dataStore");
                this.cache.Set(CacheKey, false, TimeSpan.FromSeconds(15));
                return View(new IndexViewModel { IsError = true });
            }
        }

        public async Task<ActionResult> Reset()
        {
            await this.dataStore.InitializeAsync();
            this.cache.Remove(CacheKey);
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Sync([FromServices] IWebAppMessagingService messageSender)
        {
            messageSender.TriggerJiraSync();
            this.ViewBag.Message = "Sync is triggered";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
