using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Utils.Cache;

namespace Utils.Data;

internal class Summarizer
{
    public async Task<int> ProcessAsync()
    {
        var start = new DateTime(2023, 12, 1);
        var finish = start.AddMonths(1);
        var dictionary = new WorklogDictionary(start);

        using (var db = new JiraDbContext())
        {
            string sql = @"update TeamMembers set Concerned = 1 where TeamMemberEmail in ('abdul.hai@juriba.com', 'abdulraffay.saeed@juriba.com', 'arslan.ahmad@juriba.com', 'hamza.mehmood@juriba.com',
                    'khurram.aziz@juriba.com', 'mohammed.butt@juriba.com', 'samia.saleem@juriba.com', 'sana.fatehkhan@juriba.com', 'tahreem.ahmad@juriba.com')";
            db.Database.ExecuteSqlRaw(sql);

            sql = @"select t.TeamMemberName, t.TeamMemberEmail, w.WorklogDate, sum(w.TimeSpentSeconds) TimeSpentSeconds
                    from Worklogs w join TeamMembers t on w.TeamMemberId = t.TeamMemberId
                    where t.Concerned = 1
	                    and w.WorklogDate between '2023-12-1' and '2024-1-1'
                    group by t.TeamMemberName, t.TeamMemberEmail, w.WorklogDate
                    order by 1, 2, 3";

            var q = from w in db.Worklogs
                    join t in db.TeamMembers on w.TeamMemberId equals t.TeamMemberId
                    where t.Concerned &&
                    w.WorklogDate >= start && w.WorklogDate < finish
                    group new { t, w } by new { t.TeamMemberName, t.TeamMemberEmail, w.WorklogDate } into g
                    select new
                    {
                        g.Key.TeamMemberName,
                        g.Key.TeamMemberEmail,
                        g.Key.WorklogDate,
                        TimeSpentSeconds = g.Sum(x => x.w.TimeSpentSeconds)
                    };
            q = q.OrderBy(x => x.TeamMemberName).ThenBy(x => x.WorklogDate);

            foreach (var r in q)
            {
                dictionary.AddTeamMember(r.TeamMemberEmail, r.TeamMemberName); // this will initialize days as well
                dictionary[r.TeamMemberEmail][r.WorklogDate.Day] = r.TimeSpentSeconds;
            }
        }

        var key = "LastUpdateTime";
        var value = DateTime.UtcNow.ToString();
        RedisConnection redisConnection = RedisConnection.InitializeAsync(Constants.RedisConnectionString).Result;

        await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page1", dictionary.Page1));
        await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page2", dictionary.Page2));
        await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync("page3", dictionary.Page3));

        await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync(key, value));

        return 4;
    }
}
