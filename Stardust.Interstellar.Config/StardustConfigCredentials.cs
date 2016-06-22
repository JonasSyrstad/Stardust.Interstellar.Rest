namespace Stardust.Interstellar.Rest
{
    public static class StardustConfigCredentials
    {
        public static void SetUsername(string username, string password)
        {
            SetUsername(username, password, null);
        }

        public static void SetUsername(string username, string password, string domain)
        {
            StardustConfigAuthentication.user = username;
            StardustConfigAuthentication.password = password;
            StardustConfigAuthentication.domain = domain;
            StardustConfigAuthentication.useToken = "false";
            StardustConfigAuthentication.initialized = true;
        }

        public static void SetApiToken(string token, string tokenKey)
        {
            StardustConfigAuthentication.useToken = "true";
            StardustConfigAuthentication.token = token;
            StardustConfigAuthentication.tokenKey = tokenKey;
            StardustConfigAuthentication.initialized=true;
        }
    }
}