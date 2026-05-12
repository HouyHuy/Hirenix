namespace Hirenix.Application.DTOs.Admin;

public class AnalyticsDto
{
    public string Period { get; set; } = string.Empty;
    public List<DataPointDto> UsersGrowth { get; set; } = new();
    public List<DataPointDto> JobsGrowth { get; set; } = new();
    public List<DataPointDto> ApplicationsGrowth { get; set; } = new();
}

public class DataPointDto
{
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}
