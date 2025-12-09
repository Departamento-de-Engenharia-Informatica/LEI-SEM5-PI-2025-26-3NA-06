using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.DockAggregate
{
    public interface IDockRepository : IRepository<Dock, DockId>
    {
        // All basic CRUD methods are inherited from IRepository
        // Add any Dock-specific methods here if needed
    }
}
