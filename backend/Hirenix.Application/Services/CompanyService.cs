using Hirenix.Application.DTOs.Company;
using Hirenix.Application.DTOs.Job;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;

namespace Hirenix.Application.Services;

/// <summary>
/// Service implementation for Company business logic
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<CompanyDto?> GetByIdAsync(ulong id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        return company == null ? null : MapToDto(company);
    }

    public async Task<PaginatedResultDto<CompanyDto>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var (companies, total) = await _companyRepository.GetAllAsync(page, pageSize);

        return new PaginatedResultDto<CompanyDto>
        {
            Data = companies.Select(MapToDto).ToList(),
            Pagination = new PaginationMetadata
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                TotalItems = total
            }
        };
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyDto dto)
    {
        // Validate company name uniqueness
        if (await _companyRepository.ExistsByNameAsync(dto.Name))
        {
            throw new InvalidOperationException($"Company with name '{dto.Name}' already exists");
        }

        var company = new Company
        {
            Name = dto.Name,
            Description = dto.Description,
            Website = dto.Website,
            Size = dto.Size,
            IndustryId = dto.IndustryId,
            CityId = dto.CityId,
            Address = dto.Address
        };

        var created = await _companyRepository.CreateAsync(company);
        
        // Reload with navigation properties
        var result = await _companyRepository.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task<CompanyDto> UpdateAsync(ulong id, UpdateCompanyDto dto)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null)
        {
            throw new KeyNotFoundException($"Company with ID {id} not found");
        }

        // Validate name uniqueness if name is being changed
        if (!string.IsNullOrEmpty(dto.Name) && dto.Name != company.Name)
        {
            if (await _companyRepository.ExistsByNameAsync(dto.Name, id))
            {
                throw new InvalidOperationException($"Company with name '{dto.Name}' already exists");
            }
            company.Name = dto.Name;
        }

        // Update only provided fields
        if (dto.Description != null) company.Description = dto.Description;
        if (dto.Website != null) company.Website = dto.Website;
        if (dto.Size.HasValue) company.Size = dto.Size;
        if (dto.IndustryId.HasValue) company.IndustryId = dto.IndustryId;
        if (dto.CityId.HasValue) company.CityId = dto.CityId;
        if (dto.Address != null) company.Address = dto.Address;

        var updated = await _companyRepository.UpdateAsync(company);
        
        // Reload with navigation properties
        var result = await _companyRepository.GetByIdAsync(updated.Id);
        return MapToDto(result!);
    }

    public async Task<bool> DeleteAsync(ulong id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null)
        {
            return false;
        }

        return await _companyRepository.DeleteAsync(id);
    }

    public async Task<List<CompanyDto>> GetByIndustryAsync(uint industryId)
    {
        var companies = await _companyRepository.GetByIndustryAsync(industryId);
        return companies.Select(MapToDto).ToList();
    }

    public async Task<List<CompanyDto>> GetByCityAsync(uint cityId)
    {
        var companies = await _companyRepository.GetByCityAsync(cityId);
        return companies.Select(MapToDto).ToList();
    }

    // Private helper method to map entity to DTO
    private CompanyDto MapToDto(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            LogoUrl = company.LogoUrl,
            Description = company.Description,
            Website = company.Website,
            Size = company.Size,
            IndustryId = company.IndustryId,
            IndustryName = company.Industry?.Name,
            CityId = company.CityId,
            CityName = company.City?.Name,
            Address = company.Address,
            IsVerified = company.IsVerified,
            IsActive = company.IsActive,
            EmployeeCount = company.EmployerProfiles?.Count ?? 0,
            ActiveJobCount = company.Jobs?.Count(j => j.Status == Domain.Enums.JobStatus.Active) ?? 0,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt
        };
    }
}
