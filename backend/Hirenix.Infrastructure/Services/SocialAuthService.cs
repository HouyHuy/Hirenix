using System.Net.Http.Json;
using System.Text.Json;
using Google.Apis.Auth;
using Hirenix.Application.Interfaces;

namespace Hirenix.Infrastructure.Services;

public class SocialAuthService : ISocialAuthService
{
    public async Task<SocialUserInfo?> VerifyGoogleTokenAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            return new SocialUserInfo
            {
                ProviderId = payload.Subject,
                Email = payload.Email?.ToLowerInvariant(),
                Name = payload.Name
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<SocialUserInfo?> VerifyFacebookTokenAsync(string accessToken)
    {
        try
        {
            using var httpClient = new HttpClient();
            var fbUrl = $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}";
            var fbResponse = await httpClient.GetFromJsonAsync<JsonElement>(fbUrl);

            var id = fbResponse.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(id))
                return null;

            return new SocialUserInfo
            {
                ProviderId = id,
                Email = fbResponse.TryGetProperty("email", out var emailProp)
                    ? emailProp.GetString()?.ToLowerInvariant()
                    : null,
                Name = fbResponse.TryGetProperty("name", out var nameProp)
                    ? nameProp.GetString()
                    : null
            };
        }
        catch
        {
            return null;
        }
    }
}
