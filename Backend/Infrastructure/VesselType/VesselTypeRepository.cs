using ProjArqsi.Domain.VesselTypeAggregate;
using Microsoft.EntityFrameworkCore;


namespace ProjArqsi.Infrastructure.Repositories
{
    public class VesselTypeRepository : IVesselTypeRepository
    {
        private readonly AppDbContext _context;

        public VesselTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<VesselType> AddAsync(VesselType entity)
        {
            _context.VesselTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(VesselTypeId id)
        {
            var vesselType = await _context.VesselTypes.FirstOrDefaultAsync(vt => vt.Id.Equals(id));
            if (vesselType != null)
            {
                _context.VesselTypes.Remove(vesselType);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<VesselType>> GetAllAsync()
        {
            return await _context.VesselTypes.ToListAsync();
        }

        public async Task<VesselType?> GetByIdAsync(VesselTypeId id)
        {
            return await _context.VesselTypes.FirstOrDefaultAsync(vt => vt.Id.Equals(id));
        }

        public async Task<VesselType> UpdateAsync(VesselType entity)
        {
            _context.VesselTypes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
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

