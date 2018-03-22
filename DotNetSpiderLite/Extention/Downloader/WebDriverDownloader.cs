using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetSpiderLite.Core;
using DotNetSpiderLite.Core.Downloader;
using DotNetSpiderLite.Core.Infrastructure;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using DotNetSpiderLite.Core.Redial;
using System.Runtime.InteropServices;
using DotNetSpiderLite.Extension.Infrastructure;

namespace DotNetSpiderLite.Extension.Downloader
{
	public class WebDriverDownloader : BaseDownloader
	{
		private readonly object _locker = new object();
		private IWebDriver _webDriver;
		private readonly int _webDriverWaitTime;
		private bool _isLogined;
		private readonly Browser _browser;
		private readonly Option _option;
		private bool _isDisposed;

		public event Action<RemoteWebDriver> NavigateCompeleted;

		public LoginHandler Login { get; set; }

        #region [WDY]

        /// <summary>
        /// [WDY] 返回弹出窗口内容
        ///  ReturnPopupPage = true && 存在弹出窗口, 返回弹出窗口内容
        /// </summary>
        private bool _returnPopupPage = false;
        
        /// <summary>
        /// [WDY] 表单填充+提交
        /// </summary>
        public FormSubmitHandler FormSubmit { get; set; }

        /// <summary>
        /// [WDY] Iframe 设置
        /// 当页面有iframe时, 如果 IframeOption.ReturnFrame = true, 则返回iframe
        /// </summary>
        public IframeOption IframeOption { get; set; }

        public WebDriverDownloader(Browser browser, int webDriverWaitTime = 200, LoginHandler loginHandler = null, Option option = null, bool returnPopupPage = false)
        {
            _webDriverWaitTime = webDriverWaitTime;
            _browser = browser;
            _option = option ?? new Option();
            Login = loginHandler;
            _returnPopupPage = returnPopupPage;

            if (browser == Browser.Firefox && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Task.Factory.StartNew(() =>
                {
                    while (!_isDisposed)
                    {
                        IntPtr maindHwnd = WindowsFormUtils.FindWindow(null, "plugin-container.exe - 应用程序错误");
                        if (maindHwnd != IntPtr.Zero)
                        {
                            WindowsFormUtils.SendMessage(maindHwnd, WindowsFormUtils.WmClose, 0, 0);
                        }
                        Thread.Sleep(500);
                    }
                });
            }
        }

        #endregion

        public WebDriverDownloader(Browser browser, int webDriverWaitTime = 200, LoginHandler loginHandler = null, Option option = null)
		{
			_webDriverWaitTime = webDriverWaitTime;
			_browser = browser;
			_option = option ?? new Option();
			Login = loginHandler;

			if (browser == Browser.Firefox && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Task.Factory.StartNew(() =>
				{
					while (!_isDisposed)
					{
						IntPtr maindHwnd = WindowsFormUtils.FindWindow(null, "plugin-container.exe - 应用程序错误");
						if (maindHwnd != IntPtr.Zero)
						{
							WindowsFormUtils.SendMessage(maindHwnd, WindowsFormUtils.WmClose, 0, 0);
						}
						Thread.Sleep(500);
					}
				});
			}
		}


		public WebDriverDownloader(Browser browser, LoginHandler loginHandler) : this(browser, 200, loginHandler, null)
		{
		}

		public WebDriverDownloader(Browser browser) : this(browser, 200, null, null)
		{
		}

		public WebDriverDownloader(Browser browser, Option option = null) : this(browser, 200, null, option)
		{
		}

		public override void Dispose()
		{
			_isDisposed = true;
			_webDriver?.Quit();
		}

