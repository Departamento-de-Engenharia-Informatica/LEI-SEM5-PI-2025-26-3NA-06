using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Infrastructure.Shared;

namespace ProjArqsi.Infrastructure
{
    public class StorageAreaRepository : BaseRepository<StorageArea, StorageAreaId>, IStorageAreaRepository
    {
        private readonly AppDbContext _context;

        public StorageAreaRepository(AppDbContext context) : base(context.StorageAreas)
        {
            _context = context;
        }

        public async Task<StorageArea?> FindByNameAsync(AreaName name)
        {
            return await _context.StorageAreas.FirstOrDefaultAsync(d => d.Name.Value == name.Value);
        }
    }
}
