using DotNetSpiderLite.Core;
using DotNetSpiderLite.Core.Infrastructure;
using DotNetSpiderLite.Core.Selector;
using DotNetSpiderLite.Extension.Downloader;
using DotNetSpiderLite.Extension.Infrastructure;
using DotNetSpiderLite.Extension.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WeiboHufen.Job
{
    class Program
    {
        protected static ILogger Logger = LogCenter.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static void Main(string[] args)
        {
            // set the current directory to the same directory as your windows service
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            // TEST
            //Run();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new HufenService()
            };
            ServiceBase.Run(ServicesToRun);
        }

        #region test

        private static void Run()
        {
            //// http://hufen123.vipsinaapp.com/?c=ex&a=index
            var url = "https://api.weibo.com/oauth2/authorize?client_id=76999512&redirect_uri=http%3A%2F%2Fapps.weibo.com%2Fhufenjingling&response_type=code";
            //var page = Crawl(url);

            new SignInJob().SignIn(url);
        }

        private static Page Crawl(string url)
        {
            try
            {
                // Config encoding, header, cookie, proxy etc... 定义采集的 Site 对象, 设置 Header、Cookie、代理等
                var headers = new Dictionary<string, string>();
                headers.Add("Accept", "application/xml, text/xml, */*; q=0.01");
                headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                var site = new Site(url) { EncodingName = "UTF-8", Headers = headers, RemoveOutboundLinks = true, DownloadFiles = false };

                var option = new Option(LanguageEnum.English);
                var webDownloader = new WebDriverDownloader(Browser.Chrome, 200, null, option);
                webDownloader.FormSubmit = new FormSubmitHandler()
                {
                    Selectors = new List<KeyValuePair<Selector, string>>
                    {
                        new KeyValuePair<Selector, string>(new Selector(SelectorType.Id, "userId"), "wdyday@163.com"),
                        new KeyValuePair<Selector, string>(new Selector(SelectorType.Id, "passwd"), "Worm0429@")
                    },
                    SubmitSelector = new Selector(SelectorType.XPath, "//p[@class='oauth_formbtn']/a[1]"),
                    ChildFormActions = GetChildFormActions()
                };

                Spider spider = Spider.Create(site, webDownloader);
                Page page = spider.Run();

                return page;
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
                return null;
            }
        }

        private static List<ChildFormAction> GetChildFormActions()
        {
            return new List<ChildFormAction>
            {
                new ChildFormAction
                {
                    NavigateUrl = "http://hufen123.vipsinaapp.com/?c=ex&a=index"
                },
                new ChildFormAction
                {
                    SubmitSelector = new Selector(SelectorType.XPath, "//*[@id='main-box']/div/div[1]/div[2]/p[6]/button"),
                }
            };
        }

        #endregion
    }
}
