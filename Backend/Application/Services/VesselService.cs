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
                var vesselType = await _vesselTypeRepository.GetByIdAsync(vessel.VesselTypeId) ?? throw new BusinessRuleValidationException($"Vessel type with ID '{vessel.VesselTypeId.Value}' does not exist.");
                var dto = _mapper.Map<VesselDto>(vessel);
                dto.VesselTypeName = vesselType.TypeName.Value;
                vesselDtos.Add(dto);
            }

            return vesselDtos;
        }

        public async Task<VesselDto> GetByIdAsync(Guid id)
        {
            var vessel = await _vesselRepository.GetByIdAsync(new VesselId(id)) ?? throw new InvalidOperationException($"Vessel with ID '{id}' does not exist.");
            var dto = _mapper.Map<VesselDto>(vessel);
            
            // Get vessel type name
            var vesselType = await _vesselTypeRepository.GetByIdAsync(vessel.VesselTypeId) 
                ?? throw new BusinessRuleValidationException($"Vessel type with ID '{vessel.VesselTypeId.Value}' does not exist.");
            dto.VesselTypeName = vesselType.TypeName.Value;
            
            return dto;
        }

        public async Task<VesselDto> GetByImoAsync(string imo)
        {
            var imoNumber = new IMOnumber(imo);
            var vessel = await _vesselRepository.GetByImoAsync(imoNumber) 
                ?? throw new InvalidOperationException($"Vessel with IMO number '{imo}' does not exist.");
            var dto = _mapper.Map<VesselDto>(vessel);
            
            // Get vessel type name
            var vesselType = await _vesselTypeRepository.GetByIdAsync(vessel.VesselTypeId) 
                ?? throw new BusinessRuleValidationException($"Vessel type with ID '{vessel.VesselTypeId.Value}' does not exist.");
            dto.VesselTypeName = vesselType.TypeName.Value;
            
            return dto;
        }

        public async Task<VesselDto> CreateAsync(UpsertVesselDto dto)
        {
            var imoNumber = new IMOnumber(dto.Imo);

            // Validate unique IMO
            var existingVessel = await _vesselRepository.GetByImoAsync(imoNumber);
            if (existingVessel != null) throw new InvalidOperationException($"Vessel with IMO number '{dto.Imo}' already exists.");

            var vesselName = new VesselName(dto.VesselName);
            var capacity = new Capacity(dto.Capacity);
            var rows = new Rows(dto.Rows);
            var bays = new Bays(dto.Bays);
            var tiers = new Tiers(dto.Tiers);
            var length = new Length(dto.Length);
            var vesselTypeId = new VesselTypeId(Guid.Parse(dto.VesselTypeId));

            // Validate vessel type exists and check constraints BEFORE creating vessel
            var vesselType = await _vesselTypeRepository.GetByIdAsNoTrackingAsync(vesselTypeId)
                ?? throw new BusinessRuleValidationException($"Vessel type with ID '{vesselTypeId.Value}' does not exist.");
            
            // Validate all constraints in service layer
            if (capacity.Value > vesselType.TypeCapacity.Value)
                throw new BusinessRuleValidationException($"Vessel capacity ({capacity.Value}) cannot exceed the vessel type capacity ({vesselType.TypeCapacity.Value}).");

            if (rows.Value > vesselType.MaxRows.Value)
                throw new BusinessRuleValidationException($"Vessel Rows ({rows.Value}) cannot exceed the vessel type max rows ({vesselType.MaxRows.Value}).");

            if (bays.Value > vesselType.MaxBays.Value)
                throw new BusinessRuleValidationException($"Vessel Bays ({bays.Value}) cannot exceed the vessel type max bays ({vesselType.MaxBays.Value}).");

            if (tiers.Value > vesselType.MaxTiers.Value)
                throw new BusinessRuleValidationException($"Vessel Tiers ({tiers.Value}) cannot exceed the vessel type max tiers ({vesselType.MaxTiers.Value}).");
            
            // All validations passed, create vessel with just the ID reference
            var vessel = new Vessel(imoNumber, vesselTypeId, vesselName, capacity, rows, bays, tiers, length);

            await _vesselRepository.AddAsync(vessel);
            await _unitOfWork.CommitAsync();

            var result = _mapper.Map<VesselDto>(vessel);
            result.VesselTypeName = vesselType.TypeName.Value;
            return result;
        }

        public async Task<VesselDto> UpdateAsync(Guid id, UpsertVesselDto dto)
        {
            
            var vessel = await _vesselRepository.GetByIdAsync(new VesselId(id))
                ?? throw new InvalidOperationException($"Vessel with ID '{id}' does not exist.");

            // Validate that vessel type exists and check capacity constraints
            var vesselType = await _vesselTypeRepository.GetByIdAsNoTrackingAsync(vessel.VesselTypeId) 
                ?? throw new BusinessRuleValidationException($"Vessel type with ID '{vessel.VesselTypeId.Value}' does not exist.");
            
            var vesselName = new VesselName(dto.VesselName);
            var capacity = new Capacity(dto.Capacity);
            var rows = new Rows(dto.Rows);
            var bays = new Bays(dto.Bays);
            var tiers = new Tiers(dto.Tiers);
            var length = new Length(dto.Length);
            
            // Validate all constraints in service layer BEFORE updating
            if (capacity.Value > vesselType.TypeCapacity.Value)
                throw new BusinessRuleValidationException($"Vessel capacity ({capacity.Value}) cannot exceed the vessel type capacity ({vesselType.TypeCapacity.Value}).");

            if (rows.Value > vesselType.MaxRows.Value)
                throw new BusinessRuleValidationException($"Vessel Rows ({rows.Value}) cannot exceed the vessel type max rows ({vesselType.MaxRows.Value}).");

            if (bays.Value > vesselType.MaxBays.Value)
                throw new BusinessRuleValidationException($"Vessel Bays ({bays.Value}) cannot exceed the vessel type max bays ({vesselType.MaxBays.Value}).");

            if (tiers.Value > vesselType.MaxTiers.Value)
                throw new BusinessRuleValidationException($"Vessel Tiers ({tiers.Value}) cannot exceed the vessel type max tiers ({vesselType.MaxTiers.Value}).");
            
            vessel.UpdateDetails(vesselName, capacity, rows, bays, tiers, length);
             
            await _vesselRepository.UpdateAsync(vessel);
            await _unitOfWork.CommitAsync();

            var result = _mapper.Map<VesselDto>(vessel);
            result.VesselTypeName = vesselType.TypeName.Value;
            return result;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {   
            try
            {
                var vessel = await _vesselRepository.GetByIdAsync(new VesselId(id))
                    ?? throw new InvalidOperationException($"Vessel with ID '{id}' does not exist.");
                await _vesselRepository.DeleteAsync(new VesselId(id));
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
