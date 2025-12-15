using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.VesselVisitNotification.ValueObjects;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    public class VesselVisitNotification : Entity<VesselVisitNotificationId>, IAggregateRoot
    {
        public ReferredVessel ReferredVessel { get; private set; } = null!;
        public ArrivalDate ArrivalDate { get; private set; } = null!;
        public DepartureDate DepartureDate { get; private set; } = null!;

        //public CargoManifest CargoManifest { get; private set; } = null!;
        //public bool IsHazardous { get; private set; } = false;
        //public CrewMembersList CrewMembersList { get; private set; } = null!;
        public RejectionReason? RejectionReason { get; private set; }
        public Status Status { get; private set; } = null!;

        protected VesselVisitNotification() { }

        public VesselVisitNotification(string referredVessel, DateTime? arrivalDate, DateTime? departureDate)
        {
            Id = new VesselVisitNotificationId(Guid.NewGuid());
            ReferredVessel = new ReferredVessel(new IMOnumber(referredVessel));
            ArrivalDate = new ArrivalDate(arrivalDate);
            DepartureDate = new DepartureDate(departureDate);
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
        public void UpdateCargoManifest(CargoManifest newManifest)
        {
            //CargoManifest = newManifest;
        }

        public void UpdateCrew(CrewMembersList newCrew)
        {
            //CrewMembersList = newCrew;
        }


    }

    
}
