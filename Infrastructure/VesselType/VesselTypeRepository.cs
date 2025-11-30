using ProjArqsi.Domain.VesselTypeAggregate;
using DomainVesselType = ProjArqsi.Domain.VesselTypeAggregate.VesselType;

namespace ProjArqsi.Infrastructure.VesselType
{
    public class VesselTypeRepository : IVesselTypeRepository
    {
        private readonly List<DomainVesselType> _vesselTypes = new();

        public async Task<DomainVesselType> AddAsync(DomainVesselType entity)
        {
            _vesselTypes.Add(entity);
            return await Task.FromResult(entity);
        }

        public async Task DeleteAsync(VesselTypeId id)
        {
            var vesselType = _vesselTypes.FirstOrDefault(vt => vt.Id.Equals(id));
            if (vesselType != null)
                _vesselTypes.Remove(vesselType);
            
            await Task.CompletedTask;
        }

        public async Task<List<DomainVesselType>> GetAllAsync()
        {
            return await Task.FromResult(_vesselTypes.ToList());
        }

        public async Task<DomainVesselType> GetByIdAsync(VesselTypeId id)
        {
            var vesselType = _vesselTypes.FirstOrDefault(vt => vt.Id.Equals(id));
            return await Task.FromResult(vesselType!);
        }

        public async Task<DomainVesselType> UpdateAsync(DomainVesselType entity)
        {
            var index = _vesselTypes.FindIndex(vt => vt.Id.Equals(entity.Id));
            if (index >= 0)
                _vesselTypes[index] = entity;
            
            return await Task.FromResult(entity);
        }

        // Custom methods from IVesselTypeRepository
        public async Task<DomainVesselType?> FindByNameAsync(TypeName name)
        {
            var vesselType = _vesselTypes.FirstOrDefault(vt => vt.TypeName.Value.Equals(name.Value, StringComparison.OrdinalIgnoreCase));
            return await Task.FromResult(vesselType);
        }

        public async Task<IEnumerable<DomainVesselType>> SearchByNameAsync(string searchTerm)
        {
            var results = _vesselTypes
                .Where(vt => vt.TypeName.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            return await Task.FromResult(results);
        }
    }
}

