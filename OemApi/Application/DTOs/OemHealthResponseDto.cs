namespace ProjArqsi.OemApi.Application.DTOs
{
    public class OemHealthResponseDto
    {
        public string Service { get; set; } = "OEM";
        public string Status { get; set; } = "ok";
        public DateTime UtcNow { get; set; }
        public UserInfoDto User { get; set; } = new();
    }

    public class UserInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
