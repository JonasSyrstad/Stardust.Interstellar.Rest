using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stardust.Interstellar.Rest.Client.Graph;

namespace Stardust.Interstellar.Rest.Test
{
    public class Employee : GraphBase
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
                return CreateGraphItem<Employee>(ManagerId).Value;
            }
        }

        public async Task<Employee> GetManagerAsync()
        {
            return await CreateGraphItem<Employee>(ManagerId).GetAsync();
        }

        [JsonIgnore]
        public IGraphCollection<Employee> Colleagues
        {
            get
            {
                
                return CreateGraphCollection<Employee>("colleagues", name);
            }
        }

        public string ManagerId { get; set; }
    }
}