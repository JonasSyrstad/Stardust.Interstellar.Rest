using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stardust.Interstellar.Rest.Client.Graph;

namespace Stardust.Interstellar.Rest.Test
{
    public class Employee:GraphBase
    {
        private string name;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                
                name = value;
            }
        }

        public string Email { get; set; }

        [JsonIgnore]
        public Employee Manager
        {
            get
            {
                return new GraphCollection<Employee>(typeof(IEmployeeService))[ManagerId];
            }
        }

        public async Task<Employee> GetManagerAsync()
        {
            return await new GraphCollection<Employee>(typeof(IEmployeeService)).GetAsync(ManagerId);
        }

        public IGraphCollection<Employee> Colleagues
        {
            get
            {
                return CreateGraphCollection<Employee>("colleagues");
            }
        }

        public string ManagerId { get; set; }

        public override IGraphItem Initialize(IGraphItem parent)
        {
            return this;
        }
    }
}