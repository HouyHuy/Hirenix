using Hirenix.Application.DTOs.Auth;
using Hirenix.Application.DTOs.Common;

namespace Hirenix.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<AuthResponseDto>> VerifyOtpAsync(VerifyOtpRequestDto request);
    Task<ApiResponse<object>> ResendOtpAsync(ResendOtpRequestDto request);
    Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(GoogleLoginRequestDto request);
    Task<ApiResponse<AuthResponseDto>> FacebookLoginAsync(FacebookLoginRequestDto request);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<ApiResponse<object>> LogoutAsync(string refreshToken);
    Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<ApiResponse<object>> ChangePasswordAsync(ulong userId, ChangePasswordRequestDto request);
    Task<ApiResponse<EmailCheckResultDto>> CheckEmailExistsAsync(string email);
    Task<ApiResponse<bool>> CheckPhoneExistsAsync(string phone);
}
