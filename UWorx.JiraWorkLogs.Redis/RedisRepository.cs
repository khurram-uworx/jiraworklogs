using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace UWorx.JiraWorkLogs.Redis
{
    public class RedisRepository : IWebAppRepository, IServiceRepository
    {
        const string KeyLastUpdateTime = "LastUpdateTime";

        readonly ILogger<RedisRepository> logger;
        readonly RedisConnection connection = null;

        public RedisRepository(ILogger<RedisRepository> logger)
        {
            this.logger = logger;
            this.connection = RedisConnection.InitializeAsync(Environment.GetEnvironmentVariable("ConnectionStrings__redis")).Result;
        }

        public async Task InitializeAsync()
        {
            string value = DateTime.UtcNow.ToString();

            //ViewBag.Command1 = $"GET {key}";
            //ViewBag.Command1Result = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(key))).ToString();
            //ViewBag.Command2 = $"SET {key}";
            //ViewBag.Command2Result = (await redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(key, value))).ToString();

            await connection.BasicRetryAsync(async db => await db.StringSetAsync(KeyLastUpdateTime, value));

            await connection.BasicRetryAsync(async db => await db.StringSetAsync("page1", "<ul><li>Hello<li>World</ul>"));
            await connection.BasicRetryAsync(async db => await db.StringSetAsync("page2", "<ul><li>Hello<li>Azure</ul>"));
            await connection.BasicRetryAsync(async db => await db.StringSetAsync("page3", "<ul><li>Hello<li>Redis</ul>"));
        }

        public async Task SaveHtmlAsync(int page, string html)
        {
            var value = DateTime.UtcNow.ToString();
            await connection.BasicRetryAsync(async db => await db.StringSetAsync($"page{page}", html));
            await connection.BasicRetryAsync(async db => await db.StringSetAsync(KeyLastUpdateTime, value));
        }

        public async Task<string> GetHtmlAsync(int page)
        {
            return (await connection.BasicRetryAsync(async (db) => await db.StringGetAsync($"page{page}"))).ToString();
        }

        public async Task<string> GetLastUpdateAsync()
        {
            return (await connection.BasicRetryAsync(async (db) => await db.StringGetAsync(KeyLastUpdateTime))).ToString();
        }
    }
}
