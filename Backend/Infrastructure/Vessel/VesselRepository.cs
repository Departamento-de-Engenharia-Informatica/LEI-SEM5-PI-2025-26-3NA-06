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

        public async Task<Vessel> FindByNameAsync(VesselName name)
        {
            return await _context.Vessels.FirstOrDefaultAsync(v => v.VesselName.Name.Equals(name.Name, StringComparison.CurrentCultureIgnoreCase)) 
                ?? throw new InvalidOperationException($"Vessel with name '{name.Name}' not found.");
        }

        public async Task<IEnumerable<Vessel>> SearchByImoAsync(string searchTerm)
        {
            return await _context.Vessels
                .Where(v => v.Id.AsString().Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<Vessel>> SearchByNameAsync(string searchTerm)
        {
            return await _context.Vessels
                .Where(v => v.VesselName.Name.Contains(searchTerm))
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