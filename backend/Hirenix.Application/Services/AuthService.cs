using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Hirenix.Application.DTOs.Auth;
using Hirenix.Application.DTOs.Common;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;
using Hirenix.Domain.Enums;

namespace Hirenix.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ISocialAuthService _socialAuthService;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IEmailService emailService,
        ISocialAuthService socialAuthService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _emailService = emailService;
        _socialAuthService = socialAuthService;
    }

    // ─── REGISTER (Giai đoạn 1.2) ────────────────────────────────────
    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.Phone))
            return ApiResponse<AuthResponseDto>.Fail("Email or phone number is required.");

        var passwordError = ValidatePasswordComplexity(request.Password);
        if (passwordError != null)
            return ApiResponse<AuthResponseDto>.Fail(passwordError);

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (!IsValidEmail(request.Email))
                return ApiResponse<AuthResponseDto>.Fail("Invalid email format.");

            // Kiểm tra email đã tồn tại
            var existingUser = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
            if (existingUser != null)
            {
                // Nếu đã verify → không cho đăng ký lại
                if (existingUser.IsVerified)
                    return ApiResponse<AuthResponseDto>.Fail("Email is already registered.");

                // Nếu chưa verify → xóa account cũ để cho đăng ký lại
                await _userRepository.DeleteAsync(existingUser.Id);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            if (!IsValidPhone(request.Phone))
                return ApiResponse<AuthResponseDto>.Fail("Số điện thoại không hợp lệ.");

            if (await _userRepository.PhoneExistsAsync(request.Phone))
                return ApiResponse<AuthResponseDto>.Fail("Số điện thoại này đã được sử dụng.");
        }

        // Tạo mã OTP 6 chữ số
        var otpCode = GenerateOtp();

        var user = new User
        {
            Email = request.Email?.Trim().ToLowerInvariant(),
            Phone = request.Phone?.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            AuthProvider = AuthProvider.Email,
            IsActive = true,
            IsVerified = false, // Chưa xác thực cho đến khi verify OTP
            OtpCode = otpCode,
            OtpExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user = await _userRepository.CreateAsync(user);

        // Gửi OTP qua email (nếu có email)
        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            try
            {
                await _emailService.SendOtpAsync(user.Email, otpCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EMAIL ERROR: {ex.Message}");
                // Log lỗi gửi email nhưng vẫn cho user đăng ký thành công
                // User có thể dùng resend-otp sau
            }
        }

        // Không trả token ngay, yêu cầu xác thực OTP trước
        return ApiResponse<AuthResponseDto>.Ok(null!, "Registration successful. Please check your email for OTP verification code.");
    }

    // ─── LOGIN (Giai đoạn 1.5) ───────────────────────────────────────
    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier))
            return ApiResponse<AuthResponseDto>.Fail("Email or phone number is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return ApiResponse<AuthResponseDto>.Fail("Password is required.");

        var identifier = request.Identifier.Trim();
        User? user;

        if (IsValidEmail(identifier))
            user = await _userRepository.GetByEmailAsync(identifier.ToLowerInvariant());
        else
            user = await _userRepository.GetByPhoneAsync(identifier);

        if (user == null || !user.IsActive)
            return ApiResponse<AuthResponseDto>.Fail("Invalid credentials or account inactive.");

        // Chặn user chưa verify OTP
        if (!user.IsVerified)
            return ApiResponse<AuthResponseDto>.Fail("Please verify your email before logging in. Check your inbox for the OTP code.");

        // ── Account Lockout: Kiểm tra tài khoản có đang bị khóa không ──
        if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
        {
            var remaining = (user.LockoutEnd.Value - DateTime.UtcNow).Minutes + 1;
            return ApiResponse<AuthResponseDto>.Fail($"Account is temporarily locked due to too many failed login attempts. Please try again in {remaining} minute(s).");
        }

        if (string.IsNullOrEmpty(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // ── Account Lockout: Tăng đếm lần sai ──
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                return ApiResponse<AuthResponseDto>.Fail("Too many failed login attempts. Account is locked for 15 minutes.");
            }
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return ApiResponse<AuthResponseDto>.Fail($"Invalid credentials. {5 - user.FailedLoginAttempts} attempt(s) remaining before lockout.");
        }

        // ── Account Lockout: Reset khi đăng nhập thành công ──
        if (user.FailedLoginAttempts > 0 || user.LockoutEnd != null)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        var authResponse = await GenerateAuthResponseAsync(user);

        return ApiResponse<AuthResponseDto>.Ok(authResponse, "Login successful.");
    }

    // ─── VERIFY OTP (Giai đoạn 1.3) ─────────────────────────────────
    public async Task<ApiResponse<AuthResponseDto>> VerifyOtpAsync(VerifyOtpRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.OtpCode))
            return ApiResponse<AuthResponseDto>.Fail("Email and OTP code are required.");

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

        if (user == null)
            return ApiResponse<AuthResponseDto>.Fail("User not found.");

        if (user.IsVerified)
            return ApiResponse<AuthResponseDto>.Fail("Email is already verified.");

        if (user.OtpCode != request.OtpCode)
            return ApiResponse<AuthResponseDto>.Fail("Invalid OTP code.");

        if (user.OtpExpiresAt == null || user.OtpExpiresAt < DateTime.UtcNow)
            return ApiResponse<AuthResponseDto>.Fail("OTP code has expired. Please request a new one.");

        // Xác thực thành công
        user.IsVerified = true;
        user.OtpCode = null;
        user.OtpExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Trả token để auto-login sau khi verify
        var authResponse = await GenerateAuthResponseAsync(user);

        return ApiResponse<AuthResponseDto>.Ok(authResponse, "Email verified successfully.");
    }

    // ─── RESEND OTP (Giai đoạn 1.4) ─────────────────────────────────
    public async Task<ApiResponse<object>> ResendOtpAsync(ResendOtpRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<object>.Fail("Email is required.");

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

        if (user == null)
            return ApiResponse<object>.Fail("User not found.");

        if (user.IsVerified)
            return ApiResponse<object>.Fail("Email is already verified.");

        // Tạo OTP mới
        var otpCode = GenerateOtp();
        user.OtpCode = otpCode;
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        try
        {
            await _emailService.SendOtpAsync(user.Email!, otpCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EMAIL ERROR IN RESEND: {ex.Message}");
            return ApiResponse<object>.Fail($"Failed to send OTP email. Error: {ex.Message}");
        }

        return ApiResponse<object>.Ok(null!, "OTP code has been resent to your email.");
    }

    // ─── GOOGLE LOGIN (Giai đoạn 2) ─────────────────────────────────
    public async Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(GoogleLoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            return ApiResponse<AuthResponseDto>.Fail("Google ID token is required.");

        var socialUser = await _socialAuthService.VerifyGoogleTokenAsync(request.IdToken);
        if (socialUser == null)
            return ApiResponse<AuthResponseDto>.Fail("Invalid Google token.");

        var user = await FindOrCreateSocialUserAsync(socialUser, AuthProvider.Google);

        if (!user.IsActive)
            return ApiResponse<AuthResponseDto>.Fail("Account is inactive.");

        var authResponse = await GenerateAuthResponseAsync(user);
        return ApiResponse<AuthResponseDto>.Ok(authResponse, "Google login successful.");
    }

    // ─── FACEBOOK LOGIN (Giai đoạn 3) ───────────────────────────────
    public async Task<ApiResponse<AuthResponseDto>> FacebookLoginAsync(FacebookLoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
            return ApiResponse<AuthResponseDto>.Fail("Facebook access token is required.");

        var socialUser = await _socialAuthService.VerifyFacebookTokenAsync(request.AccessToken);
        if (socialUser == null)
            return ApiResponse<AuthResponseDto>.Fail("Invalid Facebook token.");

        var user = await FindOrCreateSocialUserAsync(socialUser, AuthProvider.Facebook);

        if (!user.IsActive)
            return ApiResponse<AuthResponseDto>.Fail("Account is inactive.");

        var authResponse = await GenerateAuthResponseAsync(user);
        return ApiResponse<AuthResponseDto>.Ok(authResponse, "Facebook login successful.");
    }

    // ─── REFRESH TOKEN ───────────────────────────────────────────────
    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return ApiResponse<AuthResponseDto>.Fail("Refresh token is required.");

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<AuthResponseDto>.Fail("Invalid or expired refresh token.");

        await _refreshTokenRepository.RevokeAsync(storedToken);

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user == null || !user.IsActive)
            return ApiResponse<AuthResponseDto>.Fail("User not found or inactive.");

        var authResponse = await GenerateAuthResponseAsync(user);

        return ApiResponse<AuthResponseDto>.Ok(authResponse, "Token refreshed successfully.");
    }

    // ─── LOGOUT ──────────────────────────────────────────────────────
    public async Task<ApiResponse<object>> LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return ApiResponse<object>.Fail("Refresh token is required.");

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken != null && !storedToken.IsRevoked)
            await _refreshTokenRepository.RevokeAsync(storedToken);

        return ApiResponse<object>.Ok(null!, "Logged out successfully.");
    }

    // ─── FORGOT PASSWORD (Gửi OTP để khôi phục mật khẩu) ────────────
    public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<object>.Fail("Email is required.");

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

        if (user == null)
            return ApiResponse<object>.Fail("Email not found.");

        if (!user.IsActive)
            return ApiResponse<object>.Fail("Account is inactive.");

        var otpCode = GenerateOtp();
        user.OtpCode = otpCode;
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        try
        {
            await _emailService.SendPasswordResetOtpAsync(user.Email!, otpCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EMAIL ERROR IN FORGOT: {ex.Message}");
            return ApiResponse<object>.Fail("Failed to send OTP email. Please try again later.");
        }

        return ApiResponse<object>.Ok(null!, "OTP code has been sent to your email.");
    }

    // ─── RESET PASSWORD (Đặt lại mật khẩu bằng OTP) ─────────────────
    public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.OtpCode) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
            return ApiResponse<object>.Fail("Email, OTP code, and new password are required.");

        var passwordError = ValidatePasswordComplexity(request.NewPassword);
        if (passwordError != null)
            return ApiResponse<object>.Fail(passwordError);

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

        if (user == null)
            return ApiResponse<object>.Fail("User not found.");

        if (user.OtpCode != request.OtpCode)
            return ApiResponse<object>.Fail("Invalid OTP code.");

        if (user.OtpExpiresAt == null || user.OtpExpiresAt < DateTime.UtcNow)
            return ApiResponse<object>.Fail("OTP code has expired. Please request a new one.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.OtpCode = null;
        user.OtpExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return ApiResponse<object>.Ok(null!, "Password has been reset successfully.");
    }

    // ─── CHANGE PASSWORD (Đổi mật khẩu khi đang đăng nhập) ──────────
    public async Task<ApiResponse<object>> ChangePasswordAsync(ulong userId, ChangePasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
            return ApiResponse<object>.Fail("Current password and new password are required.");

        var passwordError = ValidatePasswordComplexity(request.NewPassword);
        if (passwordError != null)
            return ApiResponse<object>.Fail(passwordError);

        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return ApiResponse<object>.Fail("User not found.");

        if (string.IsNullOrEmpty(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return ApiResponse<object>.Fail("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return ApiResponse<object>.Ok(null!, "Password changed successfully.");
    }

    // ─── CHECK PHONE EXISTS ──────────────────────────────────────────
    public async Task<ApiResponse<bool>> CheckPhoneExistsAsync(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return ApiResponse<bool>.Fail("Phone number is required.");

        if (!IsValidPhone(phone))
            return ApiResponse<bool>.Fail("Invalid phone number format.");

        var exists = await _userRepository.PhoneExistsAsync(phone.Trim());

        return ApiResponse<bool>.Ok(exists, exists ? "Phone number is already registered." : "Phone number is available.");
    }

    // ─── CHECK EMAIL EXISTS ──────────────────────────────────────────
    public async Task<ApiResponse<EmailCheckResultDto>> CheckEmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return ApiResponse<EmailCheckResultDto>.Fail("Email is required.");

        if (!IsValidEmail(email))
            return ApiResponse<EmailCheckResultDto>.Fail("Invalid email format.");

        var user = await _userRepository.GetByEmailAsync(email.Trim().ToLowerInvariant());

        if (user == null)
        {
            return ApiResponse<EmailCheckResultDto>.Ok(
                new EmailCheckResultDto { Exists = false, IsVerified = false },
                "Email is available."
            );
        }

        if (user.IsVerified)
        {
            return ApiResponse<EmailCheckResultDto>.Ok(
                new EmailCheckResultDto { Exists = true, IsVerified = true },
                "Email is already registered."
            );
        }

        // Email tồn tại nhưng chưa verify
        return ApiResponse<EmailCheckResultDto>.Ok(
            new EmailCheckResultDto { Exists = true, IsVerified = false },
            "Email is registered but not verified."
        );
    }

    // ─── HELPER: Social Login (dùng chung cho Google & Facebook) ─────
    private async Task<User> FindOrCreateSocialUserAsync(SocialUserInfo socialUser, AuthProvider provider)
    {
        // Bước 1: Tìm bằng Provider ID
        var user = await _userRepository.GetByProviderIdAsync(provider.ToString().ToLower(), socialUser.ProviderId);

        // Bước 2: Tìm bằng email → link account
        if (user == null && !string.IsNullOrEmpty(socialUser.Email))
        {
            user = await _userRepository.GetByEmailAsync(socialUser.Email);
            if (user != null)
            {
                user.AuthProvider = provider;
                user.AuthProviderId = socialUser.ProviderId;
                user.IsVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
        }

        // Bước 3: Tạo user mới
        if (user == null)
        {
            user = new User
            {
                Email = socialUser.Email,
                AuthProvider = provider,
                AuthProviderId = socialUser.ProviderId,
                Role = UserRole.Candidate,
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            user = await _userRepository.CreateAsync(user);
        }

        return user;
    }

    // ─── HELPER: Generate Auth Response ──────────────────────────────
    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenStr = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = _tokenService.GetRefreshTokenExpiry(),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString().ToLowerInvariant(),
            AccessToken = accessToken,
            RefreshToken = refreshTokenStr,
            AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiry()
        };
    }

    private static string GenerateOtp()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

    private static string? ValidatePasswordComplexity(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Password is required.";
        if (password.Length < 6)
            return "Password must be at least 6 characters.";
        if (!Regex.IsMatch(password, @"[A-Z]"))
            return "Password must contain at least one uppercase letter (A-Z).";
        if (!Regex.IsMatch(password, @"[0-9]"))
            return "Password must contain at least one digit (0-9).";
        if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            return "Password must contain at least one special character (@, $, !, %, *, ?, & ...).";
        return null;
    }

    private static bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private static bool IsValidPhone(string phone)
    {
        return Regex.IsMatch(phone, @"^\+?[0-9]{9,15}$");
    }
}
