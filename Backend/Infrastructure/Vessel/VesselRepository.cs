using ProjArqsi.Domain.VesselAggregate;
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Infrastructure.Shared;

namespace ProjArqsi.Infrastructure.Repositories
{
    public class VesselRepository : BaseRepository<Vessel, IMOnumber>, IVesselRepository
    {
        private readonly AppDbContext _context;

        public VesselRepository(AppDbContext context) : base(context.Vessels)
        {
            _context = context;
        }

        public async Task<Vessel?> FindByNameAsync(VesselName name)
        {
            return await _context.Vessels.FirstOrDefaultAsync(v => v.VesselName.Name.Equals(name.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        public async Task<IEnumerable<Vessel>> SearchByImoAsync(IMOnumber searchTerm)
        {
            return await _context.Vessels
                .Where(v => v.Id.AsString().Contains(searchTerm.AsString()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Vessel>> SearchByNameAsync(VesselName searchTerm)
        {
            return await _context.Vessels
                .Where(v => v.VesselName.Name.Contains(searchTerm.Name))
                .ToListAsync();
        }

        public async Task<IEnumerable<Vessel>> SearchByNameOrDescriptionAsync(string searchTerm)
        {
            return await _context.Vessels
                .Where(v => v.VesselName.Name.Contains(searchTerm) || v.Id.AsString().Contains(searchTerm))
                .ToListAsync();
        }
    }
}