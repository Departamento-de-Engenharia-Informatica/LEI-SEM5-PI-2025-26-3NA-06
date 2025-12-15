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

        public async Task<StorageArea> FindByNameAsync(AreaName name)
        {
            var result = await _context.StorageAreas.FirstOrDefaultAsync(sa => sa.Name.Value == name.Value);
            return result ?? throw new InvalidOperationException("StorageArea not found.");
        }
    }
}
