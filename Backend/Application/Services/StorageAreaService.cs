using ProjArqsi.Application.DTOs.Dock;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;
using AutoMapper;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Application.DTOs.StorageArea;
using ProjArqsi.Domain.StorageArea.ValueObjects;

namespace ProjArqsi.Application.Services
{
    public class StorageAreaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageAreaRepository _repo;
        private readonly IMapper _mapper;

        public StorageAreaService(IUnitOfWork unitOfWork, IStorageAreaRepository repo, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<StorageAreaDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<List<StorageAreaDto>>(list);
        }

        public async Task<StorageAreaDto> GetByIdAsync(StorageAreaId id)
        {
            var storageArea = await _repo.GetByIdAsync(id)
                ?? throw new InvalidOperationException($"Storage area with ID '{id.Value}' not found.");
            return _mapper.Map<StorageAreaDto>(storageArea);
        }

        public async Task<StorageAreaDto> AddAsync(StorageAreaUpsertDto dto)
        {
            var areaName = new AreaName(dto.AreaName);
            
            // Check for duplicate name
            var existing = await _repo.FindByNameAsync(areaName);
            if (existing != null)
            {
                throw new BusinessRuleValidationException($"A storage area with name '{dto.AreaName}' already exists.");
            }

            var areaType = new AreaType(Enum.Parse<AreaTypeEnum>(dto.AreaType));
            var location = new Location(dto.Location);
            var maxCapacity = new MaxCapacity(dto.MaxCapacity);
            var servesEntirePort = dto.ServesEntirePort;
            var servedDocks = new ServedDocks([.. dto.ServedDockIds.Select(id => new DockId(new Guid(id)))]);
            
            
            var storageArea = new StorageArea(areaName, areaType, location, maxCapacity, servesEntirePort, servedDocks);
            await _repo.AddAsync(storageArea);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<StorageAreaDto>(storageArea);
        }

        public async Task<StorageAreaDto> UpdateAsync(StorageAreaId id, StorageAreaUpsertDto dto)
        {
        
            var storageArea = await _repo.GetByIdAsync(id)
                ?? throw new BusinessRuleValidationException($"StorageArea '{id}' not found.");

            var newAreaName = new AreaName(dto.AreaName);

            if (!Enum.TryParse<AreaTypeEnum>(dto.AreaType, ignoreCase: true, out var areaTypeEnum))
                throw new BusinessRuleValidationException($"Invalid AreaType '{dto.AreaType}'.");

            var servesEntirePort = dto.ServesEntirePort;
            var servedDockIds = (dto.ServedDockIds ?? Enumerable.Empty<string>())
                .Distinct()
                .Select(dockId => new DockId(new Guid(dockId)))
                .ToList();

            // Check for duplicate name (only if name is changing)
            if (!storageArea.Name.Equals(newAreaName))
            {
                var existing = await _repo.FindByNameAsync(newAreaName);

                if (existing != null && existing.Id.Value != id.Value)
                    throw new BusinessRuleValidationException($"A storage area with name '{dto.AreaName}' already exists.");
            }

            storageArea.UpdateDetails(
            newAreaName,
            new AreaType(areaTypeEnum),
            new Location(dto.Location),
            new MaxCapacity(dto.MaxCapacity),
            servesEntirePort,
            new ServedDocks(servedDockIds)
            );

            await _unitOfWork.CommitAsync();

            return _mapper.Map<StorageAreaDto>(storageArea);
        }

    }
}
