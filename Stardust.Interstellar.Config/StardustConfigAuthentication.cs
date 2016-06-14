using System;
using System.Configuration;
using System.Net;
using System.Text;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest
{
    internal sealed class StardustConfigAuthentication : IAuthenticationHandler
    {

        internal static string token;

        internal static string tokenKey;
        
        internal static string useToken;
        
        internal static string user;
        
        internal static string password;
        
        internal static string domain;

        internal static bool initialized;

        public void Apply(HttpWebRequest req)
        {
            if(!initialized)
            {
                token=ConfigurationManager.AppSettings["stardust.accessToken"];
                tokenKey = ConfigurationManager.AppSettings["stardust.accessTokenKey"];
                useToken = ConfigurationManager.AppSettings["stardust.useAccessToken"];
                user = ConfigurationManager.AppSettings["stardust.configUser"];
                password = ConfigurationManager.AppSettings["stardust.configPassword"];
                domain = ConfigurationManager.AppSettings["stardust.configDomain"];
                initialized = true;
            }
            if (useToken != "false")
            {
                req.Headers.Add("key",tokenKey);
                req.Headers.Add("Authorization" ,"Token " + Convert.ToBase64String(Encoding.UTF8.GetBytes(token)));
            }
            else
            {
                req.Credentials=new NetworkCredential(user,password,domain);
            }
        }
    }
}