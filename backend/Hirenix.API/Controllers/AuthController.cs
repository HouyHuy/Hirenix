using Hirenix.Application.DTOs.Auth;
using Hirenix.Application.DTOs.Common;
using Hirenix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Hirenix.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user with email or phone. Sends OTP to verify email.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Login with email/phone and password. Requires verified email.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Verify OTP code sent to email after registration.
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtpAsync([FromBody] VerifyOtpRequestDto request)
    {
        var result = await _authService.VerifyOtpAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Resend OTP code to email.
    /// </summary>
    [HttpPost("resend-otp")]
    [EnableRateLimiting("OtpPolicy")]
    public async Task<IActionResult> ResendOtpAsync([FromBody] ResendOtpRequestDto request)
    {
        var result = await _authService.ResendOtpAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Login or register using Google ID token.
    /// </summary>
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLoginAsync([FromBody] GoogleLoginRequestDto request)
    {
        var result = await _authService.GoogleLoginAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Login or register using Facebook access token.
    /// </summary>
    [HttpPost("facebook")]
    public async Task<IActionResult> FacebookLoginAsync([FromBody] FacebookLoginRequestDto request)
    {
        var result = await _authService.FacebookLoginAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Logout and revoke refresh token.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogoutAsync([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.LogoutAsync(request.RefreshToken);
        return Ok(result);
    }

    /// <summary>
    /// Get current authenticated user info.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst("uid")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var phone = User.FindFirst("phone")?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(ApiResponse<object>.Ok(new
        {
            UserId = userId,
            Email = email,
            Phone = phone,
            Role = role
        }, "Authenticated user info."));
    }

    /// <summary>
    /// Request OTP to reset password.
    /// </summary>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("OtpPolicy")]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequestDto request)
    {
        var result = await _authService.ForgotPasswordAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Reset password using OTP code.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequestDto request)
    {
        var result = await _authService.ResetPasswordAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Change password for authenticated user.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequestDto request)
    {
        var userIdStr = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !ulong.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
