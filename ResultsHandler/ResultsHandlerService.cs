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
        public static async Task GenerateCSVReport() {
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
            var fileName = $"{user}.{start}.{end}.csv";
            
            var groupByIssue = worklogs.GroupBy(w => w.IssueId);
            // MAP THIS RIGHT
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