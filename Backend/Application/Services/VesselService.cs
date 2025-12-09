using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Application.DTOs;
using AutoMapper;

namespace ProjArqsi.Application.Services
{
    public class VesselService
    {
        private readonly IVesselRepository _vesselRepository;
        private readonly IVesselTypeRepository _vesselTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VesselService(
            IVesselRepository vesselRepository,
            IVesselTypeRepository vesselTypeRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _vesselRepository = vesselRepository;
            _vesselTypeRepository = vesselTypeRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VesselDto>> GetAllAsync()
        {
            var vessels = await _vesselRepository.GetAllAsync();
            var vesselDtos = new List<VesselDto>();

            foreach (var vessel in vessels)
            {
                var vesselType = await _vesselTypeRepository.GetByIdAsync(new VesselTypeId(vessel.VesselTypeId));
                var dto = _mapper.Map<VesselDto>(vessel);
                dto.VesselTypeName = vesselType.TypeName.Value;
                vesselDtos.Add(dto);
            }

            return vesselDtos;
        }

        public async Task<VesselDto> GetByImoAsync(string imo)
        {
            var vesselId = new IMOnumber(imo);
            var vessel = await _vesselRepository.GetByIdAsync(vesselId);

            var vesselType = await _vesselTypeRepository.GetByIdAsync(new VesselTypeId(vessel.VesselTypeId));
            
            var dto = _mapper.Map<VesselDto>(vessel);
            dto.VesselTypeName = vesselType.TypeName.Value;
            return dto;
        }

        public async Task<VesselDto> CreateAsync(
            string imo,
            string vesselName,
            int capacity,
            int rows,
            int bays,
            int tiers,
            double length,
            Guid vesselTypeId)
        {
            // Check if IMO already exists - let repository throw exception
            try
            {
                await _vesselRepository.GetByIdAsync(new IMOnumber(imo));
                throw new BusinessRuleValidationException($"A vessel with IMO number '{imo}' already exists.");
            }
            catch (InvalidOperationException)
            {
                // Vessel doesn't exist, this is expected for creation
            }

            // Validate vessel type exists and check capacity
            var vesselType = await _vesselTypeRepository.GetByIdAsync(new VesselTypeId(vesselTypeId));

            if (capacity > vesselType.TypeCapacity.Value)
            {
                throw new BusinessRuleValidationException(
                    $"Vessel capacity ({capacity}) cannot exceed the vessel type capacity ({vesselType.TypeCapacity.Value}).");
            }

            if (rows > vesselType.MaxRows.Value)
            {
                throw new BusinessRuleValidationException(
                    $"Vessel Rows ({rows}) cannot exceed the vessel type max rows ({vesselType.MaxRows.Value}).");
            }

            if (bays > vesselType.MaxBays.Value)
            {
                throw new BusinessRuleValidationException(
                    $"Vessel Bays ({bays}) cannot exceed the vessel type max bays ({vesselType.MaxBays.Value}).");
            }

            if (tiers > vesselType.MaxTiers.Value)
            {
                throw new BusinessRuleValidationException(
                    $"Vessel Tiers ({tiers}) cannot exceed the vessel type max tiers ({vesselType.MaxTiers.Value}).");
            }

            var vessel = new Vessel(
                new IMOnumber(imo),
                vesselTypeId,
                new VesselName(vesselName),
                new Capacity(capacity),
                new Rows(rows),
                new Bays(bays),
                new Tiers(tiers),
                new Length(length)
            );

            await _vesselRepository.AddAsync(vessel);
            await _unitOfWork.CommitAsync();

            var dto = _mapper.Map<VesselDto>(vessel);
            dto.VesselTypeName = vesselType.TypeName.Value;
            return dto;
        }

        public async Task<VesselDto> UpdateAsync(
            string imo,
            string vesselName,
            int capacity,
            int rows,
            int bays,
            int tiers,
            double length,
            Guid vesselTypeId)
        {
            var vesselId = new IMOnumber(imo);
            var vessel = await _vesselRepository.GetByIdAsync(vesselId);

            // Validate vessel type exists and check capacity
            var vesselType = await _vesselTypeRepository.GetByIdAsync(new VesselTypeId(vesselTypeId));

            if (capacity > vesselType.TypeCapacity.Value)
            {
                throw new BusinessRuleValidationException(
                    $"Vessel capacity ({capacity}) cannot exceed the vessel type capacity ({vesselType.TypeCapacity.Value}).");
            }

            if (rows > vesselType.MaxRows.Value)
            {
                throw new BusinessRuleValidationException(
                    $"Vessel Rows ({rows}) cannot exceed the vessel type max rows ({vesselType.MaxRows.Value}).");
            }

            if (bays > vesselType.MaxBays.Value)
            {
                throw new BusinessRuleValidationException(
                    $"Vessel Bays ({bays}) cannot exceed the vessel type max bays ({vesselType.MaxBays.Value}).");
            }

            if (tiers > vesselType.MaxTiers.Value)
            {
                throw new BusinessRuleValidationException(
                    $"Vessel Tiers ({tiers}) cannot exceed the vessel type max tiers ({vesselType.MaxTiers.Value}).");
            }

            // Update vessel using UpdateDetails method
            vessel.UpdateDetails(
                new VesselName(vesselName),
                new Capacity(capacity),
                new Rows(rows),
                new Bays(bays),
                new Tiers(tiers),
                new Length(length)
            );

            await _vesselRepository.UpdateAsync(vessel);
            await _unitOfWork.CommitAsync();

            var dto = _mapper.Map<VesselDto>(vessel);
            dto.VesselTypeName = vesselType.TypeName.Value;
            return dto;
        }

        public async Task<bool> DeleteAsync(string imo)
        {
            var vesselId = new IMOnumber(imo);
            
            try
            {
                var vessel = await _vesselRepository.GetByIdAsync(vesselId);
                await _vesselRepository.DeleteAsync(vesselId);
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }
}
