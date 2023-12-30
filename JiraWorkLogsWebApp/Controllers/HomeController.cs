using JiraWorkLogsWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Utils.Cache;
using Utils.Messaging;

namespace JiraWorkLogsWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ActivitySource source = new ActivitySource("HomeController");

        private readonly ILogger<HomeController> logger;
        private readonly Task<RedisConnection> redisConnectionFactory;
        private readonly MessageSender messageSender;
        private RedisConnection redisConnection;

        public HomeController(ILogger<HomeController> logger,
            Task<RedisConnection> redisConnectionFactory,
            MessageSender messageSender)
        {
            this.logger = logger;
            this.redisConnectionFactory = redisConnectionFactory;
            this.messageSender = messageSender;
        }

        public async Task<IActionResult> Index()
        {
            logger.LogInformation($"Index is requested from {this.HttpContext.Connection.RemoteIpAddress}");

            using (Activity activity = source.StartActivity("Index-Work"))
            {
                this.redisConnection = await redisConnectionFactory;

                activity?.AddEvent(new ActivityEvent("page1"));
                ViewBag.Page1 = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("page1"))).ToString();

                activity?.AddEvent(new ActivityEvent("page2"));
                ViewBag.Page2 = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("page2"))).ToString();

                activity?.AddEvent(new ActivityEvent("page3"));
                ViewBag.Page3 = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("page3"))).ToString();

                activity?.AddEvent(new ActivityEvent("lastupdatetime"));
                ViewBag.LastUpdateTime = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("LastUpdateTime"))).ToString();
            }

            return View();
        }

        public async Task<ActionResult> Redis()
        {
            this.redisConnection = await redisConnectionFactory;
            ViewBag.Message = "A simple example for Redis on ASP.NET Core.";

            // Perform cache operations using the cache object...

            // Simple PING command
            ViewBag.command1 = "PING";
            ViewBag.command1Result = (await redisConnection.BasicRetryAsync(async (db) => await db.ExecuteAsync(ViewBag.command1))).ToString();

            // Simple get and put of integral data types into the cache
            string key = "Message";
            string value = "Hello! The cache is working from ASP.NET Core!";

            ViewBag.command2 = $"SET {key} \"{value}\"";
            ViewBag.command2Result = (await redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(key, value))).ToString();

            ViewBag.command3 = $"GET {key}";
            ViewBag.command3Result = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(key))).ToString();

            key = "LastUpdateTime";
            value = DateTime.UtcNow.ToString();

            ViewBag.command4 = $"GET {key}";
            ViewBag.command4Result = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(key))).ToString();

            ViewBag.command5 = $"SET {key}";
            ViewBag.command5Result = (await redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(key, value))).ToString();

            await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page1", "<ul><li>Hello<li>World</ul>"));
            await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page2", "<ul><li>Hello<li>Azure</ul>"));
            await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page3", "<ul><li>Hello<li>Redis</ul>"));

            return View();
        }

        public ActionResult Rabbit()
        {
            this.messageSender.SendMessage();
            this.ViewBag.Message = "RabbitMQ message is sent";
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
