using ProjArqsi.Domain.UserAggregate.ValueObjects;

namespace ProjArqsi.DTOs.User
{
    public class CreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "User";

        public CreateUserDto() {}

        public CreateUserDto(string email, string username, string role = "User")
        {
            Email = email;
            Username = username;
            Role = role;
        }
    }
}
