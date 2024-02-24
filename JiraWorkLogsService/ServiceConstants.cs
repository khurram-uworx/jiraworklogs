using System;
using System.Collections.Generic;
using System.IO;

namespace JiraWorkLogsService;

static class ServiceConstants
{
    public static string JiraUrl { get => Environment.GetEnvironmentVariable("JIRA_URL") ?? ""; }

    public static string JiraUser { get => Environment.GetEnvironmentVariable("JIRA_USER") ?? ""; }

    public static string JiraToken { get => Environment.GetEnvironmentVariable("JIRA_TOKEN") ?? ""; }

    public static string Jql
    {
        get
        {
            var jql = Environment.GetEnvironmentVariable("JIRA_JQL") ?? "";

            if (string.IsNullOrWhiteSpace(jql))
            {
                var previousMonth = DateTime.Now.AddMonths(-1);
                var start = new DateTime(previousMonth.Year, previousMonth.Month, 1);
                var finish = start.AddMonths(1);

                jql = $"worklogDate >= '{start:yyyy-M-d}' AND worklogDate < '{finish:yyyy-M-d}'";
            }

            return jql;
        }
    }

    public static string[] Emails
    {
        get
        {
            var emails = new List<string>();

            try
            {
                string data = File.ReadAllText("/data/emails.txt");
                foreach(var line in data.Split(Environment.NewLine.ToCharArray()))
                {
                    if (line.Contains("@"))
                        emails.Add(line.Trim());
                }
            }
            catch { }

            return emails.ToArray();
        }
    }
}
