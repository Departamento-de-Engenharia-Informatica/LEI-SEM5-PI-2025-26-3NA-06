using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    public interface IVesselVisitNotificationRepository : IRepository<VesselVisitNotification, VesselVisitNotificationId>
    {
        Task<List<VesselVisitNotification>> GetAllSubmittedAsync();
        Task<VesselVisitNotification?> GetSubmittedByIdAsync(VesselVisitNotificationId vesselVisitNotificationId);
        Task<List<VesselVisitNotification>> GetAllReviewedAsync();
        Task<VesselVisitNotification?> GetReviewedByIdAsync(VesselVisitNotificationId vesselVisitNotificationId);
        Task<List<VesselVisitNotification>> GetAllDraftsAsync();
        Task<VesselVisitNotification?> GetDraftByIdAsync(VesselVisitNotificationId vesselVisitNotificationId);
        Task<VesselVisitNotification> DraftVVN(VesselVisitNotification vesselVisitNotification);    
        Task<VesselVisitNotification> SubmitVVN(VesselVisitNotification vesselVisitNotification);
    }
}
