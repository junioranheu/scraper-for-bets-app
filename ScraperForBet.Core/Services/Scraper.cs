using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ScraperForBet.Core.Helpers;
using ScraperForBet.Core.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ScraperForBet.Core.Services
{
    public static class Scraper
    {
        public static FinalResponse Run()
        {
            Console.Clear();

            DateTime startDate = MiscHelper.GetDateTime();
            Stopwatch stopwatch = new();
            stopwatch.Start();

            ChromeDriver driver = ScraperHelper.CreateChromeDriver();
            driver.NavigateTo(url: Constants.Url);

            List<Href> hrefs = Constants.MustGetAllHrefs ? driver.GetAllHrefs() : driver.GetMainHrefs();
            List<Game> games = [];

            foreach (Href href in hrefs)
            {
                Game game = new()
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
                driver.GetTeamsName(game);
                driver.GetPercentPrediction(game);

                games.Add(game);
            }

            driver.Dispose();
            FinalResponse finalResponse = GenerateFinalResponse(games, stopwatch, startDate);

            return finalResponse;
        }

        #region methods
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

        private static void GetTeamsName(this ChromeDriver driver, Game game)
        {
            ReadOnlyCollection<IWebElement> teamsElements_Names = driver.GetListWebElementsByClass("fIvzGZ"); // Names;
            ReadOnlyCollection<IWebElement> teamsElements_Misc = driver.GetListWebElementsByClass("jmRURX"); // Etc;

            if (teamsElements_Names is null || teamsElements_Names.Count < 2 || teamsElements_Misc is null || teamsElements_Misc.Count < 2)
            {
                throw new Exception("There was a failure in retrieving the information for both teams");
            }

            game.Team1.Image = teamsElements_Misc[0].GetAttribute("src");
            game.Team1.Id = MiscHelper.GetIdFromTeamImageUrl(game.Team1.Image);
            game.Team1.Name = teamsElements_Names[0].Text;

            game.Team2.Image = teamsElements_Misc[1].GetAttribute("src");
            game.Team2.Id = MiscHelper.GetIdFromTeamImageUrl(game.Team2.Image);
            game.Team2.Name = teamsElements_Names[1].Text;
        }

        private static List<double> GetPercentPrediction(this ChromeDriver driver, Game game, int exceptionCount = 0)
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

                    driver.GetPercentPrediction(game, exceptionCount);
                }
            }

            // Caso aconteça o bug de não aparecer a porcentagem da terceira prediction, crie-o manualmente;
            int? firstNullIndex = Array.FindIndex(predictions, p => p == null);

            if (firstNullIndex != -1)
            {
                double missingThirdPrediction = 100 - (predictions.Sum(x => x ?? 0));
                predictions[firstNullIndex.GetValueOrDefault()] = missingThirdPrediction;
            }

            game.Predict.WinningPercentage_Team1 = predictions[0].GetValueOrDefault();
            game.Predict.DrawingPercentage = predictions[1].GetValueOrDefault();
            game.Predict.WinningPercentage_Team2 = predictions[2].GetValueOrDefault();
            game.Team1.WinningPercentage = predictions[0].GetValueOrDefault();
            game.Team2.WinningPercentage = predictions[2].GetValueOrDefault();

            List<double> filledPredictions = [.. predictions.Where(x => x.HasValue).Select(x => x.GetValueOrDefault())];

            return filledPredictions;
        }

        private static FinalResponse GenerateFinalResponse(List<Game> games, Stopwatch stopwatch, DateTime startDate)
        {
            TimeSpan elapsed = stopwatch.Elapsed;

            FinalResponse response = new()
            {
                Games = games,
                StartDate = startDate,
                FinishDate = MiscHelper.GetDateTime(),
                ElapsedTotalSeconds = elapsed.TotalSeconds,
                ElapsedTotalMinutes = elapsed.TotalMinutes
            };

            return response;
        }
        #endregion
    }
}