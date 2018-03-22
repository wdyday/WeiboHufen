using DotNetSpiderLite.Core;
using DotNetSpiderLite.Core.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.PhantomJS;
using System;
using System.IO;
#if !NET_CORE
using System.Drawing;
#endif

namespace DotNetSpiderLite.Extension.Infrastructure
{
	public class Option
	{
		private string _proxy;

		public static Option Default = new Option();

        public bool LoadImage { get; set; } = true;

		public bool AlwaysLoadNoFocusLibrary { get; set; } = true;

		public bool LoadFlashPlayer { get; set; } = true;

		public bool Headless { get; set; }

		public string Proxy
		{
			get => _proxy;
			set
			{
				string v = value;
				if (string.IsNullOrEmpty(v))
				{
					_proxy = v;
				}
				else
				{
					string[] tmp = v.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

					if (tmp.Length == 2)
					{
						if (RegexUtil.IpAddressRegex.IsMatch(tmp[0]) && int.TryParse(tmp[1], out _))
						{
							_proxy = v;
							return;
						}
					}

					throw new SpiderException("Proxy string should be like 192.168.1.100:8080.");
				}
			}
		}

		public string ProxyAuthentication { get; set; }

        #region [WDY]

        /// <summary>
        /// [WDY] 默认语言
        /// </summary>
        public LanguageEnum Language { get; set; } = LanguageEnum.English;

        /// <summary>
        /// [WDY] Iframe 设置
        /// ReturnIframe = true 时, 返回 Iframe 的 html
        /// </summary>
        public IframeOption IframeOption { get; set; }

        public Option() { }

        public Option(LanguageEnum language
            , IframeOption iframeOption = null
            , bool loadImage = false, bool alwaysLoadNoFocusLibrary = false, bool loadFlashPlayer = false)
        {
            Language = language;
            IframeOption = iframeOption;
            LoadImage = loadImage;
            AlwaysLoadNoFocusLibrary = alwaysLoadNoFocusLibrary;
            LoadFlashPlayer = loadFlashPlayer;
        }

        #endregion
    }

    #region [WDY]
    /// <summary>
    /// [WDY] 默认语言
    /// </summary>
    public enum LanguageEnum
    {
        English,
        Chinese
    }

    /// <summary>
    /// [WDY] Iframe 设置
    /// ReturnIframe = true 时, 返回 Iframe 的 html
    /// </summary>
    public class IframeOption
    {
        public IframeOption(bool returnIframe, string iframeUrl, string iframeName)
        {
            ReturnIframe = returnIframe;
            IframeUrl = iframeUrl;
            IframeName = iframeName;
        }

        public bool ReturnIframe { get; set; } = false;

        public string IframeUrl { get; set; }

        public string IframeName { get; set; }
    }

    #endregion

    public static class WebDriverExtensions
	{
#if !NET_CORE
		//public static Image ElementSnapshot(this IWebElement element, Bitmap screenSnapshot)
		//{
		//	Size size = new Size(Math.Min(element.Size.Width, screenSnapshot.Width),
		//		Math.Min(element.Size.Height, screenSnapshot.Height));
		//	Rectangle crop = new Rectangle(element.Location, size);
		//	return screenSnapshot.Clone(crop, screenSnapshot.PixelFormat);
		//}
#endif
		public static IWebDriver Open(Browser browser, Option option)
		{
			IWebDriver e = null;
			switch (browser)
			{
				case Browser.Phantomjs:
					var phantomJsDriverService = PhantomJSDriverService.CreateDefaultService();
					if (!string.IsNullOrEmpty(option.Proxy))
					{
						phantomJsDriverService.Proxy = option.Proxy;
						phantomJsDriverService.ProxyAuthentication = option.ProxyAuthentication;
					}
					e = new PhantomJSDriver(phantomJsDriverService);
					break;
				case Browser.Firefox:
					string path = Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\Mozilla\Firefox\Profiles\";
					string[] pathsToProfiles = Directory.GetDirectories(path, "*.webdriver", SearchOption.TopDirectoryOnly);
					var profile = pathsToProfiles.Length == 1 ? new FirefoxProfile(pathsToProfiles[0], false) : new FirefoxProfile();
					if (!option.AlwaysLoadNoFocusLibrary)
					{
						profile.AlwaysLoadNoFocusLibrary = true;
					}

					if (!option.LoadImage)
					{
						profile.SetPreference("permissions.default.image", 2);
					}
					if (!option.LoadFlashPlayer)
					{
						profile.SetPreference("dom.ipc.plugins.enabled.libflashplayer.so", "false");
					}
					if (!string.IsNullOrEmpty(option.Proxy))
					{
						string[] p = option.Proxy.Split(':');
						string host = p[0];
						int port = Convert.ToInt32(p[1]);
						profile.SetPreference("network.proxy.ftp_port", port);
						profile.SetPreference("network.proxy.gopher", host);
						profile.SetPreference("network.proxy.gopher_port", port);
						profile.SetPreference("network.proxy.http", host);
						profile.SetPreference("network.proxy.http_port", port);
						profile.SetPreference("network.proxy.no_proxies_on", "localhost,127.0.0.1,<-loopback>");
						profile.SetPreference("network.proxy.share_proxy_settings", true);
						profile.SetPreference("network.proxy.socks", host);
						profile.SetPreference("network.proxy.socks_port", port);
						profile.SetPreference("network.proxy.ssl", host);
						profile.SetPreference("network.proxy.ssl_port", port);
						profile.SetPreference("network.proxy.type", 1);
					}

					e = new FirefoxDriver(profile);
					break;
				case Browser.Chrome:
					ChromeDriverService cds = ChromeDriverService.CreateDefaultService(Env.BaseDirectory);
					cds.HideCommandPromptWindow = true;
					ChromeOptions opt = new ChromeOptions();
					if (!option.LoadImage)
					{
						opt.AddUserProfilePreference("profile", new { default_content_setting_values = new { images = 2 } });
					}
					if (!string.IsNullOrEmpty(option.Proxy))
					{
						opt.Proxy = new Proxy() { HttpProxy = option.Proxy };
					}
					if (option.Headless)
					{
						opt.AddArgument("--headless");
                    }
                    #region [WDY]
                    opt.AddArgument($"--lang={GetChromeLanguageName(option.Language)}");
                    #endregion

                    e = new ChromeDriver(cds, opt);
					break;
			}
			return e;
		}

        #region [WDY]
        public static string GetChromeLanguageName(LanguageEnum languageEnum)
        {
            var name = "en";
            switch (languageEnum)
            {
                case LanguageEnum.English: name = "en"; break;
                case LanguageEnum.Chinese: name = "zh-CN"; break;
                default: name = "en"; break;
            }
            return name;
        }
        #endregion
    }

}
