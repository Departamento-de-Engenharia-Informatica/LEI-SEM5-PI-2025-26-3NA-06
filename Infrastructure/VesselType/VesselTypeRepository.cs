using ProjArqsi.Domain.VesselTypeAggregate;
using Microsoft.EntityFrameworkCore;
using DomainVesselType = ProjArqsi.Domain.VesselTypeAggregate.VesselType;

namespace ProjArqsi.Infrastructure.Repositories
{
    public class VesselTypeRepository : IVesselTypeRepository
    {
        private readonly AppDbContext _context;

        public VesselTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DomainVesselType> AddAsync(DomainVesselType entity)
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

        public async Task<List<DomainVesselType>> GetAllAsync()
        {
            return await _context.VesselTypes.ToListAsync();
        }

        public async Task<DomainVesselType> GetByIdAsync(VesselTypeId id)
        {
            return await _context.VesselTypes.FirstOrDefaultAsync(vt => vt.Id.Equals(id));
        }

        public async Task<DomainVesselType> UpdateAsync(DomainVesselType entity)
        {
            _context.VesselTypes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<DomainVesselType?> FindByNameAsync(TypeName name)
        {
            return await _context.VesselTypes.FirstOrDefaultAsync(vt => vt.TypeName.Value.ToLower() == name.Value.ToLower());
        }

        public async Task<IEnumerable<DomainVesselType>> SearchByNameAsync(string searchTerm)
        {
            return await _context.VesselTypes
                .Where(vt => vt.TypeName.Value.ToLower().Contains(searchTerm.ToLower()))
                .ToListAsync();
        }
    }
}

