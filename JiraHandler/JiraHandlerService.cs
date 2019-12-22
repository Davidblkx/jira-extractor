using Atlassian.Jira;
using jiraps.Credentials;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace jiraps.JiraHandler
{
    public class JiraHandlerService
    {
        private readonly Jira _jiraClient;
        private static readonly string DEFAULT_SPRINT = "UNKOWN";
        private static string _sprint = DEFAULT_SPRINT;

        private static DateTime _sprintStart = DateTime.Now;
        private static DateTime _sprintEnd = DateTime.Now;
        private static bool _sprintDatesInit = false;

        public JiraHandlerService(Jira client)
        {
            _jiraClient = client;
        }

        public static async Task<JiraHandlerService> LoadJiraHandlerService()
        {
            var credentials = await CredentialsManager.GetCredentials();
            var client = Jira.CreateRestClient(credentials.ServerUrl, credentials.User, credentials.Token);
            client.Issues.MaxIssuesPerRequest = 2500;
            return new JiraHandlerService(client);
        }

        public static (DateTime startSprint, DateTime endSprint) GetSprintDate()
        {
            if (!_sprintDatesInit) {
                _sprintStart = GetDateFromConsole("Sprint start date");
                _sprintEnd = GetDateFromConsole("Sprint end date");
                _sprintDatesInit = true;
            }

            return (_sprintStart, _sprintEnd);
        }

        public static void SetSprintDate(DateTime start, DateTime end) {
             if (!_sprintDatesInit) {
                _sprintStart = start;
                _sprintEnd = end;
                _sprintDatesInit = true;
            }
        }

        public string GetCurrentSprint() {
            if (_sprint != DEFAULT_SPRINT) return _sprint;
            Console.WriteLine("Please insert the current sprint:");
            _sprint = Console.ReadLine();
            return _sprint;
        }

        public static void SetCurrentSprint(string sprint) {
            _sprint = sprint;
        }

        public async Task<Project> GetProject() {
            var proj =  (await _jiraClient.Projects.GetProjectsAsync())
                .FirstOrDefault();
            if (proj is null) throw new Exception("No project was found");
            return proj;
        }

        public async Task<IEnumerable<Issue>> GetTaskCurrentSprint() {
            var sprint = GetCurrentSprint();
            var project = (await GetProject()).Key;

            var JQL = $"project = {project} AND Sprint = \"{sprint}\"";
            return (await _jiraClient.Issues.GetIssuesFromJqlAsync(JQL)).AsEnumerable();
        }

        public async IAsyncEnumerable<IssueWorklog> GetWorklogsCurrentSprint() {
            var (startDate, endDate) = GetSprintDate();
            var issues = await GetTaskCurrentSprint();
            foreach (var i in issues)
            {
                var logs = (await i.GetWorklogsAsync())
                    .Where(l => DateTime.Compare(l.StartDate ?? DateTime.MinValue, startDate) >= 0)
                    .Where(l => DateTime.Compare(l.StartDate ?? DateTime.MaxValue, endDate) <= 0);
                foreach (var l in logs)
                {
                    yield return new IssueWorklog(l, i.Key.Value);
                }
            }
        }

        private static DateTime GetDateFromConsole(string question) {
            Console.WriteLine(question + ": (YYYY-MM-DD)");
            var dateStr = Console.ReadLine();
            if (!DateTime.TryParse(dateStr, out var date)) {
                Console.WriteLine("Invalid date, try gain!");
                return GetDateFromConsole(question);
            }
            return date;
        }
    }
}