using ProjArqsi.Domain.UserAggregate.ValueObjects;

namespace ProjArqsi.DTOs.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public Username Username { get; set; } = default!;
        public Role Role { get; set; } = default!;
        public Email Email { get; set; } = default!;
        public string ConfirmationToken { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
