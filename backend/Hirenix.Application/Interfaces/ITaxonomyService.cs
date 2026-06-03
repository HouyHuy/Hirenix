using Hirenix.Application.DTOs.Common;
using Hirenix.Application.DTOs.Taxonomy;

namespace Hirenix.Application.Interfaces;

public interface ITaxonomyService
{
    Task<ApiResponse<List<SkillDto>>> GetAllSkillsAsync();
    Task<ApiResponse<List<IndustryDto>>> GetAllIndustriesAsync();
    Task<ApiResponse<List<LocationDto>>> GetAllLocationsAsync();
}
