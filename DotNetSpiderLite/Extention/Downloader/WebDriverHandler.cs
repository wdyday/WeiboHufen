﻿using DotNetSpiderLite.Core.Infrastructure;
using NLog;
using OpenQA.Selenium.Remote;
using System;
using System.Threading;

namespace DotNetSpiderLite.Extension.Downloader
{
	public class Scroll : IWebDriverHandler
	{
		protected static readonly ILogger Logger = LogCenter.GetSpiderLogger();

		public int ScrollTimes { get; set; } = 1;

		public bool Handle(RemoteWebDriver webDriver)
		{
			try
			{
				webDriver.Manage().Window.Maximize();
				for (int i = 0; i <= ScrollTimes; i++)
				{
					webDriver.ExecuteScript("window.scrollBy(0, 500)");
					Thread.Sleep(1000);
				}
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}
	}
}