using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Job;

public class FilterOptionsDto
{
    public List<LocationOptionDto> Cities { get; set; } = new();
    public List<IndustryOptionDto> Industries { get; set; } = new();
    public List<WorkTypeOptionDto> WorkTypes { get; set; } = new();
    public List<JobLevelOptionDto> Levels { get; set; } = new();
}

public class LocationOptionDto
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class IndustryOptionDto
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class WorkTypeOptionDto
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class JobLevelOptionDto
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;
}
