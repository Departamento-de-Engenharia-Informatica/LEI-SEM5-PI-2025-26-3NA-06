using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Infrastructure.Shared;

namespace ProjArqsi.Infrastructure.Repositories
{
    public class DockRepository : BaseRepository<Dock, DockId>, IDockRepository
    {
        private readonly AppDbContext _context;

        public DockRepository(AppDbContext context) : base(context.Docks)
        {
            _context = context;
        }

        public async Task<Dock?> FindByNameAsync(DockName name)
        {
            return await _context.Docks.FirstOrDefaultAsync(d => d.DockName.Value == name.Value);
        }
    }
}
