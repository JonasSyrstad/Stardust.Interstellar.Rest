using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.Rest.Azure.Authentication;

namespace Continuum.Master.ControlUnit.DeploymentModule
{

    public interface IPublisher { }
    public class AzurePublisher
    {
        private ResourceManagementClient _client;

        public AzurePublisher()
        {


        }

        public async Task Initialize()
        {
            var tenantId = System.Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var clientId = System.Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var secret = System.Environment.GetEnvironmentVariable("AZURE_SECRET");
            var subscriptionId = System.Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");

            var user = await ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, secret);
            _client = new ResourceManagementClient(user);
            _client.SubscriptionId = subscriptionId;
        }

        public async Task CreateAndPublishNode(IStreamNodes node)
        {
            if (!(await _client.ResourceGroups.CheckExistenceAsync("continuumRgCluster")))
            {
                var groupParams = new ResourceGroupInner { Location = "westeurope" };
                var group = await _client.ResourceGroups.CreateOrUpdateAsync("continuumRgCluster", groupParams);
            }
            //var rg = await _client.ResourceGroups.GetAsync("continuumRgCluster");
            var appService=new Microsoft.Azure.Management.ResourceManager.Fluent.Models.GenericResourceInner
            {
                
            };
        }
    }
}
