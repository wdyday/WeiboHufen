using OpenQA.Selenium.Remote;

namespace DotNetSpiderLite.Extension.Downloader
{
	public interface IWebDriverHandler
	{
		bool Handle(RemoteWebDriver driver);
	}
}
