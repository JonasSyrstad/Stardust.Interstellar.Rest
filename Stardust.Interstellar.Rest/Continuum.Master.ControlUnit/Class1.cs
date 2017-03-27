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

    [Entity]
    public interface IStreamNodes
    {
        [Identifier(KeySeparator = "", KeyProperties = new[] { "Name" })]
        string Id { get; }

        string Name { get; set; }

        string AppId { get; set; }

        [InverseProperty("Nodes")]
        IContinuumRoot Root { get; set; }

        ICollection<IProject> Projects { get; set; }
    }

    [Entity]
    public interface IProject
    {
        [Identifier(KeySeparator = "", KeyProperties = new[] { "Name" })]
        string Id { get; }

        string Name { get; set; }

        string MasterApiKey { get; set; }

        [PropertyType("relative")]
        [InverseProperty("ParentProject")]
        ICollection<IEnvironment> Environments { get; set; }
    }

    [Entity]
    public interface IEnvironment
    {
        [Identifier(KeySeparator = "",KeyProperties = new []{"Name"})]
        string Id { get; }

        string Name { get; set; }

        string ApiKey { get; set; }

        IProject ParentProject { get; set; }


    }
}
