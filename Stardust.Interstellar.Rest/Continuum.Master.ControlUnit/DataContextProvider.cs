namespace Continuum.Master.ControlUnit
{
    class DataContextProvider : IDataContextProvider
    {
        private ContinuumContext _context;

        public ContinuumContext Current => _context ?? (_context = new ContinuumContext());
    }
}