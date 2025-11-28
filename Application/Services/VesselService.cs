using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Application.Services
{
    public class VesselService
    {
        private readonly IRepository<Vessel, IMO> _vesselRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VesselService> _logger;

        public VesselService(
            IRepository<Vessel, IMO> vesselRepository,
            IUnitOfWork unitOfWork,
            ILogger<VesselService> logger)
        {
            _vesselRepository = vesselRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Vessel>> GetAllAsync()
        {
            _logger.LogInformation("Getting all vessels");
            return await _vesselRepository.GetAllAsync();
        }

        public async Task<Vessel?> GetByImoAsync(string imo)
        {
            _logger.LogInformation("Getting vessel with IMO: {Imo}", imo);
            var vesselId = new IMO(imo);
            return await _vesselRepository.GetByIdAsync(vesselId);
        }

        public async Task<Vessel> CreateAsync(
            string imo,
            string vesselName,
            int maxTeu,
            double size,
            int cargoCapacity,
            Guid ownerId,
            Guid vesselTypeId)
        {
            _logger.LogInformation("Creating vessel with IMO: {Imo}", imo);

            var vessel = new Vessel(
                new IMO(imo),
                ownerId,
                vesselTypeId,
                new VesselName(vesselName),
                new MaxTeu(maxTeu),
                new Size(size),
                new CargoCapacity(cargoCapacity)
            );

            await _vesselRepository.AddAsync(vessel);
            await _unitOfWork.CommitAsync();

            return vessel;
        }

        public async Task<Vessel?> UpdateAsync(
            string imo,
            string vesselName,
            int maxTeu,
            double size,
            int cargoCapacity,
            Guid vesselTypeId)
        {
            _logger.LogInformation("Updating vessel with IMO: {Imo}", imo);

            var vesselId = new IMO(imo);
            var vessel = await _vesselRepository.GetByIdAsync(vesselId);

            if (vessel == null)
            {
                _logger.LogWarning("Vessel with IMO {Imo} not found", imo);
                return null;
            }

            // Create new vessel with updated values (immutable approach)
            var updatedVessel = new Vessel(
                vesselId,
                vessel.OwnerId,
                vesselTypeId,
                new VesselName(vesselName),
                new MaxTeu(maxTeu),
                new Size(size),
                new CargoCapacity(cargoCapacity)
            );

            await _vesselRepository.UpdateAsync(updatedVessel);
            await _unitOfWork.CommitAsync();

            return updatedVessel;
        }

        public async Task<bool> DeleteAsync(string imo)
        {
            _logger.LogInformation("Deleting vessel with IMO: {Imo}", imo);

            var vesselId = new IMO(imo);
            var vessel = await _vesselRepository.GetByIdAsync(vesselId);

            if (vessel == null)
            {
                _logger.LogWarning("Vessel with IMO {Imo} not found", imo);
                return false;
            }

            await _vesselRepository.DeleteAsync(vesselId);
            await _unitOfWork.CommitAsync();

            return true;
        }
    }
}
