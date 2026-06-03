using Hirenix.Application.Interfaces;
using Hirenix.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hirenix.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Candidate")]
public class ApplicationsController : ControllerBase
{
    public class SubmitApplicationRequest
    {
        public ulong JobId { get; set; }
        public IFormFile? CvFile { get; set; }
        public string? CoverLetter { get; set; }
    }

    private readonly IApplicationService _applicationService;
    private readonly IFileStorageService _fileStorageService;

    public ApplicationsController(IApplicationService applicationService, IFileStorageService fileStorageService)
    {
        _applicationService = applicationService;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Submit a job application with CV upload
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitApplication([FromForm] SubmitApplicationRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !ulong.TryParse(userIdClaim, out var candidateId))
            {
                return Unauthorized(new { message = "Invalid user authentication." });
            }

            // Validate CV file
            var cvFile = request.CvFile;
            if (cvFile == null || cvFile.Length == 0)
            {
                return BadRequest(new { message = "CV file is required." });
            }

            // Open file stream and submit application
            using var stream = cvFile.OpenReadStream();
            var application = await _applicationService.SubmitApplicationAsync(
                request.JobId,
                candidateId,
                stream,
                cvFile.FileName,
                request.CoverLetter
            );

            return Ok(new
            {
                message = "Application submitted successfully.",
                data = new
                {
                    id = application.Id,
                    jobId = application.JobId,
                    status = application.Status.ToString(),
                    appliedAt = application.AppliedAt,
                    cvUrl = _fileStorageService.GetAccessUrl(application.CvUrl)
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while submitting the application.", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all applications for the current candidate
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyApplications()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !ulong.TryParse(userIdClaim, out var candidateId))
            {
                return Unauthorized(new { message = "Invalid user authentication." });
            }

            var applications = await _applicationService.GetMyApplicationsAsync(candidateId);

            var result = applications.Select(a => new
            {
                id = a.Id,
                jobId = a.JobId,
                jobTitle = a.Job?.Title,
                companyName = a.Job?.Company?.Name,
                companyLogo = a.Job?.Company?.LogoUrl,
                location = a.Job?.City?.Name,
                status = a.Status.ToString(),
                appliedAt = a.AppliedAt,
                cvUrl = _fileStorageService.GetAccessUrl(a.CvUrl)
            });

            return Ok(new
            {
                message = "Applications retrieved successfully.",
                data = result,
                total = applications.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving applications.", error = ex.Message });
        }
    }

    /// <summary>
    /// Get application details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetApplicationById(ulong id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !ulong.TryParse(userIdClaim, out var candidateId))
            {
                return Unauthorized(new { message = "Invalid user authentication." });
            }

            var application = await _applicationService.GetApplicationByIdAsync(id, candidateId);

            return Ok(new
            {
                message = "Application retrieved successfully.",
                data = new
                {
                    id = application.Id,
                    jobId = application.JobId,
                    jobTitle = application.Job?.Title,
                    jobDescription = application.Job?.Description,
                    companyName = application.Job?.Company?.Name,
                    companyLogo = application.Job?.Company?.LogoUrl,
                    location = application.Job?.City?.Name,
                    status = application.Status.ToString(),
                    appliedAt = application.AppliedAt,
                    cvUrl = _fileStorageService.GetAccessUrl(application.CvUrl),
                    coverLetter = application.CoverLetter,
                    reviewedAt = application.ReviewedAt,
                    reviewNotes = application.ReviewNotes
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the application.", error = ex.Message });
        }
    }

    /// <summary>
    /// Withdraw an application
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> WithdrawApplication(ulong id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !ulong.TryParse(userIdClaim, out var candidateId))
            {
                return Unauthorized(new { message = "Invalid user authentication." });
            }

            await _applicationService.WithdrawApplicationAsync(id, candidateId);

            return Ok(new { message = "Application withdrawn successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while withdrawing the application.", error = ex.Message });
        }
    }
}
