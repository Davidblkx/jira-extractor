using System.Collections.Generic;
using System.Threading.Tasks;
using Atlassian.Jira;
using jiraps.JiraHandler;
using System;
using System.IO;
using System.Linq;

namespace jiraps.ResultsHandler
{
    public class ResultsHandlerService
    {
        private static string serverUrl = "";

        public static async Task GenerateCSVReport() {
            serverUrl = (await jiraps.Credentials.CredentialsManager.GetCredentials()).ServerUrl;
            Console.WriteLine("Loading worklog from current sprint...");
            var data = await GetWorklogsByUser();

            foreach(var u in data.Keys) {
                Console.WriteLine("Generating report for user: " + u);
                await GenerateReportForUser(u, data[u]);
            }
        }

        private static async Task GenerateReportForUser(string user, List<IssueWorklog> worklogs)
        {
            var (start, end) = JiraHandlerService.GetSprintDate();
            var fileName = $"{user}.{start.Year}.{start.Month}.from{start.Day}.to{end.Day}.csv";
            var csvRows = new List<string>();
            
            csvRows.Add(BuildCSVHeader(start, end));
            var groupByIssue = worklogs.GroupBy(w => w.IssueId);
            foreach (var grp in groupByIssue) {
                csvRows.Add(BuildCSVRow(grp.Key, start, end, grp));
            }
            csvRows.Add(BuildCSVRowTotal(start, end, worklogs));
            csvRows.Add("total" + "," + CalcTotalHours(worklogs));

            await File.WriteAllLinesAsync(fileName, csvRows);
        }

        private static string BuildCSVHeader(DateTime start, DateTime end)
        {
            var csvRow = "Issue";
            if (start > end) { return csvRow; }
            for (var d = start; d <= end; d = d.AddDays(1)) {
                csvRow += "," + d.ToShortDateString();
            }
            return csvRow;
        }

        private static string BuildCSVRow(string issueId, DateTime start, DateTime end, IEnumerable<IssueWorklog> worklogs)
        {
            var csvRow = serverUrl + "/browse/" + issueId;
            if (start > end) { return csvRow; }
            for (var d = start; d <= end; d = d.AddDays(1)) {
                var logs = worklogs.Where(w => MatchDate(w.StartDate, d));
                var total = CalcTotalHours(logs);
                csvRow += $",{total}";
            }
            return csvRow;
        }

        private static string BuildCSVRowTotal(DateTime start, DateTime end, IEnumerable<IssueWorklog> worklogs)
        {
            var csvRow = "total day";
            if (start > end) { return csvRow; }
            for (var d = start; d <= end; d = d.AddDays(1)) {
                var logs = worklogs.Where(w => MatchDate(w.StartDate, d));
                var total = CalcTotalHours(logs);
                csvRow += $",{total}";
            }
            return csvRow;
        }

        private static string CalcTotalHours(IEnumerable<IssueWorklog> logs)
        {
            var sum = logs.Sum(w => w.TimeSpentInSeconds);
            var hours = TimeSpan.FromSeconds(sum);
            return $"{Math.Round(hours.TotalHours, 2)}".Replace(",", ".");
        }

        private static bool MatchDate(DateTime? d1, DateTime d2) {
            if (!d1.HasValue) return false;
            return d1.Value.Year == d2.Year
                && d1.Value.Month == d2.Month
                && d1.Value.Day == d2.Day;
        }

        private static long CalculateHours(IEnumerable<Worklog> worklogs, DateTime date)
        {
            bool matchDate(DateTime? toMatch) {
                return date.Day == toMatch?.Day
                    && date.Month == toMatch?.Month
                    && date.Year == toMatch?.Year;
            }
            var totalSeconds = worklogs.Where(w => matchDate(w.StartDate))
                .Sum(w => w.TimeSpentInSeconds);
            
            return totalSeconds / 60 / 60;
        }

        private static async Task<Dictionary<string, List<IssueWorklog>>> GetWorklogsByUser()
        {
            var dict = new Dictionary<string, List<IssueWorklog>>();
            var jiraClient = await JiraHandlerService.LoadJiraHandlerService();

            await foreach (var l in jiraClient.GetWorklogsCurrentSprint())
            {
                dict = AddWorklogToDictionary(dict, l);
            }

            return dict;
        }

        private static Dictionary<string, List<IssueWorklog>> AddWorklogToDictionary(Dictionary<string, List<IssueWorklog>> dict, IssueWorklog log)
        {
            var user = log.Author;
            if (dict.ContainsKey(user)) {
                dict[user].Add(log);
            } else {
                dict[user] = new List<IssueWorklog>{ log };
            }
            return dict;
        }

        private static dynamic BuildCSVRow() {
            return new {};
        } 
    }
}