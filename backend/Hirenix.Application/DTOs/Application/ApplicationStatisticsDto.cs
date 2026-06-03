namespace Hirenix.Application.DTOs.Application;

public class ApplicationStatisticsDto
{
    public int Total { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<ulong, int> ByJob { get; set; } = new();
}
