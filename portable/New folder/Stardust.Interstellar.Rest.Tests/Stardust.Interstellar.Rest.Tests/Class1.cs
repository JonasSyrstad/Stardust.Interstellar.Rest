using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Client;
using Xunit;
using Xunit.Abstractions;
using Stardust.Interstellar.Rest.Annotations;

namespace Stardust.Interstellar.Rest.Tests
{
    public class Class1
    {
        private readonly ITestOutputHelper output;

        public Class1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test()
        {
            ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/");
        }
    }
    [IRoutePrefix("api")]
    [CircuitBreaker(50, 1)]
    public interface ITestApi
    {
        [Route("test/{id}")]
        [HttpGet]
        string Apply1([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name);

        [Route("test2/{id}")]
        [HttpGet]
        Task<StringWrapper> Apply2([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3);

        [Route("test3/{id}")]
        [HttpGet]
        [AuthorizeWrapper(null)]
        string Apply3([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3, [In(InclutionTypes.Header)]string item4);

        [Route("put1/{id}")]
        [HttpPut]
        void Put([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);


        [Route("test5/{id}")]
        [HttpGet]
        [Retry(10, 3, false, ErrorCategorizer = typeof(ErrorCategorizer))]
        [CallingMachineName]
        Task<StringWrapper> ApplyAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Path)]string item3, [In(InclutionTypes.Path)]string item4);

        [Route("put2/{id}")]
        [HttpPut]
        Task PutAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);

        [Route("failure/{id}")]
        [HttpPut]
        Task FailingAction([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] string timestamp);

        [Route("opt")]
        [HttpOptions]
        Task<List<string>> GetOptions();

        [Route("head")]
        [HttpHead]
        Task GetHead();


        [Route("tr")]
        [HttpGet]
        [Throttling(-1)]
        Task Throttled();
    }
}
