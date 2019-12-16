using Atlassian.Jira;
using jiraps.Credentials;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace jiraps
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting..");
            await LoadWorklog();
            Console.WriteLine("Ending..");
        }

        static async Task LoadWorklog() {
            var user = (new CredentialsManager()).GetCredentials();
            var jira = Jira.CreateRestClient(user.ServerUrl, user.User, user.Token);

            var issues = await jira.Issues.GetIssuesFromJqlAsync("project = SAM AND Sprint = 28", 100);

            Console.WriteLine("Count: " + issues.Count());
            var workLogs = new List<IGrouping<string, Worklog>>();
            foreach (var i in issues) {
                Console.WriteLine("Loading " + i.JiraIdentifier);
                var tempLogs = await i.GetWorklogsAsync();
                Console.WriteLine("Loaded " + tempLogs.Count());
                var logs = tempLogs.Where(w => compareDate(w.StartDate)).GroupBy(w => w.Author);
                if (logs is null) { continue; }
                workLogs.AddRange(logs);
            }
            
            var data = new Dictionary<string, TimeSpan>();
            
            foreach (var g in workLogs) {
                if (!data.ContainsKey(g.Key))
                    data[g.Key] = TimeSpan.FromDays(0);
                var total = g.Sum(w => w.TimeSpentInSeconds);
                data[g.Key] = data[g.Key].Add(TimeSpan.FromSeconds(total));
            }

            foreach (var k in data) {
                Console.WriteLine(k.Key + ": " + k.Value.TotalHours);
            }
        }

        static bool ValidateSprint(Issue i) {
            var sprint = i.CustomFields["Sprint"];
            if (sprint is null || sprint.Values.Length == 0) { return false; }
            return sprint.Values.Count(e => e == "ROGUE") == 1;
        }

        static bool compareDate(DateTime? d1) {
            return DateTime.Compare(d1 ?? DateTime.Now , DateTime.Parse("2019-12-02")) >= 0;
        }
    }
}
