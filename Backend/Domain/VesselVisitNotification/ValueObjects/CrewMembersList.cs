using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselVisitNotificationAggregate
{
    public class CrewMembersList : ValueObject
    {
        private readonly List<CrewMember> members = new();

        public IReadOnlyCollection<CrewMember> Members => members.AsReadOnly();

        public CrewMembersList(IEnumerable<CrewMember>? crew)
        {
            if (crew != null)
            {
                members.AddRange(crew);
            }
        }

        public bool HasSafetyOfficer() =>
            members.Any(m => m.Role == CrewMemberRole.SafetyOfficer);

        public bool HasCaptain() =>
            members.Any(m => m.Role == CrewMemberRole.Captain);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            foreach (var member in members)
            {
                yield return member;
            }
        }
    }

    public class CrewMember : ValueObject
    {
        public string Name { get; }
        public string CitizenId { get; }
        public string Nationality { get; }
        public CrewMemberRole Role { get; }

        public CrewMember(string name, string citizenId, string nationality, CrewMemberRole role)
        {
            Name = (name ?? string.Empty).Trim();
            CitizenId = (citizenId ?? string.Empty).Trim();
            Nationality = (nationality ?? string.Empty).Trim();
            Role = role;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return CitizenId;
            yield return Nationality;
            yield return Role;
        }
    }

    public enum CrewMemberRole
    {
        Captain,
        SafetyOfficer,
        CrewMember
    }
}
