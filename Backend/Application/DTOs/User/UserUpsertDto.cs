namespace ProjArqsi.DTOs.User
{
    public class UserUpsertDto
    {
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string Role { get; set; }
    }
}
