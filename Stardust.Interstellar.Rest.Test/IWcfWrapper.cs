using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Stardust.Interstellar.Rest.Annotations;
using Stardust.Interstellar.Rest.Annotations.Messaging;

namespace Stardust.Interstellar.Rest.Test
{
    [ServiceContract]
    [PerformanceHeaders]
    public interface IWcfWrapper : IServiceWithGlobalParameters
    {
        [OperationContract]
        [WebGet(UriTemplate = "wcf/test/Get1")]
        StringWrapper TestImplementationGet(string wrapper);


        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "wcf/test/Put1/{id}")]
        StringWrapper TestImplementationPut(string id, StringWrapper wrapper);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "wcf/test/Put2/{id}")]
        StringWrapper TestImplementationPut2([In(InclutionTypes.Path)]string id, [ExtensionLevel(3)][In(InclutionTypes.Body)]IDictionary<string,IEnumerable<object>> hierarcy);
    }
}