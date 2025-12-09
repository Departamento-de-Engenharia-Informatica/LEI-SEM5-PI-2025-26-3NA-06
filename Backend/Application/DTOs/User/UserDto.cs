namespace ProjArqsi.DTOs.User
{
    public class UserDto
    {
        public required Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Role { get; set; }
        public required string Email { get; set; }
        public required string ConfirmationToken { get; set; }
        public DateTime? ConfirmationTokenExpiry { get; set; }
        public required bool IsActive { get; set; }
    }
}
