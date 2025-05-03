namespace ScraperForBet.Core.Models
{
    public sealed class Team
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double WinningPercentage { get; set; }
        public string? Image { get; set; }
    }
}