using System.Collections.Generic;
using System.Threading.Tasks;
using Atlassian.Jira;
using jiraps.JiraHandler;
using System;

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

        private static async Task GenerateReportForUser(string user, List<Worklog> worklogs)
        {
            
        }

        private static async Task<Dictionary<string, List<Worklog>>> GetWorklogsByUser()
        {
            var dict = new Dictionary<string, List<Worklog>>();
            var jiraClient = await JiraHandlerService.LoadJiraHandlerService();

            await foreach (var l in jiraClient.GetWorklogsCurrentSprint())
            {
                dict = AddWorklogToDictionary(dict, l);
            }

            return dict;
        }

        private static Dictionary<string, List<Worklog>> AddWorklogToDictionary(Dictionary<string, List<Worklog>> dict, Worklog log)
        {
            var user = log.Author;
            if (dict.ContainsKey(user)) {
                dict[user].Add(log);
            } else {
                dict[user] = new List<Worklog>{ log };
            }
            return dict;
        }
    }
}