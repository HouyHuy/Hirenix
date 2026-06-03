using Hirenix.Domain.Entities;

namespace Hirenix.Application.Interfaces;

public interface ITaxonomyRepository
{
    Task<List<Location>> GetAllLocationsAsync();
    Task<List<Industry>> GetAllIndustriesAsync();
    Task<List<Skill>> GetAllSkillsAsync();
}
