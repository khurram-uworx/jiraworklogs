using JiraWorkLogsService.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UWorx.JiraWorkLogs;
using UWorx.JiraWorkLogs.Redis;

namespace JiraWorkLogsService.Helpers;

class Summarizer
{
    private readonly IServiceRepository dataStore;

    public Summarizer(IServiceRepository dataStore)
    {
        this.dataStore = dataStore;
    }

    public async Task<int> ProcessAsync(string[] emails, DateTime start)
    {
        var finish = start.AddMonths(1);
        var dictionary = new WorklogDictionary(start);

        using (var db = new JiraDbContext())
        {
            var qt = from t in db.TeamMembers
                     where emails.Contains(t.TeamMemberEmail)
                     select t;

            bool needUpdate = false;
            foreach (var rt in qt)
            {
                if (!rt.Concerned)
                {
                    needUpdate = true;
                    rt.Concerned = true;
                }
            }
            if (needUpdate) db.SaveChanges();

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
                        TimeSpentSeconds = g.Sum(x => x.w.TimeSpentSeconds)             // aggregation
                    };
            q = q.OrderBy(x => x.TeamMemberName).ThenBy(x => x.WorklogDate);

            foreach (var r in q)
            {
                dictionary.AddTeamMember(r.TeamMemberEmail, r.TeamMemberName);          // this will initialize days as well and its safe to call it multiple times
                dictionary[r.TeamMemberEmail][r.WorklogDate.Day] = r.TimeSpentSeconds;  // we already have aggregated it; but could do it here well
            }
        }

        await this.dataStore.SaveHtmlAsync(1, dictionary.Page1);
        await this.dataStore.SaveHtmlAsync(2, dictionary.Page2);
        await this.dataStore.SaveHtmlAsync(3, dictionary.Page3);

        return 3;
    }

    public async Task<int> ProcessAsync(string[] emails)
    {
        var previousMonth = DateTime.Now.AddMonths(-1);
        var start = new DateTime(previousMonth.Year, previousMonth.Month, 1);
        return await ProcessAsync(emails, start);
    }
}
