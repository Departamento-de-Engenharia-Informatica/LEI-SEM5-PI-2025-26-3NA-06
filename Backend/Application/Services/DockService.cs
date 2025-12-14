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

        public async Task<DockDto> GetByIdAsync(DockId id)
        {
            var dock = await _repo.GetByIdAsync(id);
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
            var allowedVesselTypes = new AllowedVesselTypes(dto.AllowedVesselTypeIds);

            var dock = new Dock(dockName, location, length, depth, maxDraft, allowedVesselTypes);

            await _repo.AddAsync(dock);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<DockDto>(dock);
        }

        public async Task<DockDto> UpdateAsync(DockId id, DockUpsertDto dto)
        {
            var dock = await _repo.GetByIdAsync(id);

            var newDockName = new DockName(dto.DockName);
            
            // Check for duplicate name (only if name is changing)
            if (dock.DockName.Value != dto.DockName)
            {
                var existing = await _repo.FindByNameAsync(newDockName);
                if (existing != null && existing.Id.AsGuid() != id.AsGuid())
                {
                    throw new BusinessRuleValidationException($"A dock with name '{dto.DockName}' already exists.");
                }
            }

            dock.ChangeDockName(newDockName);
            dock.ChangeLocation(new Location(dto.LocationDescription));
            dock.ChangeLength(new DockLength(dto.Length));
            dock.ChangeDepth(new Depth(dto.Depth));
            dock.ChangeMaxDraft(new Draft(dto.MaxDraft));
            dock.SetAllowedVesselTypes(new AllowedVesselTypes(dto.AllowedVesselTypeIds));

            await _unitOfWork.CommitAsync();

            return _mapper.Map<DockDto>(dock);
        }
    }
}
