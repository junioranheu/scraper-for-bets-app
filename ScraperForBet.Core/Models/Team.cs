namespace ScraperForBet.Core.Models;

public sealed class Team
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? Goals { get; set; } = 0;
    public double WinningPercentage { get; set; }
    public string? Image { get; set; }
}