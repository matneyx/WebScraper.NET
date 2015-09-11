using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace WebScraper.NET.Web
{

    public class AccessTiming
    {

        public Uri URI { get; set; }

        [DefaultValue(0)]
        public long TimingInTicks { get; private set; }

        public DateTime StartTime { get; }

        public DateTime CurrentStartTime { get; private set; }

        public AccessTiming(Uri uri, long timingInTicks = 0)
        {
            URI = uri;
            TimingInTicks = timingInTicks;
            StartTime = DateTime.UtcNow;
            CurrentStartTime = StartTime;
        }

        public void MarkTiming()
        {
            var nowTime = DateTime.UtcNow;
            TimingInTicks += nowTime.Ticks - CurrentStartTime.Ticks;
        }

        public void StartTiming()
        {
            CurrentStartTime = DateTime.UtcNow;
        }

        public void AddTiming(AccessTiming accessTiming)
        {
            TimingInTicks += accessTiming.TimingInTicks;
        }

        public double GetTimingInSeconds()
        {
            return new TimeSpan(TimingInTicks).TotalSeconds;
        }


    }

    public abstract class Agent : ExtensionMethods
    {

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public WebBrowser WebBrowser { get; set; }

        public Dictionary<string, object> RequestContext { get; set; }

        public Dictionary<string, object> Outputs { get; set; }

        public bool MonitorTimings { get; set; }

        protected Stack<AccessTiming> AccessTimes { get; set; }

        protected IWebAction ActiveAction;

        protected WebBrowserDocumentCompletedEventHandler CompletedEventHandler;

        protected WebBrowserDocumentCompletedEventHandler CompletedEventHandlerForTiming;

        protected AutoResetEvent Trigger;

        protected WaitHandle[] WaitHandles;

        public DateTime LastedUpdated { get; private set; }

        public Agent()
        {
        }

        public Agent(WebBrowser browser = null)
        {
            WebBrowser = browser;
        }


        public virtual void Init()
        {
            RequestContext = new Dictionary<string, object>();
            Outputs = new Dictionary<string, object>();
            Trigger = new AutoResetEvent(false);
            WaitHandles = new WaitHandle[] { Trigger };
            if (!MonitorTimings) return;
            CompletedEventHandlerForTiming = PageLoadedForMonitoring;
            WebBrowser.DocumentCompleted += CompletedEventHandlerForTiming;
            AccessTimes = new Stack<AccessTiming>();
        }

        public virtual void DoActions(List<IWebAction> actions)
        {
            CompletedEventHandler = PageLoaded;
            WebBrowser.DocumentCompleted += CompletedEventHandler;
            var activeActions = new Queue<IWebAction>(actions);
            while (0 < activeActions.Count)
            {
                ActiveAction = activeActions.Dequeue();
                if (!ActiveAction.CanDoAction(this)) continue;
                if (ActiveAction.ShouldWaitAction(this))
                {
                    Trigger.Reset();
                    WaitHandle.WaitAny(WaitHandles);
                }
                ActiveAction.DoAction(this);
                if (ActiveAction.isWaitForEvent())
                {
                    Trigger.Reset();
                    WaitHandle.WaitAny(WaitHandles);
                }
            }
            CompletedActions();
        }

        public virtual void CompletedActions()
        {
            WebBrowser.DocumentCompleted -= CompletedEventHandler;
        }

        public virtual void Cleanup()
        {
            if (IsNull(CompletedEventHandlerForTiming)) return;
            UpdateAccessTimings(WebBrowser.Url, true);
            WebBrowser.DocumentCompleted -= CompletedEventHandlerForTiming;
        }

        public virtual void CompletedWaitAction()
        {
            Trigger.Set();
        }

        public virtual bool ValidateActiveAction()
        {
            if (!IsNull(ActiveAction) && ActiveAction.isWaitForEvent() && ActiveAction.Validate(this))
            {

                Trigger.Set();
                return true;
            }
            return false;
        }

        public virtual void PageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ValidateActiveAction();
        }

        public virtual void PageLoadedForMonitoring(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (!MonitorTimings) return;
            UpdateAccessTimings(e.Url);
            LastedUpdated = DateTime.Now;
        }

        protected void UpdateAccessTimings(Uri url, bool updateOldOnly = false)
        {
            if (IsNull(AccessTimes)) return;
            var lastEntry = 0 == AccessTimes.Count ? null : AccessTimes.Peek();
            lastEntry?.MarkTiming();
            if (!updateOldOnly)
            {
                AccessTimes.Push(new AccessTiming(url));
            }
        }

        public List<AccessTiming> GetDomainAccessTimings()
        {
            var ret = new List<AccessTiming>();
            var timingMap = new Dictionary<string, AccessTiming>();
            if (IsNull(AccessTimes)) return ret;
            foreach (var accessTime in AccessTimes)
            {
                var timing = timingMap.ContainsKey(accessTime.URI.Host) ? timingMap[accessTime.URI.Host] : null;
                if (IsNull(timing))
                {
                    timing = new AccessTiming(accessTime.URI, accessTime.TimingInTicks);
                    timingMap[accessTime.URI.Host] = timing;
                    ret.Add(timing);
                }
                else
                {
                    timing.AddTiming(accessTime);
                }
            }
            ret.Reverse();
            return ret;
        }
    }

    public class SimpleAgent : Agent
    {
        public List<IWebAction> WebActions { get; set; }

        public SimpleAgent()
        {

        }
        public SimpleAgent(WebBrowser browser = null, List<IWebAction> actions = null)
            : base(browser)
        {
            WebActions = actions;
        }

        public virtual void StartAgent()
        {
            DoActions(WebActions);
        }

    }

    public class PageDumpAgent : SimpleAgent
    {
    }

}
