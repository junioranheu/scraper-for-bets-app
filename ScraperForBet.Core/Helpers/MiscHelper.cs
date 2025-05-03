namespace ScraperForBet.Core.Helpers
{
    public static class MiscHelper
    {
        public static bool IsDebug()
        {
#if !DEBUG
return false;
#endif

            return true;
        }
    }
}