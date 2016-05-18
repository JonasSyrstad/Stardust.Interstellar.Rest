using System.ServiceModel;
using System.ServiceModel.Web;
using Stardust.Interstellar.Rest.Annotations;

namespace Stardust.Interstellar.Rest.Test
{
    [ServiceContract]
    [PerformanceHeaders]
    public interface IWcfWrapper
    {
        [OperationContract]
        [WebGet(UriTemplate = "wcf/test/Get1")]
        StringWrapper TestImplementationGet(string wrapper);


        [OperationContract]
        [WebInvoke(Method = "PUT",UriTemplate = "wcf/test/Put1/{id}")]
        StringWrapper TestImplementationPut(string id, StringWrapper wrapper);
    }
}