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
            var result = await _context.VesselTypes.FirstOrDefaultAsync(vt => vt.TypeName.Value == name.Value);
            return result;
        }

        public async Task<IEnumerable<VesselType>> SearchByNameAsync(TypeName searchTerm)
        {
            return await _context.VesselTypes
                .Where(vt => vt.TypeName.Value.Contains(searchTerm.Value))
                .ToListAsync();
        }

        public async Task<IEnumerable<VesselType>> SearchByDescriptionAsync(TypeDescription searchTerm)
        {
            return await _context.VesselTypes
                .Where(vt => vt.TypeDescription.Value.Contains(searchTerm.Value))
                .ToListAsync();
        }

        public async Task<IEnumerable<VesselType>> SearchByNameOrDescriptionAsync(string searchTerm)
        {
            return await _context.VesselTypes
                .Where(vt => vt.TypeName.Value.Contains(searchTerm) || vt.TypeDescription.Value.Contains(searchTerm))
                .ToListAsync();
        }

        // used to fetch when no updates will be made
        public async Task<VesselType?> GetByIdAsNoTrackingAsync(VesselTypeId id)
        {
            return await _context.VesselTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(vt => vt.Id == id);
        }
    }
}