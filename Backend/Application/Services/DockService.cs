using ProjArqsi.Application.DTOs.Dock;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;
using AutoMapper;

namespace ProjArqsi.Application.Services
{
    public class DockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDockRepository _repo;
        private readonly IMapper _mapper;

        public DockService(IUnitOfWork unitOfWork, IDockRepository repo, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<DockDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<List<DockDto>>(list);
        }

        public async Task<DockDto> GetByIdAsync(Guid id)
        {
            var dock = await _repo.GetByIdAsync(new DockId(id))
                ?? throw new InvalidOperationException($"Dock with ID '{id}' not found.");
            return _mapper.Map<DockDto>(dock);
        }

        public async Task<DockDto> AddAsync(DockUpsertDto dto)
        {
            var dockName = new DockName(dto.DockName);
            
            // Check for duplicate name
            var existing = await _repo.FindByNameAsync(dockName);
            if (existing != null)
            {
                throw new BusinessRuleValidationException($"A dock with name '{dto.DockName}' already exists.");
            }

            var location = new Location(dto.LocationDescription);
            var length = new DockLength(dto.Length);
            var depth = new Depth(dto.Depth);
            var maxDraft = new Draft(dto.MaxDraft);
            var allowedVesselTypeGuids = dto.AllowedVesselTypeIds.Select(Guid.Parse).ToList();
            var allowedVesselTypes = new AllowedVesselTypes(allowedVesselTypeGuids);

            var dock = new Dock(dockName, location, length, depth, maxDraft, allowedVesselTypes);

            await _repo.AddAsync(dock);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<DockDto>(dock);
        }

        public async Task<DockDto> UpdateAsync(DockId id, DockUpsertDto dto)
        {
            var dock = await _repo.GetByIdAsync(id) ?? throw new InvalidOperationException($"Dock with ID '{id}' does not exist.");
            
            var newDockName = new DockName(dto.DockName);
            
            // Check for duplicate name (only if name is changing)
            if (dock.DockName.Value != dto.DockName)
            {
                var existing = await _repo.FindByNameAsync(newDockName);
                if (existing != null && existing.Id.Value != id.Value)
                {
                    throw new BusinessRuleValidationException($"A dock with name '{dto.DockName}' already exists.");
                }
            }

            dock.UpdateDetails(
                newDockName,
                new Location(dto.LocationDescription),
                new DockLength(dto.Length),
                new Depth(dto.Depth),
                new Draft(dto.MaxDraft),
                new AllowedVesselTypes(dto.AllowedVesselTypeIds.Select(Guid.Parse).ToList())
            );
            await _unitOfWork.CommitAsync();

            return _mapper.Map<DockDto>(dock);
        }
    }
}
