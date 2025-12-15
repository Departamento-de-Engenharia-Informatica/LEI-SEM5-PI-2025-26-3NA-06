using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public interface IVesselRepository : IRepository<Vessel, IMOnumber>
    {
        Task<Vessel?> FindByNameAsync(VesselName name);
        Task<IEnumerable<Vessel>> SearchByImoAsync(IMOnumber searchTerm);
        Task<IEnumerable<Vessel>> SearchByNameAsync(VesselName searchTerm);
        Task<IEnumerable<Vessel>> SearchByNameOrDescriptionAsync(string searchTerm);
    }
}