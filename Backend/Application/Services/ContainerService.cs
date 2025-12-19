using ProjArqsi.Domain.ContainerAggregate;
using ProjArqsi.Domain.ContainerAggregate.ValueObjects;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Application.DTOs;
using AutoMapper;

namespace ProjArqsi.Application.Services
{
    public class ContainerService
    {
        private readonly IContainerRepository _containerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ContainerService(
            IContainerRepository containerRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _containerRepository = containerRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ContainerDto>> GetAllAsync()
        {
            var containers = await _containerRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ContainerDto>>(containers);
        }

        public async Task<ContainerDto> GetByIdAsync(Guid id)
        {
            var container = await _containerRepository.GetByIdAsync(new ContainerId(id)) 
                ?? throw new InvalidOperationException($"Container with ID '{id}' does not exist.");
            return _mapper.Map<ContainerDto>(container);
        }

        public async Task<ContainerDto> GetByIsoCodeAsync(string isoCode)
        {
            var iso = new IsoCode(isoCode);
            var container = await _containerRepository.GetByIsoCodeAsync(iso)
                ?? throw new InvalidOperationException($"Container with ISO code '{isoCode}' does not exist.");
            return _mapper.Map<ContainerDto>(container);
        }

        public async Task<ContainerDto> CreateAsync(UpsertContainerDto dto)
        {
            // Normalize and validate ISO code (will throw if invalid)
            var isoCode = new IsoCode(dto.IsoCode);

            // Check for duplicate ISO code (natural key constraint)
            var existingContainer = await _containerRepository.GetByIsoCodeAsync(isoCode);
            if (existingContainer != null)
                throw new BusinessRuleValidationException($"A container with ISO code '{dto.IsoCode}' already exists. ISO codes must be unique.");

            var cargoType = new CargoType(dto.CargoType);
            var description = new ContainerDescription(dto.Description);

            var container = new Container(
                isoCode,
                dto.IsHazardous,
                cargoType,
                description
            );

            await _containerRepository.AddAsync(container);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<ContainerDto>(container);
        }

        public async Task<ContainerDto> UpdateAsync(Guid id, UpsertContainerDto dto)
        {
            var container = await _containerRepository.GetByIdAsync(new ContainerId(id))
                ?? throw new InvalidOperationException($"Container with ID '{id}' does not exist.");

            // ISO code is immutable - check if trying to change it
            var newIsoCode = new IsoCode(dto.IsoCode);
            if (newIsoCode.Value != container.IsoCode.Value)
                throw new BusinessRuleValidationException("ISO code cannot be changed after container creation. It is immutable.");

            var cargoType = new CargoType(dto.CargoType);
            var description = new ContainerDescription(dto.Description);

            container.UpdateCargoInformation(cargoType, description, dto.IsHazardous);

            await _containerRepository.UpdateAsync(container);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<ContainerDto>(container);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var container = await _containerRepository.GetByIdAsync(new ContainerId(id))
                ?? throw new InvalidOperationException($"Container with ID '{id}' does not exist.");

            await _containerRepository.DeleteAsync(container.Id);
            await _unitOfWork.CommitAsync();

            return true;
        }
    }
}
