using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Continuum.Master.ControlUnit
{
    public class NodeHostManager : INodeHostManager
    {
        private readonly IDataContextProvider _provider;

        public NodeHostManager(IDataContextProvider provider)
        {
            _provider = provider;
        }
    }
}
