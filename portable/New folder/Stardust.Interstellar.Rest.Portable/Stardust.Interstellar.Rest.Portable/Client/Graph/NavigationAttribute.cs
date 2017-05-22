using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest.Client.Graph
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NavigationAttribute:Attribute
    {
        private  Type accessorServiceInterface;
        

        private  string idParameter;

        public NavigationAttribute(Type accessorServiceInterface)
        {
            this.accessorServiceInterface = accessorServiceInterface;
        }

        public NavigationAttribute(Type accessorServiceInterface, string idParameter)
        {
            this.accessorServiceInterface = accessorServiceInterface;
            this.idParameter = idParameter;
        }

        public Type AccessorServiceInterface
        {
            get
            {
                return accessorServiceInterface;
            }
            set
            {
                accessorServiceInterface = value;
            }
        }

        public string IdParameter
        {
            get
            {
                return idParameter;
            }
            set
            {
                idParameter = value;
            }
        }

        public string ServiceRootKey { get; set; }
    }
}
