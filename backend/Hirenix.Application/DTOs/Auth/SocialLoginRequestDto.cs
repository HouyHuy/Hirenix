namespace Hirenix.Application.DTOs.Auth;

public class GoogleLoginRequestDto
{
    public string IdToken { get; set; } = string.Empty;
}

public class FacebookLoginRequestDto
{
    public string AccessToken { get; set; } = string.Empty;
}
