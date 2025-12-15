using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselTypeAggregate
{
    public interface IVesselTypeRepository : IRepository<VesselType, VesselTypeId>
    {
        Task<VesselType> FindByNameAsync(TypeName name);
        Task<IEnumerable<VesselType>> SearchByNameAsync(TypeName searchTerm);
        Task<IEnumerable<VesselType>> SearchByDescriptionAsync(TypeDescription searchTerm);
        Task<IEnumerable<VesselType>> SearchByNameOrDescriptionAsync(string searchTerm);
    }
}