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
        private string _sprint = DEFAULT_SPRINT;

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

        public string GetCurrentSprint() {
            if (_sprint != DEFAULT_SPRINT) return _sprint;
            Console.WriteLine("Please insert the current sprint:");
            _sprint = Console.ReadLine();
            return _sprint;
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

        public async IAsyncEnumerable<Worklog> GetWorklogsCurrentSprint() {
            var issues = await GetTaskCurrentSprint();
            foreach (var i in issues)
            {
                var logs = await i.GetWorklogsAsync();
                foreach (var l in logs)
                {
                    yield return l;
                }
            }
        }
    }
}