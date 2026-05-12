using Hirenix.Application.DTOs.Admin;
using Hirenix.Application.DTOs.Common;
using Hirenix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hirenix.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminDashboardController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStatsAsync()
    {
        var data = await _adminService.GetDashboardStatsAsync();
        return Ok(ApiResponse<DashboardStatsDto>.Ok(data));
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalyticsAsync([FromQuery] string period = "30d")
    {
        var data = await _adminService.GetAnalyticsAsync(period);
        return Ok(ApiResponse<AnalyticsDto>.Ok(data));
    }

    [HttpGet("recent-activities")]
    public async Task<IActionResult> GetRecentActivitiesAsync([FromQuery] int limit = 10)
    {
        var data = await _adminService.GetRecentActivitiesAsync(limit);
        return Ok(ApiResponse<object>.Ok(new { activities = data }));
    }
}
