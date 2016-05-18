using System;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Client;
using Stardust.Interstellar.Rest.Extensions;

namespace Stardust.Interstellar.Rest.Test
{
    public class TestClient : RestWrapper, ITestApi
    {


        public TestClient(IAuthenticationHandler authenticationHandler, IHeaderHandlerFactory headerHandlers, TypeWrapper interfaceType)
            : base(authenticationHandler, headerHandlers, interfaceType)
        {

        }

        public string Apply1(string id, string name)
        {
            const string apply = "Apply";
            var par = new object[] { id, name };
            var parameters = GetParameters(apply, par);
            var result = Invoke<string>(apply, parameters);
            return result;
        }

        public string Apply2(string id, string name, string item3)
        {
            const string apply = "Apply";
            var par = new object[] { id, name, item3 };
            var parameters = GetParameters(apply, par);
            var result = Invoke<string>(apply, parameters);
            return result;
        }


        public string Apply3(string id, string name, string item3, string item4)
        {
            const string apply = "Apply";
            var par = new object[] { id, name, item3, item4 };
            var parameters = GetParameters(apply, par);
            var result = Invoke<string>(apply, parameters);
            return result;
        }

        public void Put(string id, DateTime timestamp)
        {
            const string apply = "Put";
            var par = new object[] { id, timestamp };
            var parameters = GetParameters(apply, par);
            InvokeVoid(apply, parameters);
        }

        public Task<StringWrapper> ApplyAsync(string id, string name, string item3, string item4)
        {
            const string apply = "ApplyAsync";
            var par = new object[] { id, name, item3, item4 };
            var parameters = GetParameters(apply, par);
            var result = InvokeAsync<StringWrapper>(apply, parameters);
            return result;
        }

        public Task PutAsync(string id, DateTime timestamp)
        {
            const string apply = "PutAsync";
            var par = new object[] { id, timestamp };
            var parameters = GetParameters(apply, par);
            var result = InvokeAsync<int>(apply, parameters);
            return result;
        }
    }
}