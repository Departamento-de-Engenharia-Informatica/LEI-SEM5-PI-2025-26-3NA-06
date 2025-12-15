using ProjArqsi.Domain.Shared;
using AutoMapper;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;
using ProjArqsi.Application.DTOs.VVN;

namespace ProjArqsi.Application.Services
{
    public class VesselVisitNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVesselVisitNotificationRepository _repo;
        private readonly IMapper _mapper;

        public VesselVisitNotificationService(IUnitOfWork unitOfWork, IVesselVisitNotificationRepository repo, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repo = repo;
            _mapper = mapper;
        }

        //This stores draft VVN. Only basic checks are performed. The object will suffer changes later.
        public async Task<VVNDraftDtoWId> DraftVVNAsync(VVNDraftDto dto)
        {
            var referredVessel = dto.ReferredVesselId;
            var arrivalDate = dto.ArrivalDate;
            var departureDate = dto.DepartureDate;

            var draft = new VesselVisitNotification(referredVessel, arrivalDate, departureDate);

            await _repo.DraftVVN(draft);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<VVNDraftDtoWId>(draft);
        }
        //This stores finished VVN. All business rules must be validated.
        public async Task<VVNSubmitDtoWId> SubmitVVNAsync(VVNSubmitDto dto)
        {
            //Validate all business rules here
           
            var referredVessel = dto.ReferredVesselId;
            var arrivalDate = dto.ArrivalDate;
            var departureDate = dto.DepartureDate;
         
            var draft = new VesselVisitNotification(referredVessel,
                arrivalDate,
                departureDate
                );
            await _repo.AddAsync(draft);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<VVNSubmitDtoWId>(draft);
        }

         public async Task<VVNDto> AcceptAsync(Guid id)
        {
            var vvn = await _repo.GetByIdAsync(new VesselVisitNotificationId(id));
            if (vvn == null)
                throw new BusinessRuleValidationException("Vessel Visit Notification not found.");
            if (!vvn.Status.Equals(Statuses.Submitted))
                throw new BusinessRuleValidationException("Only submitted notifications can be accepted.");
            vvn.Accept();
            await _unitOfWork.CommitAsync();
            return _mapper.Map<VVNDto>(vvn);
        }

        public async Task<VVNDto> RejectAsync(Guid id, string rejectionReason)
        {
            var vvn = await _repo.GetByIdAsync(new VesselVisitNotificationId(id));
            if (vvn == null)
                throw new BusinessRuleValidationException("Vessel Visit Notification not found.");
            if (!vvn.Status.Equals(Statuses.Submitted))
                throw new BusinessRuleValidationException("Only submitted notifications can be rejected.");
            if (string.IsNullOrWhiteSpace(rejectionReason))
                throw new BusinessRuleValidationException("Rejection reason is required.");
            vvn.Reject(rejectionReason);
            await _unitOfWork.CommitAsync();
            return _mapper.Map<VVNDto>(vvn);
        }

        public async Task<List<VVNDto>> GetAllSubmittedAsync()
        {
            var vvns = await _repo.GetAllSubmittedAsync();
            return _mapper.Map<List<VVNDto>>(vvns);
        }

        public async Task<VVNDto> GetSubmittedByIdAsync(Guid vesselVisitNotificationId)
        {
            var vvn = await _repo.GetSubmittedByIdAsync(new VesselVisitNotificationId(vesselVisitNotificationId));
            if (vvn == null)
            {
                throw new InvalidOperationException($"Vessel Visit Notification with ID '{vesselVisitNotificationId}' not found.");
            }
            return _mapper.Map<VVNDto>(vvn);
        }

        public async Task<List<VVNDto>> GetAllReviewedAsync()
        {
            var vvns = await _repo.GetAllReviewedAsync();
            return _mapper.Map<List<VVNDto>>(vvns);
        }

        public async Task<VVNDto> GetReviewedByIdAsync(Guid vesselVisitNotificationId)
        {
            var vvn = await _repo.GetReviewedByIdAsync(new VesselVisitNotificationId(vesselVisitNotificationId));
            if (vvn == null)
            {
                throw new InvalidOperationException($"Vessel Visit Notification with ID '{vesselVisitNotificationId}' not found.");
            }
            return _mapper.Map<VVNDto>(vvn);
        }

        public async Task<List<VVNDto>> GetAllDraftsAsync()
        {
            var vvns = await _repo.GetAllDraftsAsync();
            return _mapper.Map<List<VVNDto>>(vvns);
        }

        public async Task<VVNDto> GetDraftByIdAsync(Guid id)
        {
            var vvn = await _repo.GetDraftByIdAsync(new VesselVisitNotificationId(id));
            if (vvn == null)
            {
                throw new InvalidOperationException($"Vessel Visit Notification with ID '{id}' not found.");
            }
            return _mapper.Map<VVNDto>(vvn);
        }

        public async Task DeleteDraftAsync(Guid id)
        {
            await _repo.DeleteAsync(new VesselVisitNotificationId(id));
            await _unitOfWork.CommitAsync();
        }
    }
}
