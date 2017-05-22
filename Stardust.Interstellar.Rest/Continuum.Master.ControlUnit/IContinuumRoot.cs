using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightstarDB.EntityFramework;

namespace Continuum.Master.ControlUnit
{
    [Entity]
    public interface IContinuumRoot
    {
        [Identifier(KeySeparator = "", KeyProperties = new[] { "OwnerOrganizationName" })]
        string Id { get; }

        string OwnerOrganizationName { get; set; }

        string AzureDirectoryId{ get; set; }

        string OwnAppId { get; set; }

        string JoinKey { get; set; }

        ICollection<IStreamNodes> Nodes { get; set; }
    }
}
