using System.Collections.Generic;
using BrightstarDB.EntityFramework;

namespace Continuum.Master.ControlUnit
{
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
}