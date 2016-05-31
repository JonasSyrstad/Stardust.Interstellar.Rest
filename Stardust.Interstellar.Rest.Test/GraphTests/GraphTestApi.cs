using System;
using System.Collections.Generic;
using System.Text;
using Stardust.Interstellar.Rest.Client.Graph;

namespace Stardust.Interstellar.Rest.Test
{
    public class GraphTestApi : GraphContext<Employee>
    {
        public GraphTestApi(string baseUrl) : base(baseUrl)
        {
            Id = "jonassyrstad@outlook.com";

        }
        public IGraphCollection<Employee> Employees
        {
            get
            {
                return CreateGraphCollection<Employee>();
            }
        }

        public IGraphItem<Employee> Me
        {
            get
            {
                return CreateGraphItem<Employee>(Id);
            }
        }
    }
}
