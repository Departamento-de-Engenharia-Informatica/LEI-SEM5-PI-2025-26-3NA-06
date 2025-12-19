using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.VesselAggregate;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    // This entity can be saved partially. Only the ReferedVesselId and Status is mandatory at creation.
    public class VesselVisitNotification : Entity<VesselVisitNotificationId>, IAggregateRoot
    {
        public ReferredVesselId ReferredVesselId { get; private set; } = null!;
        public ArrivalDate? ArrivalDate { get; private set; }
        public DepartureDate? DepartureDate { get; private set; }

        //public CargoManifest CargoManifest { get; private set; } = null!;
        public bool IsHazardous { get; private set; } = false;
        //public CrewMembersList CrewMembersList { get; private set; } = null!;
        public RejectionReason? RejectionReason { get; private set; }
        public Status Status { get; private set; } = null!;

        // Primitive property for EF Core queries
        public int StatusValue
        {
            get => (int)Status.Value;
            private set => Status = new Status((StatusEnum)value);
        }

    protected VesselVisitNotification() { }

    public VesselVisitNotification(string referredVessel, DateTime? arrivalDate, DateTime? departureDate)
    {
        Id = new VesselVisitNotificationId(Guid.NewGuid());
        ReferredVesselId = new ReferredVesselId(new IMOnumber(referredVessel));
        ArrivalDate = new ArrivalDate(arrivalDate);
        DepartureDate = new DepartureDate(departureDate);
        Status = Statuses.InProgress;
    }
    
    public void Submit()
        {
            if (!Status.Equals(Statuses.InProgress))
                throw new InvalidOperationException("Only notifications in progress can be submitted.");
            Status = Statuses.Submitted;
        }

        public void Accept()
            {
                if (!Status.Equals(Statuses.Submitted))
                    throw new InvalidOperationException("Only submitted notifications can be accepted.");
                Status = Statuses.Accepted;
                RejectionReason = null;
            }

        public void Reject(string rejectionReason)
        {
            if (!Status.Equals(Statuses.Submitted))
                throw new InvalidOperationException("Only submitted notifications can be rejected.");
            if (string.IsNullOrWhiteSpace(rejectionReason))
                throw new InvalidOperationException("Rejection reason is required.");
            Status = Statuses.Rejected;
            RejectionReason = new RejectionReason(rejectionReason);
        }
    }
}
