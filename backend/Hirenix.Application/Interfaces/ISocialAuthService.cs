namespace Hirenix.Application.Interfaces;

public class SocialUserInfo
{
    public string ProviderId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Name { get; set; }
}

public interface ISocialAuthService
{
    Task<SocialUserInfo?> VerifyGoogleTokenAsync(string idToken);
    Task<SocialUserInfo?> VerifyFacebookTokenAsync(string accessToken);
}
