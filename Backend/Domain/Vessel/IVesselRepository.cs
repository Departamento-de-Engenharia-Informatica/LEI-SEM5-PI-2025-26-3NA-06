using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselAggregate
{
    public interface IVesselRepository : IRepository<Vessel, IMOnumber>
    {
        Task<Vessel> FindByNameAsync(VesselName name);
        Task<IEnumerable<Vessel>> SearchByImoAsync(string searchTerm);
        Task<IEnumerable<Vessel>> SearchByNameAsync(string searchTerm);
        Task<IEnumerable<Vessel>> SearchByNameOrDescriptionAsync(string searchTerm);
    }
}