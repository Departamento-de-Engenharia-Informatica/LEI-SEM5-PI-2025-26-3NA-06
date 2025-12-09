using ProjArqsi.Domain.VesselTypeAggregate;
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Infrastructure.Shared;

namespace ProjArqsi.Infrastructure.Repositories
{
    public class VesselTypeRepository : BaseRepository<VesselType, VesselTypeId>, IVesselTypeRepository
    {
        private readonly AppDbContext _context;

        public VesselTypeRepository(AppDbContext context) : base(context.VesselTypes)
        {
            _context = context;
        }

        public async Task<VesselType?> FindByNameAsync(TypeName name)
        {
            return await _context.VesselTypes.FirstOrDefaultAsync(vt => vt.TypeName.Value.ToLower() == name.Value.ToLower());
        }

        public async Task<IEnumerable<VesselType>> SearchByNameAsync(string searchTerm)
        {
            return await _context.VesselTypes
                .Where(vt => vt.TypeName.Value.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<VesselType>> SearchByDescriptionAsync(string searchTerm)
        {
            return await _context.VesselTypes
                .Where(vt => vt.TypeDescription.Value.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<VesselType>> SearchByNameOrDescriptionAsync(string searchTerm)
        {
            return await _context.VesselTypes
                .Where(vt => vt.TypeName.Value.Contains(searchTerm) || vt.TypeDescription.Value.Contains(searchTerm))
                .ToListAsync();
        }
    }
}