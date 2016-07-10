using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Service;

namespace Stardust.Interstellar.Rest.Test
{
    [IRoutePrefix("api")]
    [CallingMachineName]
    [PerformanceHeaders]
    [ErrorHandler(typeof(TestHandler))]
    public interface ITestApi
    {
        [Route("test/{id}")]
        [HttpGet]
        string Apply1([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name);

        [Route("test2/{id}")]
        [HttpGet]
        [UseXml]
        Task<StringWrapper> Apply2([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3);

        [Route("test3/{id}")]
        [HttpGet]
        [Authorize]
        string Apply3([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3, [In(InclutionTypes.Header)]string item4);

        [Route("put1/{id}")]
        [HttpPut]
        void Put([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);

        [Route("test5/{id}")]
        [HttpGet]
        Task<StringWrapper> ApplyAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Path)]string item3, [In(InclutionTypes.Path)]string item4);

        [Route("put2/{id}")]
        [HttpPut]
        [ServiceDescription("Sample description", Responses = "404;not found|401;Unauthorized access")]
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
    }
}