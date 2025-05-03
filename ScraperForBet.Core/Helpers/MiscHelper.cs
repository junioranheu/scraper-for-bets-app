using System.Globalization;
using System.Text.RegularExpressions;

namespace ScraperForBet.Core.Helpers
{
    public static partial class MiscHelper
    {
        public static bool IsDebug()
        {
#if !DEBUG
            return false;
#endif

            return true;
        }

        public static int GetOnlyNumbers(string input)
        {
            string digits = new([.. input.Where(char.IsDigit)]);
            return int.TryParse(digits, out int result) ? result : 0;
        }

        public static double GetOnlyNumberAsDouble(string input)
        {
            Match match = RegexNumberAsDouble().Match(input);

            if (match.Success && double.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            return 0;
        }

        public static int GetIdFromTeamImageUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception("URL is empty or null");
            }

            Match match = RegexGetIdFromTeamImageUrl1().Match(url);

            if (!match.Success)
            {
                Match match2 = RegexGetIdFromTeamImageUrl2().Match(url);

                if (!match2.Success)
                {
                    throw new Exception($"Could not extract team ID from URL: {url}");
                }

                return int.Parse(match2.Groups[1].Value);
            }

            return int.Parse(match.Groups[1].Value);
        }

        [GeneratedRegex(@"\d+(\.\d+)?")]
        private static partial Regex RegexNumberAsDouble();

        [GeneratedRegex(@"team/(\d+)/image")]
        private static partial Regex RegexGetIdFromTeamImageUrl1();

        [GeneratedRegex(@"/team/football/[^/]+/(\d+)", RegexOptions.IgnoreCase, "pt-BR")]
        private static partial Regex RegexGetIdFromTeamImageUrl2();
    }
}