using Hirenix.Application.DTOs.Common;
using Hirenix.Application.DTOs.Taxonomy;
using Hirenix.Application.Interfaces;

namespace Hirenix.Application.Services;

public class TaxonomyService : ITaxonomyService
{
    private readonly ITaxonomyRepository _repository;

    public TaxonomyService(ITaxonomyRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<SkillDto>>> GetAllSkillsAsync()
    {
        try
        {
            var skills = await _repository.GetAllSkillsAsync();
            
            var skillDtos = skills.Select(s => new SkillDto
            {
                Id = s.Id,
                Name = s.Name,
                Slug = s.Slug,
                Category = s.Category
            }).ToList();

            return ApiResponse<List<SkillDto>>.Ok(skillDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<SkillDto>>.Fail($"Error retrieving skills: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<IndustryDto>>> GetAllIndustriesAsync()
    {
        try
        {
            var industries = await _repository.GetAllIndustriesAsync();
            
            var industryDtos = industries.Select(i => new IndustryDto
            {
                Id = i.Id,
                Name = i.Name,
                Slug = i.Slug
            }).ToList();

            return ApiResponse<List<IndustryDto>>.Ok(industryDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<IndustryDto>>.Fail($"Error retrieving industries: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<LocationDto>>> GetAllLocationsAsync()
    {
        try
        {
            var locations = await _repository.GetAllLocationsAsync();
            
            var locationDtos = locations.Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Slug = l.Slug,
                CountryCode = l.CountryCode
            }).ToList();

            return ApiResponse<List<LocationDto>>.Ok(locationDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LocationDto>>.Fail($"Error retrieving locations: {ex.Message}");
        }
    }
}
