using DotNetSpiderLite.Core;
using System;
using DotNetSpiderLite.Core.Selector;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Threading;
using DotNetSpiderLite.Extension.Model;
using DotNetSpiderLite.Core.Infrastructure;
using NLog;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;

namespace DotNetSpiderLite.Extension.Downloader
{
    /// <summary>
    /// [WDY] 表单填充+提交
    /// </summary>
    public abstract class FormActionHandler : Named, IWebDriverHandler
    {
        protected static readonly ILogger Logger = LogCenter.GetSpiderLogger();

        public abstract bool Handle(RemoteWebDriver driver);
    }

    /// <summary>
    /// [WDY] 表单填充+提交
    /// </summary>
    public class FormSubmitHandler : FormActionHandler
    {
        /// <summary>
        /// 页面元素选择器+元素值
        /// </summary>
        public List<KeyValuePair<Selector, string>> Selectors { get; set; }

        /// <summary>
        /// 操作按钮
        /// </summary>
        public Selector SubmitSelector { get; set; }

        /// <summary>
        /// 等待时间: 操作按钮 点击后, 等待n秒后进行下一步操作
        /// </summary>
        private int _waitSeconds { get; set; }
        public int WaitSeconds
        {
            get { return _waitSeconds > 0 ? _waitSeconds : 1; }
            set { this._waitSeconds = value; }
        }

        /// <summary>
        /// 页面子操作
        /// 如点击查询后, 显示内容中再次点击操作
        /// </summary>
        public List<ChildFormAction> ChildFormActions { get; set; }

        public override bool Handle(RemoteWebDriver webDriver)
        {
            try
            {
                foreach (var selector in Selectors)
                {
                    var element = Utils.FindElement(webDriver, selector.Key);
                    element.SendKeys(selector.Value);
                    Thread.Sleep(100);
                }

                var submit = Utils.FindElement(webDriver, SubmitSelector);
                submit.Click();
                Thread.Sleep(WaitSeconds * 1000);
                
                // 页面子操作
                DoChildFormActions(webDriver);

                return true;
            }
            catch (Exception ex)
            {
                Logger.AllLog($"FormSubmitHandler failed: {ex}.", NLog.LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// 页面子操作
        /// 如点击查询后, 显示内容中再次点击操作
        /// </summary>
        private void DoChildFormActions(RemoteWebDriver webDriver)
        {
            if (ChildFormActions == null)
            {
                return;
            }

            foreach (var child in ChildFormActions)
            {
                if (child.SubmitSelector != null)
                {
                    // 找action元素, 如果不存在, 休眠3s, 3s后仍不存在则退出
                    var eleSubmit = Utils.FindElement(webDriver, child.SubmitSelector);
                    if (eleSubmit == null)
                    {
                        Thread.Sleep(3000);

                        eleSubmit = Utils.FindElement(webDriver, child.SubmitSelector);
                        if (eleSubmit == null)
                        {
                            return;
                        }
                    }

                    // 填充表单
                    if (child.Selectors != null)
                    {
                        foreach (var selector in child.Selectors)
                        {
                            var element = Utils.FindElement(webDriver, selector.Key);
                            element.SendKeys(selector.Value);
                            Thread.Sleep(100);
                        }
                    }
                    // 提交
                    var submit = Utils.FindElement(webDriver, child.SubmitSelector);
                    submit.Click();
                    Thread.Sleep(child.WaitSeconds * 1000);
                }
                else if (!string.IsNullOrEmpty(child.NavigateUrl))
                {
                    webDriver.Navigate().GoToUrl(child.NavigateUrl);

                    Thread.Sleep(child.WaitSeconds * 1000);
                }
            }
        }
    }

    /// <summary>
    /// 页面子操作
    /// 如点击查询后, 显示内容中再次点击操作
    /// </summary>
    public class ChildFormAction
    {
        /// <summary>
        /// 页面元素选择器+元素值
        /// </summary>
        public List<KeyValuePair<Selector, string>> Selectors { get; set; }

        /// <summary>
        /// 页面操作, 优先级高于 url 重定向
        /// </summary>
        public Selector SubmitSelector { get; set; }

        /// <summary>
        /// url 重定向, 优先级低于页面操作
        /// </summary>
        public string NavigateUrl { get; set; }

        /// <summary>
        /// 等待时间: 操作按钮 点击后, 等待n秒后进行下一步操作
        /// </summary>
        private int _waitSeconds { get; set; }
        public int WaitSeconds
        {
            get { return _waitSeconds > 0 ? _waitSeconds : 1; }
            set { this._waitSeconds = value; }
        }
    }
}
