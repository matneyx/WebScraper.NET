using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WebScraper.Data;

namespace WebScraper.Web
{

    public class AccessTiming
    {

        public Uri URI { get; set; }

        [DefaultValue(0)]
        public long TimingInTicks { get; private set; }

        public DateTime StartTime { get; private set; }

        public DateTime CurrentStartTime { get; private set; }

        public AccessTiming(Uri uri, long TimingInTicks = 0)
        {
            this.URI = uri;
            this.TimingInTicks = TimingInTicks;
            this.StartTime = DateTime.UtcNow;
            this.CurrentStartTime = this.StartTime;
        }

        public void MarkTiming()
        {
            DateTime nowTime = DateTime.UtcNow;
            TimingInTicks += nowTime.Ticks - this.CurrentStartTime.Ticks;
        }

        public void StartTiming()
        {
            this.CurrentStartTime = DateTime.UtcNow;
        }

        public void AddTiming(AccessTiming accessTiming)
        {
            this.TimingInTicks += accessTiming.TimingInTicks;
        }

        public double GetTimingInSeconds()
        {
            return new TimeSpan(TimingInTicks).TotalSeconds;
        }


    }

    public abstract class Agent
    {

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public WebBrowser WebBrowser { get; set; }

        public Dictionary<string, object> RequestContext { get; set; }

        public Dictionary<string, object> Outputs { get; set; }

        public bool MonitorTimings { get; set; }

        protected Stack<AccessTiming> AccessTimes { get; set; }

        protected WebAction ActiveAction;

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
            this.WebBrowser = browser;
        }


        public virtual void Init()
        {
            RequestContext = new Dictionary<string, object>();
            Outputs = new Dictionary<string, object>();
            Trigger = new AutoResetEvent(false);
            WaitHandles = new WaitHandle[] { Trigger };
            if (MonitorTimings)
            {
                CompletedEventHandlerForTiming = new WebBrowserDocumentCompletedEventHandler(this.PageLoadedForMonitoring);
                WebBrowser.DocumentCompleted += CompletedEventHandlerForTiming;
                AccessTimes = new Stack<AccessTiming>();
            }
        }

        public virtual void DoActions(List<WebAction> actions)
        {
            CompletedEventHandler = new WebBrowserDocumentCompletedEventHandler(this.PageLoaded);
            WebBrowser.DocumentCompleted += CompletedEventHandler;
            var activeActions = new Queue<WebAction>(actions);
            while (0 < activeActions.Count)
            {
                ActiveAction = activeActions.Dequeue();
                if (ActiveAction.canDoAction(this))
                {
                    if (ActiveAction.shouldWaitAction(this))
                    {
                        Trigger.Reset();
                        WaitHandle.WaitAny(WaitHandles);
                    }
                    ActiveAction.doAction(this);
                    if (ActiveAction.isWaitForEvent())
                    {
                        Trigger.Reset();
                        WaitHandle.WaitAny(WaitHandles);
                    }
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
            if (null != CompletedEventHandlerForTiming)
            {
                UpdateAccessTimings(WebBrowser.Url, true);
                WebBrowser.DocumentCompleted -= CompletedEventHandlerForTiming;
            }
        }

        public virtual void CompletedWaitAction()
        {
            Trigger.Set();
        }

        public virtual bool ValidateActiveAction()
        {
            var ret = false;
            if (null != ActiveAction && ActiveAction.isWaitForEvent() && ActiveAction.validate(this))
            {
                ret = true;
                Trigger.Set();
            }
            return ret;
        }

        public virtual void PageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ValidateActiveAction();
        }

        public virtual void PageLoadedForMonitoring(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (MonitorTimings)
            {
                UpdateAccessTimings(e.Url);
                LastedUpdated = DateTime.Now;
            }
        }

        protected void UpdateAccessTimings(Uri url, bool updateOldOnly = false)
        {
            if (null != AccessTimes)
            {
                AccessTiming lastEntry = 0 == AccessTimes.Count ? null : AccessTimes.Peek();
                if (null != lastEntry)
                {
                    lastEntry.MarkTiming();
                }
                if (!updateOldOnly)
                {
                    AccessTimes.Push(new AccessTiming(url));
                }
            }
        }

        public List<AccessTiming> getDomainAccessTimings()
        {
            List<AccessTiming> ret = new List<AccessTiming>();
            Dictionary<String, AccessTiming> timingMap = new Dictionary<String, AccessTiming>();
            if (null != AccessTimes)
            {
                foreach (AccessTiming accessTime in AccessTimes)
                {
                    AccessTiming timing = timingMap.ContainsKey(accessTime.URI.Host) ? timingMap[accessTime.URI.Host] : null;
                    if (null == timing)
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
            }
            return ret;
        }
    }

    public class SimpleAgent : Agent
    {
        public List<WebAction> WebActions { get; set; }

        public SimpleAgent()
            : base()
        {

        }
        public SimpleAgent(WebBrowser browser = null, List<WebAction> actions = null)
            : base(browser: browser)
        {
            this.WebActions = actions;
        }

        public virtual void startAgent()
        {
            DoActions(WebActions);
        }

    }

    public class PageDumpAgent : SimpleAgent
    {
    }

}
