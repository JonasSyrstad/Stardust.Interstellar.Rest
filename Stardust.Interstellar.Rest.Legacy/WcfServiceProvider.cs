using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Stardust.Interstellar.Rest.Common;

namespace Stardust.Interstellar.Rest.Legacy
{
    public static class WcfServiceProvider
    {
        public static void RegisterWcfAdapters()
        {
            ExtensionsFactory.SetServiceLocator(new WcfRouteResolver());
        }
    }

}
