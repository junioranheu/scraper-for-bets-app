using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.UI;
using ScraperForBet.Core.Enums;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

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
            // options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2); // Desabilitar imagens;
            options.AddUserProfilePreference("profile.managed_default_content_settings.stylesheets", 2); // Desabilitar CSS;
            options.AddUserProfilePreference("profile.managed_default_content_settings.fonts", 2); // Desabilitar fontes;

            ChromeDriver driver = new(options);

            return driver;
        }

        public static SafariDriver CreateSafariDriver()
        {
            SafariDriverService service = SafariDriverService.CreateDefaultService();
            SafariDriver driver = new(service);

            return driver;
        }

        public static IWebDriver CreateDriverBasedOnOS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return CreateChromeDriver();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return CreateSafariDriver();
            }
            else
            {
                throw new NotSupportedException("Unsupported Operating System: This application only supports ChromeDriver on Windows and SafariDriver on macOS.");
            }
        }

        public static void NavigateTo(this IWebDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public static IWebElement GetWebElement(this IWebDriver driver, string elementName, FindElementTypeEnum findElementType = FindElementTypeEnum.Equals, double waitTimeInSeconds = 1.5)
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

        public static ReadOnlyCollection<IWebElement> GetListWebElementsByDataId(this IWebDriver driver, string dataId, string dataIdObject = "data-testid")
        {
            ReadOnlyCollection<IWebElement>? elements = driver.FindElements(By.CssSelector($"[{dataIdObject}='{dataId}']"));

            if (elements is null || elements?.Count == 0)
            {
                throw new NoSuchElementException($"Elements with id '{dataId}' not found.");
            }

            return elements!;
        }

        public static ReadOnlyCollection<IWebElement> GetListWebElementsByClass(this IWebDriver driver, string className)
        {
            ReadOnlyCollection<IWebElement>? elements = driver.FindElements(By.ClassName(className));

            if (elements is null || elements?.Count == 0)
            {
                throw new NoSuchElementException($"Elements with class '{className}' not found.");
            }

            return elements!;
        }

        public static void ClickElement(this IWebDriver driver, IWebElement element)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                Console.WriteLine($"Button {element.TagName} clicked successfully!");
            }
            catch (Exception ex)
            {
                throw new Exception($"There was an error clicking the element ({nameof(ClickElement)}) '{element.TagName}': {ex.Message}.");
            }
        }

        public static void ClickElementSmart(this IWebDriver driver, IWebElement element)
        {
            try
            {
                // Try native click first;
                element.Click();
                Console.WriteLine($"Button {element.TagName} clicked successfully!");
            }
            catch
            {
                try
                {
                    // Scroll and JS click;
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);
                    Thread.Sleep(100);
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                    Console.WriteLine($"Button {element.TagName} clicked successfully!");
                }
                catch (Exception ex)
                {
                    throw new Exception($"There was an error clicking the element ({nameof(ClickElementSmart)}) '{element.TagName}': {ex.Message}.");
                }
            }
        }

        public static void ScrollDown(this IWebDriver driver)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        }

        public static void ScrollDownOnly100VH(this IWebDriver driver)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0, window.innerHeight);");
        }

        public static void ScrollUp(this IWebDriver driver)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0);");
        }

        public static bool IsAtBottomOfPage(this IWebDriver driver)
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

        public static void Dispose(this IWebDriver driver)
        {
            driver.Quit();
            Environment.Exit(0);
        }
    }
}