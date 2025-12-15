using ProjArqsi.Application.DTOs;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Domain.Shared;
using AutoMapper;

namespace ProjArqsi.Application.Services
{
    public class VesselTypeService
    {
        private readonly IVesselTypeRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VesselTypeService(IVesselTypeRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<VesselTypeDto> CreateAsync(VesselTypeUpsertDto dto)
        {
            // Validate unique name
            var typeName = new TypeName(dto.TypeName);
            var existing = await _repository.FindByNameAsync(typeName);
            if (existing != null)
                throw new InvalidOperationException($"Vessel type with name '{dto.TypeName}' already exists.");

            // Create domain entity
            var vesselType = new VesselType(
                new VesselTypeId(Guid.NewGuid()),
                typeName,
                new TypeDescription(dto.TypeDescription),
                new TypeCapacity(dto.TypeCapacity),
                new MaxRows(dto.MaxRows),
                new MaxBays(dto.MaxBays),
                new MaxTiers(dto.MaxTiers)
            );

            await _repository.AddAsync(vesselType);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<VesselTypeDto>(vesselType);
        }

        public async Task<VesselTypeDto> UpdateAsync(Guid id, VesselTypeUpsertDto dto)
        {
            var vesselType = await _repository.GetByIdAsync(new VesselTypeId(id));
            if (vesselType == null)
                throw new KeyNotFoundException($"Vessel type with id '{id}' not found.");

            // Validate unique name (if changed)
            var newTypeName = new TypeName(dto.TypeName);
            if (vesselType.TypeName.Value != dto.TypeName)
            {
                var existing = await _repository.FindByNameAsync(newTypeName);
                if (existing != null && existing.Id.Value.ToString() != id.ToString())
                    throw new InvalidOperationException($"Vessel type with name '{dto.TypeName}' already exists.");
            }

            // Update entity
            vesselType.UpdateDetails(
                newTypeName,
                new TypeDescription(dto.TypeDescription),
                new TypeCapacity(dto.TypeCapacity),
                new MaxRows(dto.MaxRows),
                new MaxBays(dto.MaxBays),
                new MaxTiers(dto.MaxTiers)
            );

            await _unitOfWork.CommitAsync();

            return _mapper.Map<VesselTypeDto>(vesselType);
        }

        public async Task<VesselTypeDto> GetByIdAsync(Guid id)
        {
            var vesselType = await _repository.GetByIdAsync(new VesselTypeId(id));
            return _mapper.Map<VesselTypeDto>(vesselType);
        }

        public async Task<IEnumerable<VesselTypeDto>> GetAllAsync()
        {
            var vesselTypes = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<VesselTypeDto>>(vesselTypes);
        }

        public async Task<IEnumerable<VesselTypeDto>> SearchByNameAsync(string searchTerm)
        {
            var vesselTypes = await _repository.SearchByNameAsync(new TypeName(searchTerm));
            return _mapper.Map<IEnumerable<VesselTypeDto>>(vesselTypes);
        }

        public async Task<IEnumerable<VesselTypeDto>> SearchByDescriptionAsync(string searchTerm)
        {
            var vesselTypes = await _repository.SearchByDescriptionAsync(new TypeDescription(searchTerm));
            return _mapper.Map<IEnumerable<VesselTypeDto>>(vesselTypes);
        }

        public async Task<IEnumerable<VesselTypeDto>> SearchByNameOrDescriptionAsync(string searchTerm)
        {
            var vesselTypes = await _repository.SearchByNameOrDescriptionAsync(searchTerm);
            return _mapper.Map<IEnumerable<VesselTypeDto>>(vesselTypes);
        }
    }
}
