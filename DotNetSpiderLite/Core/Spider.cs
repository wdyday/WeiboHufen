using DotNetSpiderLite.Core.Downloader;
using DotNetSpiderLite.Core.Infrastructure;
using DotNetSpiderLite.Core.Redial;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetSpiderLite.Core
{
    public class Spider : AppBase, ISpider
    {
        //protected static readonly ILogger Logger = LogCenter.GetLogger();

        private readonly Site _site;
        private string _identity;
        private IDownloader _downloader = new HttpClientDownloader();
        private Status _realStat = Status.Init;

        /// <summary>
        /// start time of spider.
        /// </summary>
        protected DateTime StartTime { get; private set; }

        /// <summary>
        /// end time of spider.
        /// </summary>
        protected DateTime EndTime { get; private set; } = DateTime.MinValue;

        protected Request Request { get; set; }

        /// <summary>
        /// Identity of spider.
        /// </summary>
        public override string Identity
        {
            get => _identity;
            set
            {
                CheckIfRunning();

                if (string.IsNullOrEmpty(value) || value.Length > Env.IdentityMaxLength)
                {
                    throw new ArgumentException($"Length of Identity should less than {Env.IdentityMaxLength}.");
                }

                _identity = value;
            }
        }

        /// <summary>
        /// Site of spider.
        /// </summary>
        public Site Site => _site;

        /// <summary>
        /// Status of spider.
        /// </summary>
        public Status Stat { get; private set; } = Status.Init;

        /// <summary>
        /// Event of crawler a request success.
        /// </summary>
        public event Action<Request> OnSuccess;

        public IDownloader Downloader
        {
            get => _downloader;
            set
            {
                CheckIfRunning();
                _downloader = value;
            }
        }

        /// <summary>
        /// Scheduler of spider.
        /// </summary>
        //public IScheduler Scheduler
        //{
        //    get => _scheduler;
        //    set
        //    {
        //        CheckIfRunning();
        //        _scheduler = value;
        //    }
        //}

        /// <summary>
        /// Start url builders of spider.
        /// </summary>
        public readonly List<IStartUrlBuilder> StartUrlBuilders = new List<IStartUrlBuilder>();

        #region init
        /// <summary>
        /// Init component of spider.
        /// </summary>
        /// <param name="arguments"></param>
        protected virtual void InitComponent(params string[] arguments)
        {
            Logger.AllLog(Identity, "Build internal component...", LogLevel.Info);

#if !NET_CORE // 开启多线程支持
            ServicePointManager.DefaultConnectionLimit = 1000;
#endif

            InitSite();

            InitDownloader();

            //InitScheduler(arguments);

            //InitPageProcessor();

            //InitPipelines(arguments);

            //InitFileCloseSignals();

            //InitMonitor();

            //InjectCookie();

            Console.CancelKeyPress += ConsoleCancelKeyPress;

            //_waitCountLimit = EmptySleepTime / WaitInterval;

            //PrepareErrorRequestsLogFile();

            //PipelineRetryTimes = PipelineRetryTimes <= 0 ? 1 : PipelineRetryTimes;

            //_pipelineRetryPolicy = Policy.Handle<Exception>().Retry(PipelineRetryTimes, (ex, count) =>
            //{
            //    Logger.Error($"Execute pipeline failed [{count}]: {ex}");
            //});

            //ExecuteStartUrlBuilders(arguments);

            //PushStartRequestToScheduler();

            //_init = true;
        }

        #region private mothods

        private void InitSite()
        {
            Site.Accept = Site.Accept ?? "application/json, text/javascript, */*; q=0.01";
            Site.UserAgent = Site.UserAgent ??
                             "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";
            if (!Site.Headers.ContainsKey("Accept-Language"))
            {
                Site.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            }
        }
        private void InitDownloader()
        {
            Downloader = Downloader ?? new HttpClientDownloader();
        }

        private void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Exit();
            //while (!_scheduler.IsExited)
            //{
            //    Thread.Sleep(100);
            //}
        }

        //private void ExecuteStartUrlBuilders(params string[] arguments)
        //{
        //    if (StartUrlBuilders != null && StartUrlBuilders.Count > 0)
        //    {
        //        try
        //        {
        //            for (int i = 0; i < StartUrlBuilders.Count; ++i)
        //            {
        //                var builder = StartUrlBuilders[i];
        //                Logger.AllLog(Identity, $"Add start urls to scheduler via builder[{i + 1}].", LogLevel.Info);
        //                builder.Build(Site);
        //            }
        //        }
        //        finally
        //        {
        //            //InitStartRequestsFinished();
        //        }
        //    }
        //}
        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            NetworkCenter.Current.Executor?.Dispose();
            Exit();
            //while (!_scheduler.IsExited)
            //{
            //    Thread.Sleep(100);
            //}
        }

        private void SafeDestroy(object obj)
        {
            if (obj is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception e)
                {
                    Logger.AllLog(Identity, e.ToString(), LogLevel.Error);
                }
            }
        }

        #endregion

        #endregion

        public static Spider Create(Site site, IDownloader downloader = null)
        {
            return new Spider(site, Guid.NewGuid().ToString("N"), downloader);
        }
        public static Spider Create(Site site, string identify, IDownloader downloader = null)
        {
            return new Spider(site, identify, downloader);
        }

        /// <summary>
        /// Run spider.
        /// </summary>
        /// <param name="arguments"></param>
        public Page Run(params string[] arguments)
        {
            Page page = null;

            CheckIfSettingsCorrect();

            InitComponent(arguments);


            if (StartTime == DateTime.MinValue)
            {
                StartTime = DateTime.Now;
            }

            Stat = Status.Running;
            _realStat = Status.Running;

            //Request request = Scheduler.Poll();
            Request request = Site.StartRequest;
            try
            {
                Stopwatch sw = new Stopwatch();
                page = HandleRequest(sw, request, Downloader);

                Stat = Status.Finished;
            }
            catch (Exception e)
            {
                //OnError(request);
                Logger.AllLog(Identity, $"Crawler {request.Url} failed: {e}.", LogLevel.Error, e);
            }
            finally
            {
                //if (request.Proxy != null)
                //{
                //    var statusCode = request.StatusCode;
                //    Site.ReturnHttpProxy(request.Proxy, statusCode ?? HttpStatusCode.Found);
                //}
            }

            EndTime = DateTime.Now;
            _realStat = Status.Exited;

            //ReportStatus();

            OnClose();

            //OnClosing?.Invoke(this);

            var msg = Stat == Status.Finished ? "Crawl complete" : "Crawl terminated";
            Logger.AllLog(Identity, $"{msg}, cost: {(EndTime - StartTime).TotalSeconds} seconds.", LogLevel.Info);

            //OnClosed?.Invoke(this);

            return page;
        }

        public static Page AddToCycleRetry(Request request, Site site)
        {
            Page page = new Page(request, null)
            {
                ContentType = site.ContentType
            };

            request.CycleTriedTimes++;

            if (request.CycleTriedTimes <= site.CycleRetryTimes)
            {
                request.Priority = 0;
                page.AddTargetRequest(request, false);
                page.Retry = true;
                return page;
            }
            else
            {
                return null;
            }
        }

        protected Spider()
        {
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#else
            ThreadPool.SetMinThreads(200, 200);
#endif
            var type = GetType();
            var spiderNameAttribute = type.GetCustomAttribute<TaskName>();
            if (spiderNameAttribute != null)
            {
                Name = spiderNameAttribute.Name;
            }
            else
            {
                Name = type.Name;
            }

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        /// <summary>
        /// Create a spider with site, identity, scheduler and pageProcessors.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="identity"></param>
        /// <param name="pageProcessors"></param>
        /// <param name="scheduler"></param>
        protected Spider(Site site, string identity, IDownloader downloader) : this()
        {
            Identity = identity;
            Downloader = downloader;

            _site = site;

            //Scheduler = new QueueDuplicateRemovedScheduler();

            //if (_site == null)
            //{
            //    _site = new Site();
            //}

            CheckIfSettingsCorrectImmediately();
        }

        /// <summary>
        /// Check if all settings of spider are correct.
        /// </summary>
        protected void CheckIfSettingsCorrectImmediately()
        {
            Identity = (string.IsNullOrWhiteSpace(Identity) || string.IsNullOrEmpty(Identity))
                ? Encrypt.Md5Encrypt(Guid.NewGuid().ToString())
                : Identity;

            if (Identity.Length > 100)
            {
                throw new SpiderException("Length of Identity should less than 100.");
            }

            //if (PageProcessors == null || PageProcessors.Count == 0)
            //{
            //    throw new SpiderException("Count of PageProcessor is zero.");
            //}

            Site.Accept = Site.Accept ?? "application/json, text/javascript, */*; q=0.01";
            Site.UserAgent = Site.UserAgent ??
                             "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";
            if (!Site.Headers.ContainsKey("Accept-Language"))
            {
                Site.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            }

            //foreach (var processor in PageProcessors)
            //{
            //    processor.Site = Site;
            //}

            //Scheduler = Scheduler ?? new QueueDuplicateRemovedScheduler();
            Downloader = Downloader ?? new HttpClientDownloader();
        }

        /// <summary>
		/// Check whether spider is running.
		/// </summary>
		protected void CheckIfRunning()
        {
            if (Stat == Status.Running)
            {
                throw new SpiderException("Spider is running.");
            }
        }

        /// <summary>
        /// Check if all settings of spider are correct.
        /// </summary>
        protected void CheckIfSettingsCorrect()
        {
            Identity = (string.IsNullOrWhiteSpace(Identity) || string.IsNullOrEmpty(Identity))
                ? Encrypt.Md5Encrypt(Guid.NewGuid().ToString())
                : Identity;

            if (Identity.Length > 100)
            {
                throw new SpiderException("Length of Identity should less than 100.");
            }

            Site.Accept = Site.Accept ?? "application/json, text/javascript, */*; q=0.01";
            Site.UserAgent = Site.UserAgent ??
                             "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";
            if (!Site.Headers.ContainsKey("Accept-Language"))
            {
                Site.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            }

            Downloader = Downloader ?? new HttpClientDownloader();
        }

        protected void _OnSuccess(Request request)
        {
            //Scheduler.IncreaseSuccessCount();
            OnSuccess?.Invoke(request);
        }


        /// <summary>
        /// Single/atom logical to handle a request by downloader, processors and pipelines.
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="request"></param>
        /// <param name="downloader"></param>
        protected Page HandleRequest(Stopwatch sw, Request request, IDownloader downloader)
        {
            Page page = null;

            try
            {
                sw.Reset();
                sw.Start();

                page = downloader.Download(request, this);

                sw.Stop();
                //CalculateDownloadSpeed(sw.ElapsedMilliseconds);

                if (page == null || page.Skip)
                {
                    return null;
                }

                if (page.Exception == null)
                {
                    sw.Reset();
                    sw.Start();

                    sw.Stop();
                    //CalculateProcessorSpeed(sw.ElapsedMilliseconds);
                }
                //else
                //{
                //    OnError(page.Request);
                //}
            }
            catch (DownloadException de)
            {
                //if (page != null) OnError(page.Request);
                Logger.AllLog(Identity, $"Should not catch download exception: {request.Url}.", LogLevel.Error, de);
            }
            catch (Exception e)
            {
                //if (page != null) OnError(page.Request);
                Logger.AllLog(Identity, $"Extract {request.Url} failed, please check your pipeline: {e}.", LogLevel.Error, e);
            }

            if (page == null)
            {
                return null;
            }
            if (page.Exception == null)
            {
                sw.Reset();
                sw.Start();

                _OnSuccess(request);

                sw.Stop();
            }

            return page;
        }


        /// <summary>
        /// Event when spider on close.
        /// </summary>
        protected void OnClose()
        {
            //var containsData = _cached != null && _cached.Count > 0;

            //foreach (IPipeline pipeline in Pipelines)
            //{
            //    if (containsData)
            //    {
            //        pipeline.Process(_cached.ToArray());
            //    }
            //    SafeDestroy(pipeline);
            //}

            //SafeDestroy(Scheduler);
            //SafeDestroy(PageProcessors);

            SafeDestroy(Downloader);

            //SafeDestroy(Site.HttpProxyPool);
            //SafeDestroy(_errorRequestStreamWriter);
        }
        /// <summary>
        /// Pause spider.
        /// </summary>
        /// <param name="action"></param>
        public void Pause(Action action = null)
        {
            if (Stat != Status.Running)
            {
                Logger.AllLog(Identity, "Crawler is not running.", LogLevel.Warn);
                return;
            }
            Stat = Status.Stopped;
            Logger.AllLog(Identity, "Stop running...", LogLevel.Warn);
            if (action != null)
            {
                Task.Factory.StartNew(() =>
                {
                    while (_realStat != Status.Stopped)
                    {
                        Thread.Sleep(100);
                    }
                    action();
                });
            }
        }

        /// <summary>
        /// Contiune spider if spider is paused.
        /// </summary>
        public void Contiune()
        {
            if (_realStat == Status.Stopped)
            {
                Stat = Status.Running;
                _realStat = Status.Running;
                Logger.AllLog(Identity, "Continue...", LogLevel.Warn);
            }
            else
            {
                Logger.AllLog(Identity, "Crawler was not paused, can not continue...", LogLevel.Warn);
            }
        }
        /// <summary>
        /// Exit spider.
        /// </summary>
        /// <param name="action"></param>
        public void Exit(Action action = null)
        {
            if (Stat == Status.Running || Stat == Status.Stopped)
            {
                Stat = Status.Exited;
                Logger.AllLog(Identity, "Exit...", LogLevel.Warn);
                return;
            }
            Logger.AllLog(Identity, "Crawler is not running.", LogLevel.Warn);
            if (action != null)
            {
                Task.Factory.StartNew(() =>
                {
                    while (_realStat != Status.Exited)
                    {
                        Thread.Sleep(100);
                    }
                    action();
                });
            }
        }

        /// <summary>
        /// Dispose spider.
        /// </summary>
        public void Dispose()
        {
            CheckIfRunning();

            //int i = 0;
            //while (!_scheduler.IsExited)
            //{
            //    ++i;
            //    Thread.Sleep(500);
            //    if (i > 10)
            //    {
            //        break;
            //    }
            //}

            OnClose();
        }
    }
}
