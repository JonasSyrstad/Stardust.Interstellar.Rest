using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stardust.Interstellar.Rest.Test
{
    [ServiceContract]
    public interface IWcfWrapper
    {
        [OperationContract]
        [WebGet(UriTemplate = "wcf/test/Get1")]
        StringWrapper TestImplementationGet(string wrapper);


        [OperationContract]
        [WebInvoke(Method = "PUT",UriTemplate = "wcf/test/Put1/{id}")]
        StringWrapper TestImplementationPut(string id, StringWrapper wrapper);
    }

    [IRoutePrefix("api")]
    [CallingMachineName]
    public interface ITestApi
    {
        [Route("test/{id}")]
        [HttpGet]
        string Apply1([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name);

        [Route("test2/{id}")]
        [HttpGet]
        string Apply2([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3);

        [Route("test3/{id}")]
        [HttpGet]
        string Apply3([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Header)]string item3, [In(InclutionTypes.Header)]string item4);

        [Route("put1/{id}")]
        [HttpPut]
        void Put([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);

        [Route("test5/{id}")]
        [HttpGet]
        Task<StringWrapper> ApplyAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)]string name, [In(InclutionTypes.Path)]string item3, [In(InclutionTypes.Path)]string item4);

        [Route("put2/{id}")]
        [HttpPut]
        Task PutAsync([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Body)] DateTime timestamp);
    }
}