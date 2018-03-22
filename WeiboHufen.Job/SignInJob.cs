using DotNetSpiderLite.Core;
using DotNetSpiderLite.Core.Infrastructure;
using DotNetSpiderLite.Core.Selector;
using DotNetSpiderLite.Extension.Downloader;
using DotNetSpiderLite.Extension.Infrastructure;
using DotNetSpiderLite.Extension.Model;
using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WeiboHufen.Job.Models;
using WeiboHufen.Job.Utils;

namespace WeiboHufen.Job
{
    /// <summary>
    /// 互粉大厅 每日签到
    /// </summary>
    public class SignInJob : IJob
    {
        protected ILogger Logger = LogCenter.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private string _signInUrl = ConfigurationManager.AppSettings["SignInUrl"];

        public Task Execute(IJobExecutionContext context)
        {
            return Run();
        }

        private async Task Run()
        {
            Logger.Info("[SignInJob] Start!");

            try
            {
                await Task.Factory.StartNew(() =>
                {
                    SignIn(_signInUrl);
                });
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }

            Logger.Info("[SignInJob] End!");
        }

        #region 签到

        public void SignIn(string url)
        {
            var configPath = Environment.CurrentDirectory + @"\data.json";
            var users = FileUtil.GetJson<List<User>>(configPath);

            foreach (var user in users)
            {
                if (!IsSignedToday(user))
                {
                    var succeeded = SignIn(url, user.UserName, user.Password);
                    user.SignInSucceeded = succeeded;
                    user.SignInTime = DateTime.Now;
                }
            }
            
            FileUtil.Save(configPath, users);
        }

        private bool SignIn(string url, string userName, string password)
        {
            var succeeded = true;

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
                        new KeyValuePair<Selector, string>(new Selector(SelectorType.Id, "userId"), userName),
                        new KeyValuePair<Selector, string>(new Selector(SelectorType.Id, "passwd"), password)
                    },
                    SubmitSelector = new Selector(SelectorType.XPath, "//p[@class='oauth_formbtn']/a[1]"),
                    WaitSeconds = 5,   // 等待5秒后进行下一步操作
                    ChildFormActions = GetChildFormActions()
                };

                Spider spider = Spider.Create(site, webDownloader);
                Page page = spider.Run();

                if (!page.TargetUrl.ToLower().Contains("hufen123.vipsinaapp.com"))
                {
                    // 签到失败
                    succeeded = false;
                    Logger.Info($"签到失败 : {userName}");
                }
                else
                {
                    Logger.Info($"签到成功 : {userName}");
                }
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
                Logger.Info($"签到失败 : {userName}");
                // 签到失败
                succeeded = false;
            }

            return succeeded;
        }

        /// <summary>
        /// 今日是否已签到
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool IsSignedToday(User user)
        {
            return user.SignInTime.HasValue && user.SignInTime.Value.Date == DateTime.Now.Date && user.SignInSucceeded;
        }

        private List<ChildFormAction> GetChildFormActions()
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
