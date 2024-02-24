using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraWorkLogsService.Data;

class WorklogDictionary : Dictionary<string, Dictionary<int, long>>    // dictionary[email][day] = timeWorkedInSeconds
{
    double numberOfDays = 0;
    DateTime month;
    Dictionary<string, string> lookup = new();

    WorklogDictionary()
    { }

    public WorklogDictionary(DateTime month)
    {
        this.month = new DateTime(month.Year, month.Month, 1);
        var nextMonth = this.month.AddMonths(1);
        numberOfDays = nextMonth.Subtract(this.month).TotalDays;
    }

    string getPage(int pageNumber)
    {
        int start = pageNumber * 10 - 10 + 1;
        int ending = pageNumber == 3 ? Convert.ToInt32(numberOfDays) : pageNumber * 10;

        var sb = new StringBuilder();

        sb.Append("<table class=\"table\">");

        //header
        {
            sb.Append(@"<thead><tr><th scope=""col""></th>");

            for (int i = start; i <= ending; i++)
            {
                var dt = new DateTime(month.Year, month.Month, i);
                sb.Append($"<th scope=\"col\">{i}<br>{dt.DayOfWeek}</th>");
            }

            sb.Append("</tr></thead>");
        }
        {
            sb.Append("<tbody>");

            foreach (var k in lookup.Keys.OrderBy(s => s))
            {
                sb.Append($"<tr><td>{lookup[k]} [{k}]</td>");

                for (int i = start; i <= ending; i++)
                {
                    var seconds = this[k][i];
                    var value = string.Empty;

                    if (seconds >= 60 * 60)
                        value = $"{seconds / 60.0 / 60.0:0.##}h";
                    else if (seconds >= 60)
                        value = $"{seconds / 60.0:0.##}m";
                    else if (seconds > 0)
                        value = $"{seconds}s";

                    sb.Append($"<td>{value}</td>");
                }

                sb.Append("</tr>");
            }

            sb.Append("</tbody>");
        }

        sb.Append("</table>");

        return sb.ToString();
    }

    public void AddTeamMember(string email, string name)
    {
        if (!lookup.ContainsKey(email))
        {
            lookup.Add(email, name);

            Add(email, new Dictionary<int, long>());
            for (int i = 1; i <= numberOfDays; i++)
                this[email].Add(i, 0);
        }
    }

    public string Page1
    {
        get { return this.getPage(1); }
    }

    public string Page2
    {
        get { return this.getPage(2); }
    }

    public string Page3
    {
        get { return this.getPage(3); }
    }
}