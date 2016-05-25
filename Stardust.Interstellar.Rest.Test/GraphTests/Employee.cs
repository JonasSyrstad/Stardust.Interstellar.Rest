using System.Threading.Tasks;
using Newtonsoft.Json;
using Stardust.Interstellar.Rest.Client.Graph;

namespace Stardust.Interstellar.Rest.Test
{
    public class Employee
    {
        public string Name { get; set; }

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

        public string ManagerId { get; set; }
    }
}