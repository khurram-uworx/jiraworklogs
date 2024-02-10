using JiraWorkLogsWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using Utils;

namespace JiraWorkLogsWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly Task<RedisConnection> redisConnectionFactory;

        public HomeController(ILogger<HomeController> logger,
            Task<RedisConnection> redisConnectionFactory)
        {
            this.logger = logger;
            this.redisConnectionFactory = redisConnectionFactory;
        }

        public async Task<IActionResult> Index()
        {
            var remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString();

            var activityName = $"Home/Index";
            using var activity = Program.JiraActivitySource.StartActivity(activityName)?
                .AddBaggage("RemoteIpAddress", remoteIpAddress);

            var redisConnection = await this.redisConnectionFactory;
            activity?.AddEvent(new ActivityEvent("page1"));
            ViewBag.Page1 = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("page1"))).ToString();
            activity?.AddEvent(new ActivityEvent("page2"));
            ViewBag.Page2 = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("page2"))).ToString();
            activity?.AddEvent(new ActivityEvent("page3"));
            ViewBag.Page3 = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("page3"))).ToString();
            activity?.AddEvent(new ActivityEvent("lastupdatetime"));
            ViewBag.LastUpdateTime = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("LastUpdateTime"))).ToString();

            this.logger.LogInformation("Incrementing greeting");
            Program.CountGreetings.Add(1);

            activity?.SetTag("greeting", Program.CountGreetings);

            return View();
        }

        public async Task<ActionResult> Redis()
        {
            var redisConnection = await this.redisConnectionFactory;

            string key = "LastUpdateTime";
            string value = DateTime.UtcNow.ToString();

            ViewBag.Command1 = $"GET {key}";
            ViewBag.Command1Result = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(key))).ToString();
            ViewBag.Command2 = $"SET {key}";
            ViewBag.Command2Result = (await redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(key, value))).ToString();

            await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page1", "<ul><li>Hello<li>World</ul>"));
            await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page2", "<ul><li>Hello<li>Azure</ul>"));
            await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page3", "<ul><li>Hello<li>Redis</ul>"));

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
