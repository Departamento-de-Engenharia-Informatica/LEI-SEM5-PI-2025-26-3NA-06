using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.UserAggregate.ValueObjects;

namespace ProjArqsi.Domain.UserAggregate
{
    public class User : Entity<UserId>, IAggregateRoot
    {
        public Username Username { get; private set; } = null!;
        public Role Role { get; private set; } = null!;
        public Email Email { get; private set; } = null!;
        public bool IsActive { get; private set; } = false;
        public string ConfirmationToken { get; set; } = string.Empty;
        public DateTime? ConfirmationTokenExpiry { get; set; }

        private User()
        {
        }

        public User(Username username, Role role, Email email, bool isActive = true, string confirmationToken = "")
        {
            Id = new UserId(Guid.NewGuid());
            Username = username;
            Role = role;
            Email = email;
            IsActive = isActive;
            ConfirmationToken = confirmationToken;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public void ChangeRole(Role role)
        {
            Role = role;
        }

        public void ChangeUsername(Username username)
        {
            Username = username;
        }

        public void ChangeEmail(Email email)
        {
            Email = email;
        }

        public void ChangeConfirmationToken(string confirmationToken)
        {
            ConfirmationToken = confirmationToken;
        }

        public void GenerateConfirmationToken()
        {
            ConfirmationToken = Guid.NewGuid().ToString("N");
            ConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);
        }
    }
}