		protected override Page DowloadContent(Request request, ISpider spider)
		{
			Site site = spider.Site;
			try
			{
				lock (_locker)
				{
					_webDriver = _webDriver ?? WebDriverExtensions.Open(_browser, _option);

					if (!_isLogined && Login != null)
					{
						_isLogined = Login.Handle(_webDriver as RemoteWebDriver);
						if (!_isLogined)
						{
							throw new DownloadException("Login failed. Please check your login codes.");
						}
					}
				}

				Uri uri = request.Url;

				//#if NET_CORE
				//				string query = string.IsNullOrEmpty(uri.Query) ? "" : $"?{WebUtility.UrlEncode(uri.Query.Substring(1, uri.Query.Length - 1))}";
				//#else
				//				string query = string.IsNullOrEmpty(uri.Query) ? "" : $"?{HttpUtility.UrlPathEncode(uri.Query.Substring(1, uri.Query.Length - 1))}";
				//#endif
				//				string realUrl = $"{uri.Scheme}://{uri.DnsSafeHost}{(uri.Port == 80 ? "" : ":" + uri.Port)}{uri.AbsolutePath}{query}";

				//var domainUrl = $"{uri.Scheme}://{uri.DnsSafeHost}{(uri.Port == 80 ? "" : ":" + uri.Port)}";

				//var options = _webDriver.Manage();
				//if (options.Cookies.AllCookies.Count == 0 && spider.Site.Cookies?.PairPart.Count > 0)
				//{
				//	_webDriver.Url = domainUrl;
				//	options.Cookies.DeleteAllCookies();
				//	if (spider.Site.Cookies != null)
				//	{
				//		foreach (var c in spider.Site.Cookies.PairPart)
				//		{
				//			options.Cookies.AddCookie(new Cookie(c.Key, c.Value));
				//		}
				//	}
				//}

				string realUrl = request.Url.ToString();

				NetworkCenter.Current.Execute("webdriver-download", () =>
				{
					_webDriver.Navigate().GoToUrl(realUrl);

					NavigateCompeleted?.Invoke((RemoteWebDriver)_webDriver);
				});

				Thread.Sleep(_webDriverWaitTime);

                #region [WDY]

                #region [WDY] 表单填充+提交

                if (FormSubmit != null)
                {
                    FormSubmit.Handle(_webDriver as RemoteWebDriver);
                }

                #endregion

                #region [WDY] 弹出页面

                if (_returnPopupPage)
                {
                    var currentWindowHandle = _webDriver.CurrentWindowHandle;
                    var popupWindowHandle = string.Empty;
                    foreach (var handle in _webDriver.WindowHandles)
                    {
                        if (handle != currentWindowHandle)
                        {
                            popupWindowHandle = handle;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(popupWindowHandle))
                    {
                        _webDriver.SwitchTo().Window(popupWindowHandle);
                    }
                }

                #endregion

                // [WDY] 页面内容, 禁止移动位置
                string content = _webDriver.PageSource;

                #region [WDY] iframe
                /*
                    [WDY] Iframe 设置
                    ReturnIframe = true 时, 返回 Iframe 的 html
                */

                string iframeContent = null;
                if (_option.IframeOption != null && _option.IframeOption.ReturnIframe)
                {
                    if (!string.IsNullOrEmpty(_option.IframeOption.IframeUrl) || !string.IsNullOrEmpty(_option.IframeOption.IframeName))
                    {
                        var iframeElement = !string.IsNullOrEmpty(_option.IframeOption.IframeUrl)
                            ? _webDriver.FindElement(By.XPath($"//iframe[contains(@src,'{_option.IframeOption.IframeUrl}')]"))
                            : _webDriver.FindElement(By.XPath($"//iframe[contains(@name,'{_option.IframeOption.IframeName}')]"));

                        _webDriver.SwitchTo().Frame(iframeElement); // 此处 switchTo iframe 后, 下边 _webDriver.PageSource 则会返回iframe的html

                        iframeContent = _webDriver.PageSource;

                        //_webDriver.SwitchTo().ParentFrame();
                    }
                }

                #endregion

                #endregion

                Page page = new Page(request, site.RemoveOutboundLinks ? site.Domains : null)
				{
					Content = _webDriver.PageSource,
                    IframeContent = iframeContent,  //[WDY]
                    TargetUrl = _webDriver.Url,
					Title = _webDriver.Title
				};

				// 结束后要置空, 这个值存到Redis会导置无限循环跑单个任务
				//request.PutExtra(Request.CycleTriedTimes, null);
				return page;
			}
			catch (DownloadException de)
			{
				Page page = new Page(request, null) { Exception = de };
				if (site.CycleRetryTimes > 0)
				{
					page = Spider.AddToCycleRetry(request, site);
				}
				Logger.AllLog(spider.Identity, $"下载 {request.Url} 失败: {de.Message}.", NLog.LogLevel.Warn);
				return page;
			}
			catch (Exception e)
			{
				Logger.AllLog(spider.Identity, $"下载 {request.Url} 失败: {e.Message}.", NLog.LogLevel.Warn);
				Page page = new Page(request, null) { Exception = e };
				return page;
			}
		}

	}
}
