using JiraWorkLogsWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JiraWorkLogsWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;

        public HomeController(ILogger<HomeController> logger)
        {
            this.logger = logger;
        }

        public IActionResult Index()
        {
            var remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString();

            var activityName = $"Home/Index";
            using var activity = Program.JiraActivitySource.StartActivity(activityName)?
                .AddBaggage("RemoteIpAddress", remoteIpAddress);

            this.logger.LogInformation("Incrementing greeting");
            Program.CountGreetings.Add(1);

            activity?.SetTag("greeting", Program.CountGreetings);

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
