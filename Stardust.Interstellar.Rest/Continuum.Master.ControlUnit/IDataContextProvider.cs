namespace Continuum.Master.ControlUnit
{
    public interface IDataContextProvider
    {
        ContinuumContext Current { get; }
    }
}