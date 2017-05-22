using BrightstarDB.EntityFramework;

namespace Continuum.Master.ControlUnit
{
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