namespace ScraperForBet.Core.Models
{
    public sealed class Match
    {
        public int Id { get; set; }
        public required Team Team1 { get; set; }
        public required Team Team2 { get; set; }
        public required Predict Predict { get; set; }
    }
}