using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Stardust.Particles;

namespace Continuum.Master.ControlGraph
{
    public class GraphManager : IGraphManager
    {
        private DocumentClient _client;



        public GraphManager()
        {

            _client = new DocumentClient(new Uri(endpoint),authKey,new ConnectionPolicy {ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp});
        }

        public string endpoint { get{ConfigurationManagerHelper.}}
    }

    public interface IGraphManager
    {
    }
}
