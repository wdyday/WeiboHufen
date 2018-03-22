using DotNetSpiderLite.Core.Infrastructure;
using NLog;
using Quartz;
using Quartz.Impl;
using Quartz.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WeiboHufen.Job
{
    /// <summary>
    /// Hufen Service
    /// 创建服务:
    ///     sc create WeiboHufenService binPath= "F:\Project\WeiboHufen\WeiboHufen.Job\bin\Release\WeiboHufen.Job.exe" 
    /// 配置服务:
    ///     sc config WeiboHufenService start = AUTO    (自动) 
    ///     sc config WeiboHufenService start = DEMAND(手动)
    ///     sc config WeiboHufenService start = DISABLED(禁用)
    /// 启动服务:
    ///     net start WeiboHufenService
    /// 关闭服务:
    ///     net stop WeiboHufenService
    /// 删除服务:
    ///     sc delete WeiboHufenService
    /// </summary>
    partial class HufenService : ServiceBase
    {
        protected ILogger Logger = LogCenter.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public HufenService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("service OnStart");
            try
            {
                Task.Run(async () =>
                {
                    await InitScheduler();
                });
            }
            catch (Exception ex)
            {
                Logger.Info("service OnStart Error");
                Logger.Info(ex);

                //Console.WriteLine(ex.Message);
                //Console.Read();
            }
        }

        protected override void OnStop()
        {
            Logger.Info("service OnStop");
        }

        protected override void OnPause()
        {
            base.OnPause();
            Logger.Info("service OnPause");
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            Logger.Info("service OnShutdown");
        }


        private async Task InitScheduler()
        {
            try
            {
                // First we must get a reference to a scheduler
                NameValueCollection properties = new NameValueCollection();
                properties["quartz.scheduler.instanceName"] = "XmlConfiguredInstance";

                //// set thread pool info
                //properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
                //properties["quartz.threadPool.threadCount"] = "10";
                //properties["quartz.threadPool.threadPriority"] = "Normal";

                properties["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz";
                //properties["quartz.serializer.type"] = "binary";

                // job initialization plugin handles our xml reading, without it defaults are used
                properties["quartz.plugin.xml.type"] = "Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin, Quartz.Plugins";
                properties["quartz.plugin.xml.fileNames"] = "~/quartz_jobs.xml";

                ISchedulerFactory sf = new StdSchedulerFactory(properties);
                IScheduler sched = await sf.GetScheduler();

                // start the schedule 
                await sched.Start();

                //Logger.Info($"Scheduler is started : {sched.IsStarted}");
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
        }
    }
}
