using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;
using Hirenix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for candidate profile operations
/// </summary>
public class CandidateProfileRepository : ICandidateProfileRepository
{
    private readonly HirenixDbContext _context;

    public CandidateProfileRepository(HirenixDbContext context)
    {
        _context = context;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  PROFILE OPERATIONS
    // ═══════════════════════════════════════════════════════════════════

    public async Task<CandidateProfile?> GetByUserIdAsync(ulong userId)
    {
        return await _context.CandidateProfiles
            .Include(p => p.City)
            .Include(p => p.Industry)
            .Include(p => p.Educations)
            .Include(p => p.Experiences)
            .Include(p => p.Skills)
                .ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<CandidateProfile?> GetByIdAsync(ulong id)
    {
        return await _context.CandidateProfiles
            .Include(p => p.City)
            .Include(p => p.Industry)
            .Include(p => p.Educations)
            .Include(p => p.Experiences)
            .Include(p => p.Skills)
                .ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<CandidateProfile> CreateAsync(CandidateProfile profile)
    {
        profile.CreatedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;
        
        await _context.CandidateProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();
        
        return profile;
    }

    public async Task<CandidateProfile> UpdateAsync(CandidateProfile profile)
    {
        profile.UpdatedAt = DateTime.UtcNow;
        
        _context.CandidateProfiles.Update(profile);
        await _context.SaveChangesAsync();
        
        return profile;
    }

    public async Task<bool> DeleteAsync(ulong id)
    {
        var profile = await _context.CandidateProfiles.FindAsync(id);
        if (profile == null)
            return false;

        _context.CandidateProfiles.Remove(profile);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ExistsByUserIdAsync(ulong userId)
    {
        return await _context.CandidateProfiles
            .AnyAsync(p => p.UserId == userId);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  EDUCATION OPERATIONS
    // ═══════════════════════════════════════════════════════════════════

    public async Task<CandidateEducation> AddEducationAsync(CandidateEducation education)
    {
        await _context.CandidateEducations.AddAsync(education);
        await _context.SaveChangesAsync();
        
        return education;
    }

    public async Task<CandidateEducation?> GetEducationByIdAsync(ulong id)
    {
        return await _context.CandidateEducations
            .Include(e => e.Candidate)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<CandidateEducation> UpdateEducationAsync(CandidateEducation education)
    {
        _context.CandidateEducations.Update(education);
        await _context.SaveChangesAsync();
        
        return education;
    }

    public async Task<bool> DeleteEducationAsync(ulong id)
    {
        var education = await _context.CandidateEducations.FindAsync(id);
        if (education == null)
            return false;

        _context.CandidateEducations.Remove(education);
        await _context.SaveChangesAsync();
        
        return true;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  EXPERIENCE OPERATIONS
    // ═══════════════════════════════════════════════════════════════════

    public async Task<CandidateExperience> AddExperienceAsync(CandidateExperience experience)
    {
        await _context.CandidateExperiences.AddAsync(experience);
        await _context.SaveChangesAsync();
        
        return experience;
    }

    public async Task<CandidateExperience?> GetExperienceByIdAsync(ulong id)
    {
        return await _context.CandidateExperiences
            .Include(e => e.Candidate)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<CandidateExperience> UpdateExperienceAsync(CandidateExperience experience)
    {
        _context.CandidateExperiences.Update(experience);
        await _context.SaveChangesAsync();
        
        return experience;
    }

    public async Task<bool> DeleteExperienceAsync(ulong id)
    {
        var experience = await _context.CandidateExperiences.FindAsync(id);
        if (experience == null)
            return false;

        _context.CandidateExperiences.Remove(experience);
        await _context.SaveChangesAsync();
        
        return true;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SKILLS OPERATIONS
    // ═══════════════════════════════════════════════════════════════════

    public async Task AddSkillsAsync(ulong candidateId, List<CandidateSkill> skills)
    {
        await _context.CandidateSkills.AddRangeAsync(skills);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> RemoveSkillAsync(ulong candidateId, uint skillId)
    {
        var candidateSkill = await _context.CandidateSkills
            .FirstOrDefaultAsync(cs => cs.CandidateId == candidateId && cs.SkillId == skillId);

        if (candidateSkill == null)
            return false;

        _context.CandidateSkills.Remove(candidateSkill);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<List<CandidateSkill>> GetSkillsByCandidateIdAsync(ulong candidateId)
    {
        return await _context.CandidateSkills
            .Include(cs => cs.Skill)
            .Where(cs => cs.CandidateId == candidateId)
            .ToListAsync();
    }
}
