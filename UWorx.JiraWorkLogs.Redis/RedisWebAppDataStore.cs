using System;
using System.Threading.Tasks;

namespace UWorx.JiraWorkLogs.Redis
{
    public class RedisWebAppDataStore : IWebAppDataStore
    {
        const string KeyLastUpdateTime = "LastUpdateTime";
        readonly RedisConnection redisConnection = null;

        public RedisWebAppDataStore(string connectionString)
        {
            this.redisConnection = RedisConnection.InitializeAsync(connectionString).Result;
        }

        public async Task InitializeAsync()
        {
            string value = DateTime.UtcNow.ToString();

            //ViewBag.Command1 = $"GET {key}";
            //ViewBag.Command1Result = (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(key))).ToString();
            //ViewBag.Command2 = $"SET {key}";
            //ViewBag.Command2Result = (await redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(key, value))).ToString();

            await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync(KeyLastUpdateTime, value));

            await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page1", "<ul><li>Hello<li>World</ul>"));
            await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page2", "<ul><li>Hello<li>Azure</ul>"));
            await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page3", "<ul><li>Hello<li>Redis</ul>"));
        }

        public async Task<string> GetHtmlAsync(int page)
        {
            return (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync($"page{page}"))).ToString();
        }

        public async Task<string> GetLastUpdateAsync()
        {
            return (await redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(KeyLastUpdateTime))).ToString();
        }
    }
}
