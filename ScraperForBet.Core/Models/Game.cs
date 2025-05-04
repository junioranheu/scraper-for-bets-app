using ScraperForBet.Core.Enums;

namespace ScraperForBet.Core.Models;

public sealed class Game
{
    public int Id { get; set; }
    public GameStatusEnum Status { get; set; }
    public DateTime DateTime { get; set; }
    public string? Venue { get; set; }
    public string? About { get; set; }
    public required League League { get; set; }
    public required Team Team1 { get; set; }
    public required Team Team2 { get; set; }
    public required Predict Predict { get; set; }
}