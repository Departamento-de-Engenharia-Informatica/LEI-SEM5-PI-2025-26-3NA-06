using ProjArqsi.Domain.ContainerAggregate;
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Infrastructure.Shared;

namespace ProjArqsi.Infrastructure.Repositories
{
    public class ContainerRepository : BaseRepository<Container, ContainerId>, IContainerRepository
    {
        private readonly AppDbContext _context;

        public ContainerRepository(AppDbContext context) : base(context.Containers)
        {
            _context = context;
        }

        public async Task<Container?> GetByIsoCodeAsync(IsoCode isoCode)
        {
            return await _context.Containers
                .FirstOrDefaultAsync(c => c.IsoCode.Value == isoCode.Value);
        }

        public async Task<bool> ExistsByIsoCodeAsync(IsoCode isoCode)
        {
            return await _context.Containers
                .AnyAsync(c => c.IsoCode.Value == isoCode.Value);
        }
    }
}
