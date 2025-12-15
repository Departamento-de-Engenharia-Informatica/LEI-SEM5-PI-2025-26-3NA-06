using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.VesselVisitNotification.ValueObjects
{
    public class CrewMembersList : IValueObject
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
    }

    public class CrewMember : IValueObject
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
    }

    public enum CrewMemberRole
    {
        Captain,
        SafetyOfficer,
        CrewMember
    }
}
