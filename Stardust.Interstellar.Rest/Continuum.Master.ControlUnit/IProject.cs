using System.Collections.Generic;
using BrightstarDB.EntityFramework;

namespace Continuum.Master.ControlUnit
{
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
}