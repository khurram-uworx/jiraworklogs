using Dapplo.Jira;
using JiraWorkLogsService.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraWorkLogsService.Helpers;

class JiraHelper
{
    IJiraClient jiraClient = null;

    JiraHelper() { }

    public JiraHelper(string uriString, string user, string password)
    {
        this.jiraClient = JiraClient.Create(new Uri(uriString));
        this.jiraClient.SetBasicAuthentication(user, password);

        using (var db = new JiraDbContext())
        {
            db.Database.EnsureCreated();
        }
    }

    public async Task ListIssuesAsync(string jql)
    {
        var knownMembers = new Dictionary<string, int>();

        using (var db = new JiraDbContext())
        {
            var searchResult = await this.jiraClient.Issue.SearchAsync(jql);

            while (searchResult.Count > 0)
            {
                foreach (var issue in searchResult.Issues)
                {
                    Console.WriteLine($"{issue.Key} {issue.Fields.Status.Name} {issue.Fields.Summary}");

                    var workLogs = await this.jiraClient.WorkLog.GetAsync(issue.Key);

                    foreach (var workLog in workLogs)
                    {
                        if (null != workLog.Author && !string.IsNullOrWhiteSpace(workLog.Author.EmailAddress) && workLog.Started.HasValue)
                        {
                            string memberName = workLog.Author.DisplayName;
                            string memberEmail = workLog.Author.EmailAddress;
                            int memberId = 0;

                            if (knownMembers.ContainsKey(memberEmail))
                                memberId = knownMembers[memberEmail];
                            else
                            {
                                var qtm = from tm in db.TeamMembers
                                          where tm.TeamMemberEmail == memberEmail
                                          select new { tm.TeamMemberId };
                                var rtm = qtm.FirstOrDefault();

                                if (null == rtm)
                                {
                                    var teamMember = new TeamMember()
                                    {
                                        TeamMemberName = memberName,
                                        TeamMemberEmail = memberEmail,
                                        Concerned = false
                                    };
                                    db.TeamMembers.Add(teamMember);
                                    db.SaveChanges();

                                    memberId = teamMember.TeamMemberId;
                                    knownMembers.Add(memberEmail, memberId);
                                }
                                else
                                {
                                    memberId = rtm.TeamMemberId;
                                    knownMembers.Add(memberEmail, memberId);
                                }
                            }

                            DateTimeOffset started = workLog.Started.Value;

                            if (workLog.TimeSpentSeconds.HasValue)
                            {
                                var qw = from w in db.Worklogs
                                         where w.WorklogJiraId == workLog.Id
                                         select new { w.WorklogId };
                                var rw = qw.FirstOrDefault();

                                if (null == rw)
                                {
                                    db.Worklogs.Add(new Worklog()
                                    {
                                        WorklogJiraId = workLog.Id,
                                        TeamMemberId = memberId,
                                        WorklogDate = started.Date,
                                        TimeSpentSeconds = workLog.TimeSpentSeconds.Value
                                    });
                                    db.SaveChanges();
                                }
                            }
                        }

                        Console.WriteLine($"\t{workLog.Id} {workLog.Author.EmailAddress} {workLog.Started} {workLog.TimeSpent}");
                    }
                }

                searchResult = await this.jiraClient.Issue.SearchAsync(searchResult.SearchParameter, searchResult.NextPage);
            }
        }
    }
}