using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ScraperForBet.Core.Enums;

namespace ScraperForBet.Core.Helpers
{
    public static class ScraperHelper
    {
        public static ChromeDriver CreateChromeDriver()
        {
            ChromeOptions options = new();

            if (!MiscHelper.IsDebug())
            {
                options.AddArgument("--headless=new"); // Não abrir o navegador;
            }

            // options.AddArgument("--disable-javascript"); // Limitar o uso do javascript; 
            options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2); // Desabilitar imagens;
            options.AddUserProfilePreference("profile.managed_default_content_settings.stylesheets", 2); // Desabilitar CSS;
            options.AddUserProfilePreference("profile.managed_default_content_settings.fonts", 2); // Desabilitar fontes;

            ChromeDriver driver = new(options);

            return driver;
        }

        public static void NavigateTo(this ChromeDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public static IWebElement GetWebElement(this ChromeDriver driver, string elementName, FindElementTypeEnum findElementType = FindElementTypeEnum.Equals, double waitTimeInSeconds = 1.5)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitTimeInSeconds));

            IWebElement element;
            string xpath = findElementType == FindElementTypeEnum.Contains ? $"//*[contains(text(),'{elementName}')]" : $"//*[text()='{elementName}']";

            try
            {
                element = wait.Until(x => x.FindElement(By.XPath(xpath)));

                if (element is null)
                {
                    throw new NoSuchElementException($"Element with text '{elementName}' not found.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                throw new NoSuchElementException($"Element with text '{elementName}' not found within the timeout.");
            }

            return element;
        }

        public static void ClickElement(this ChromeDriver driver, IWebElement element)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                Console.WriteLine($"Button {element.TagName} clicked successfully!");
            }
            catch (Exception ex)
            {
                throw new Exception($"There was an error clicking the element '{element.TagName}': {ex.Message}.");
            }
        }

        public static void ScrollDown(this ChromeDriver driver)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        }

        public static void ScrollDownOnly100VH(this ChromeDriver driver)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0, window.innerHeight);");
        }

        public static void ScrollUp(this ChromeDriver driver)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0);");
        }

        public static bool IsAtBottomOfPage(this ChromeDriver driver)
        {
            // Obtém a altura da janela visível;
            var viewportHeight = Convert.ToInt32(((IJavaScriptExecutor)driver).ExecuteScript("return window.innerHeight"));

            // Obtém a altura total da página;
            var documentHeight = Convert.ToInt32(((IJavaScriptExecutor)driver).ExecuteScript("return document.documentElement.scrollHeight"));

            // Obtém a posição atual do scroll;
            var scrollPosition = Convert.ToInt32(((IJavaScriptExecutor)driver).ExecuteScript("return window.pageYOffset"));

            // Verifica se a posição do scroll + a altura da janela é maior ou igual à altura total da página;
            return scrollPosition + viewportHeight >= documentHeight - 10; // Subtraímos um valor pequeno para lidar com pequenas variações;
        }

        public static void Dispose(this ChromeDriver driver)
        {
            driver.Quit();
            Environment.Exit(0);
        }
    }
}