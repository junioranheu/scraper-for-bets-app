namespace ScraperForBet.Core.Models
{
    public sealed class FinalResponse
    {
        public List<Game>? Games { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public double ElapsedTotalSeconds { get; set; }
        public double ElapsedTotalMinutes { get; set; }
    }
}