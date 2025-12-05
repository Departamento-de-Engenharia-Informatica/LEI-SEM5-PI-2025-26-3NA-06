namespace ProjArqsi.DTOs.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string ConfirmationToken { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
