using DotNetSpiderLite.Core;
using DotNetSpiderLite.Core.Selector;
using DotNetSpiderLite.Extension.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetSpiderLite.Extension.Downloader
{
    /// <summary>
    /// [WDY] WebDriver 读取页面元素
    /// </summary>
    public class Utils
    {
        public static IWebElement FindElement(RemoteWebDriver webDriver, Selector element)
        {
            try
            {

                switch (element.Type)
                {
                    case SelectorType.XPath:
                        {
                            return webDriver.FindElementByXPath(element.Expression);
                        }
                    case SelectorType.Css:
                        {
                            return webDriver.FindElementByCssSelector(element.Expression);
                        }
                    case SelectorType.Id:
                        {
                            return webDriver.FindElementById(element.Expression);
                        }
                    default: return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
