using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public interface IVesselRepository : IRepository<Vessel, VesselId>
    {
        Task<Vessel?> FindByNameAsync(VesselName name);
        Task<Vessel?> GetByImoAsync(IMOnumber imo);
        Task<IEnumerable<Vessel>> SearchByNameAsync(VesselName searchTerm);
        Task<IEnumerable<Vessel>> SearchByNameOrImoAsync(string searchTerm);
    }
}