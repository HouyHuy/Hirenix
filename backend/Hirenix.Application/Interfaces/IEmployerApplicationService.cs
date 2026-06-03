using Hirenix.Application.DTOs.Application;
using Hirenix.Domain.Enums;

namespace Hirenix.Application.Interfaces;

public interface IEmployerApplicationService
{
    Task<List<EmployerApplicationDto>> GetApplicationsAsync(
        ulong userId,
        ulong? jobId = null,
        ApplicationStatus? status = null);

    Task<EmployerApplicationDto?> GetApplicationByIdAsync(ulong userId, ulong applicationId);

    Task<bool> UpdateApplicationStatusAsync(
        ulong userId,
        ulong applicationId,
        ApplicationStatus newStatus,
        string? reviewNotes = null);

    Task<ApplicationStatisticsDto> GetStatisticsAsync(ulong userId);
}
