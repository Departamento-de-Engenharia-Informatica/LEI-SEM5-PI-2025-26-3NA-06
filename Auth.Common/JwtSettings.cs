namespace ProjArqsi.Auth.Common;

public class JwtSettings
{
    public string Issuer { get; set; } = "ProjArqsi.AuthApi";
    public string Audience { get; set; } = "ProjArqsi.Api";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
