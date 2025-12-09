using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Infrastructure.Shared;

namespace ProjArqsi.Infrastructure.Repositories
{
    public class DockRepository : BaseRepository<Dock, DockId>, IDockRepository
    {
        public DockRepository(AppDbContext context) : base(context.Docks)
        {
        }
    }
}
