using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace jiraps.Credentials
{
    public class CredentialsManager
    {
        private static readonly string FILE_NAME = "credentials.pass";

        public static Task<JiraServer> GetCredentials() {
            return (new CredentialsManager()).LoadJiraCredentials();
        }

        private async Task<JiraServer> LoadJiraCredentials() {
            var (exist, server) = await GetCredentialsFromFile();
            if (exist) return server;

            return await GetCredentialsFromConsole();
        }

        private async Task<JiraServer> GetCredentialsFromConsole() {
            Console.WriteLine("Server not defined, please fill some information:");
            Console.WriteLine("Server url: (https://[name].atlassian.net)");
            var server = Console.ReadLine();
            Console.WriteLine("Username: (email)");
            var user = Console.ReadLine();
            Console.WriteLine("Token? (https://id.atlassian.com/manage/api-tokens)");
            var pass = Console.ReadLine();

            var jira = new JiraServer
            {
                ServerUrl = server,
                User = user,
                Token = pass,
            };

            await SaveFile(jira);
            return jira;
        }

        private async Task<(bool success, JiraServer server)> GetCredentialsFromFile() {
            var (exist, file) = await ReadFile();
            if (exist) {
                return (true, JsonSerializer.Deserialize<JiraServer>(file));
            }

            return (false, new JiraServer());
        }

        private async Task<(bool exist, string file)> ReadFile() {
            if(!File.Exists(FILE_NAME)) {
                return (false, "");
            }

            var text = await File.ReadAllTextAsync(FILE_NAME);
            return (true, text);
        }

        private async Task SaveFile(JiraServer server) {
            var json = JsonSerializer.Serialize(server);
            await File.WriteAllTextAsync(FILE_NAME, json);
        }
    }
}