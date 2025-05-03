using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ScraperForBet.Core.Helpers;
using System.Collections.ObjectModel;

namespace ScraperForBet.Core.Services
{
    public static class Scraper
    {
        public static void Main()
        {
            Console.Clear();
            ChromeDriver driver = ScraperHelper.CreateChromeDriver();
            driver.NavigateTo(url: "https://www.sofascore.com/");

            //IWebElement buttonFinished = driver.GetWebElement(elementName: "Finished", findElementType: FindElementTypeEnum.Contains);
            //driver.ClickElement(element: buttonFinished);

            //Thread.Sleep(100);
            //IWebElement buttonUpcoming = driver.GetWebElement(elementName: "Upcoming");
            //driver.ClickElement(element: buttonUpcoming);

            List<string> hrefsMain = driver.GetMainHrefs();

            foreach (string href in hrefsMain)
            {
                driver.NavigateTo(url: href);
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
                ReadOnlyCollection<IWebElement> links = driver.FindElements(By.CssSelector("a[data-testid='event_cell']"));

                foreach (var link in links)
                {
                    var href = link.GetAttribute("href");
                    Console.WriteLine(href);
                    hrefs.Add(href);
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

            ReadOnlyCollection<IWebElement> links = driver.FindElements(By.CssSelector("a[data-testid='event_cell']"));

            foreach (var link in links)
            {
                var href = link.GetAttribute("href");
                Console.WriteLine(href);
                hrefs.Add(href);
            }

            List<string> hrefsUnique = [.. hrefs.Distinct()];

            driver.ScrollUp();

            return hrefsUnique;
        }
    }
}