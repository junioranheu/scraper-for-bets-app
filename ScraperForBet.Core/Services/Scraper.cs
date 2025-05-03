using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ScraperForBet.Core.Helpers;
using ScraperForBet.Core.Models;
using System.Collections.ObjectModel;

namespace ScraperForBet.Core.Services
{
    public static class Scraper
    {
        public static void Main(bool mustGetAll = false)
        {
            Console.Clear();
            ChromeDriver driver = ScraperHelper.CreateChromeDriver();
            driver.NavigateTo(url: Constants.Url);

            //IWebElement buttonFinished = driver.GetWebElement(elementName: "Finished", findElementType: FindElementTypeEnum.Contains);
            //driver.ClickElement(element: buttonFinished);

            //Thread.Sleep(100);
            //IWebElement buttonUpcoming = driver.GetWebElement(elementName: "Upcoming");
            //driver.ClickElement(element: buttonUpcoming);

            List<string> hrefsMain = mustGetAll ? driver.GetAllHrefs() : driver.GetMainHrefs();

            foreach (string href in hrefsMain)
            {
                driver.NavigateTo(url: href);
                driver.GetPercentPrediction();
            }

            driver.Dispose();
        }

        private static List<string> GetAllHrefs(this ChromeDriver driver)
        {
            List<string?> hrefs = [];

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
                        var href = link.GetAttribute("href");
                        Console.WriteLine(href);
                        hrefs.Add(href);
                    }
                    catch (Exception)
                    {
                    }
                }

                driver.ScrollDownOnly100VH();
                Thread.Sleep(100);
            }

            List<string> hrefsUnique = [.. hrefs.Distinct()];

            driver.ScrollUp();

            return hrefsUnique;
        }

        private static List<string> GetMainHrefs(this ChromeDriver driver)
        {
            List<string?> hrefs = [];

            driver.ScrollUp();

            ReadOnlyCollection<IWebElement> links = driver.GetListWebElementsByDataTestId("event_cell");

            foreach (var link in links)
            {
                try
                {
                    var href = link.GetAttribute("href");
                    Console.WriteLine(href);
                    hrefs.Add(href);
                }
                catch (Exception)
                {
                }
            }

            List<string> hrefsUnique = [.. hrefs.Distinct()];

            driver.ScrollUp();

            return hrefsUnique;
        }

        private static void GetTeamsName(this ChromeDriver driver)
        {
            //var predictionElements = driver.FindElements(By.CssSelector("[data-testid='prediction_option']"));

            //foreach (var element in predictionElements)
            //{
            //    try
            //    {
            //        // Find the <span> inside and get its text
            //        var percentSpan = element.FindElement(By.TagName("span"));
            //        Console.WriteLine(percentSpan.Text); // e.g., "82%"
            //    }
            //    catch (NoSuchElementException)
            //    {
            //        Console.WriteLine("No span found in prediction_option element.");
            //    }
            //}
        }

        private static void GetPercentPrediction(this ChromeDriver driver)
        {
            //ReadOnlyCollection<IWebElement> predictionElements = driver.GetListWebElementsByDataTestId("prediction_option");

            //            foreach (var element in predictionElements)
            //{
            //    try
            //    {
            //        // Find the <span> inside and get its text
            //        var percentSpan = element.FindElement(By.TagName("span"));
            //        Console.WriteLine(percentSpan.Text); // e.g., "82%"
            //    }
            //    catch (NoSuchElementException)
            //    {
            //        ReadOnlyCollection<IWebElement> predictionElements = driver.GetListWebElementsByDataTestId("prediction_option");
            //        IWebElement buttonFinished = driver.GetWebElement(elementName: "[data-testid='prediction_option']");
            //        driver.ClickElement(element: buttonFinished);

            //        Console.WriteLine("No span found in prediction_option element.");
            //    }
            //}
        }
    }
}