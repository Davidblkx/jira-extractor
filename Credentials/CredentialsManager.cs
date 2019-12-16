

namespace jiraps.Credentials
{
    public class CredentialsManager
    {
        public JiraServer GetCredentials() {
            // TOKEN from: https://id.atlassian.com/manage/api-tokens
            return new JiraServer {
                User = "",
                Token = "",
                ServerUrl = "",
            };
        }
    }
}