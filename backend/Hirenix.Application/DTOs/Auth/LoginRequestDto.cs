namespace Hirenix.Application.DTOs.Auth;

public class LoginRequestDto
{
    /// <summary>
    /// Can be email or phone number.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
