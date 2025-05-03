using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ScraperForBet.Core.Helpers;
using ScraperForBet.Core.Models;
using System.Collections.ObjectModel;

namespace ScraperForBet.Core.Services
{
    public static class Scraper
    {
        public static void Run()
        {
            Console.Clear();
            ChromeDriver driver = ScraperHelper.CreateChromeDriver();
            driver.NavigateTo(url: Constants.Url);

            List<Href> hrefs = Constants.MustGetAllHrefs ? driver.GetAllHrefs() : driver.GetMainHrefs();
            List<Match> matches = [];

            foreach (Href href in hrefs)
            {
                Match match = new()
                {
                    Id = href.Id,
                    Team1 = new(),
                    Team2 = new(),
                    Predict = new()
                };

                if (string.IsNullOrEmpty(href.Url))
                {
                    continue;
                }

                driver.NavigateTo(url: href.Url);
                driver.GetTeamsName(match);
                driver.GetPercentPrediction(match);

                matches.Add(match);
            }

            driver.Dispose();
        }

        private static List<Href> GetAllHrefs(this ChromeDriver driver)
        {
            List<Href> hrefs = [];

            driver.ScrollDown();

            IWebElement buttonShowAll = driver.GetWebElement(elementName: "Show all");
            driver.ClickElement(element: buttonShowAll);

            driver.ScrollUp();

            while (!driver.IsAtBottomOfPage())
            {
                ReadOnlyCollection<IWebElement> links = driver.GetListWebElementsByDataTestId("event_cell");

                foreach (var link in links)
                {
                    try
                    {
                        string? id = link.GetAttribute("data-id");
                        string? href = link.GetAttribute("href");
                        Console.WriteLine(href);

                        Href newHref = new()
                        {
                            Id = Convert.ToInt32(id),
                            Url = href
                        };

                        hrefs.Add(newHref);
                    }
                    catch (Exception)
                    {
                    }
                }

                driver.ScrollDownOnly100VH();
                Thread.Sleep(100);
            }

            List<Href> hrefsUnique = [.. hrefs.DistinctBy(x => x?.Id)];

            driver.ScrollUp();

            return hrefsUnique;
        }

        private static List<Href> GetMainHrefs(this ChromeDriver driver)
        {
            List<Href?> hrefs = [];

            driver.ScrollUp();

            ReadOnlyCollection<IWebElement> links = driver.GetListWebElementsByDataTestId("event_cell");

            foreach (var link in links)
            {
                try
                {
                    string? id = link.GetAttribute("data-id");
                    var href = link.GetAttribute("href");
                    Console.WriteLine(href);

                    Href newHref = new()
                    {
                        Id = Convert.ToInt32(id),
                        Url = href
                    };

                    hrefs.Add(newHref);
                }
                catch (Exception)
                {
                }
            }

            List<Href> hrefsUnique = [.. hrefs.DistinctBy(x => x?.Id)];

            driver.ScrollUp();

            return hrefsUnique;
        }

        private static void GetTeamsName(this ChromeDriver driver, Match match)
        {
            ReadOnlyCollection<IWebElement> teamsElements_Names = driver.GetListWebElementsByClass("fIvzGZ"); // Names;
            IWebElement? teamsElements_Team1 = driver.GetListWebElementsByDataTestId("left_team").FirstOrDefault();
            IWebElement? teamsElements_Team2 = driver.GetListWebElementsByDataTestId("right_team").FirstOrDefault();

            if (teamsElements_Names is null || teamsElements_Names.Count < 2 || teamsElements_Team1 is null || teamsElements_Team2 is null)
            {
                throw new Exception("There was a failure in retrieving the information for both teams");
            }

            match.Team1.Image = teamsElements_Team1.FindElement(By.TagName("a")).GetAttribute("href");
            match.Team1.Id = MiscHelper.GetIdFromTeamImageUrl(match.Team1.Image);
            match.Team1.Name = teamsElements_Names[0].Text;

            match.Team2.Image = teamsElements_Team2.FindElement(By.TagName("a")).GetAttribute("href");
            match.Team2.Id = MiscHelper.GetIdFromTeamImageUrl(match.Team2.Image);
            match.Team2.Name = teamsElements_Names[1].Text;
        }

        private static List<double> GetPercentPrediction(this ChromeDriver driver, Match match, int exceptionCount = 0)
        {
            const int maxTry = 3;
            double?[] predictions = new double?[3];
            ReadOnlyCollection<IWebElement> predictionElements = driver.GetListWebElementsByDataTestId("prediction_option");

            for (int i = 0; i < predictionElements.Count; i++)
            {
                try
                {
                    IWebElement element = predictionElements[i];

                    IWebElement percentSpan = element.FindElement(By.TagName("span"));
                    double percent = MiscHelper.GetOnlyNumberAsDouble(percentSpan.Text);

                    predictions[i] = percent;
                }
                catch (NoSuchElementException)
                {
                    if (exceptionCount >= maxTry)
                    {
                        // throw new NoSuchElementException($"Prediction elements not found.");
                        break;
                    }

                    // Muitas das vezes, será necessário realizar o voto antes de obter os dados,
                    // Portanto se faz necessário realizar o voto manual e depois a tentativa de obtenção dos dados;
                    exceptionCount++;

                    IWebElement buttonDraw = predictionElements[1];
                    driver.ClickElementSmart(element: buttonDraw);

                    driver.GetPercentPrediction(match, exceptionCount);
                }
            }

            // Caso aconteça o bug de não aparecer a porcentagem da terceira prediction, crie-o manualmente;
            int? firstNullIndex = Array.FindIndex(predictions, p => p == null);

            if (firstNullIndex != -1)
            {
                double missingThirdPrediction = 100 - (predictions.Sum(x => x ?? 0));
                predictions[firstNullIndex.GetValueOrDefault()] = missingThirdPrediction;
            }

            match.Predict.WinningPercentage_Team1 = predictions[0].GetValueOrDefault();
            match.Predict.DrawingPercentage = predictions[1].GetValueOrDefault();
            match.Predict.WinningPercentage_Team2 = predictions[2].GetValueOrDefault();
            match.Team1.WinningPercentage = predictions[0].GetValueOrDefault();
            match.Team2.WinningPercentage = predictions[2].GetValueOrDefault();

            List<double> filledPredictions = [.. predictions.Where(x => x.HasValue).Select(x => x.GetValueOrDefault())];

            return filledPredictions;
        }
    }
}