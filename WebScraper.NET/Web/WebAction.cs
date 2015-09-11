using System.Threading;
using System.Windows.Forms;

namespace WebScraper.NET.Web
{
    public interface IWebAction
    {
        bool isWaitForEvent();
        void DoAction(Agent agent);
        bool Validate(Agent agent);
        bool CanDoAction(Agent agent);
        bool ShouldWaitAction(Agent agent);

    }

    public abstract class AbstractWebAction : ExtensionMethods, IWebAction
    {
        public bool IsWaitForEvent { get; set; }
        public IWebValidator Validator { get; set; }
        public IWebValidator CanDoValidator { get; set; }
        public IWebValidator ShouldWaitValidator { get; set; }
        public AbstractWebAction()
        {

        }

        public AbstractWebAction(bool waitForEvent = false, IWebValidator validator = null, IWebValidator canDoValidator = null, IWebValidator shouldWaitValidator = null)
        {
            IsWaitForEvent = waitForEvent;
            Validator = validator;
            CanDoValidator = canDoValidator;
            ShouldWaitValidator = shouldWaitValidator;
        }

        public bool isWaitForEvent()
        {
            return IsWaitForEvent;
        }

        public abstract void DoAction(Agent agent);
        public virtual bool Validate(Agent agent)
        {
            var ret = true;
            if (!IsNull(Validator))
            {
                ret = Validator.Validate(agent);
            }
            return ret;
        }
        public virtual bool CanDoAction(Agent agent)
        {
            var ret = true;
            if (!IsNull(CanDoValidator))
            {
                ret = CanDoValidator.Validate(agent);
            }
            return ret;
        }
        public virtual bool ShouldWaitAction(Agent agent)
        {
            var ret = false;
            if (!IsNull(ShouldWaitValidator))
            {
                ret = !ShouldWaitValidator.Validate(agent);
            }
            return ret;
        }
    }

    public class ExtractWebAction<V> : AbstractWebAction
    {
        public IDataExtractor<HtmlElement, V> Extractor { get; set; }
        public HtmlElementLocator Locator { get; set; }
        public string ContextKey { get; set; }
        public V ExtractedData { get; set; }
        public ExtractWebAction()
        {

        }
        public ExtractWebAction(IDataExtractor<HtmlElement, V> extractor = null, string contextKey = null, HtmlElementLocator locator = null)
        {
            Extractor = extractor;
            ContextKey = contextKey;
            Locator = locator;
        }
        public override void DoAction(Agent agent)
        {
            ExtractedData = default(V);
            HtmlElement element = null;
            if (!IsNull(agent.WebBrowser.Document))
                element = null == Locator ? agent.WebBrowser.Document?.Body : Locator.Locate(agent);
            if (IsNull(element)) return;
            var data = Extractor.Extract(element);
            ExtractedData = data;
            if (!IsNull(ContextKey) && !IsNull(data))
            {
                agent.RequestContext.Add(ContextKey, data);
            }
        }
    }

    public class SimpleWebAction : AbstractWebAction
    {
        public IWebStep Step { get; set; }
        public SimpleWebAction()
        {

        }
        public SimpleWebAction(IWebStep step = null, IWebValidator validator = null, IWebValidator canDoValidator = null, IWebValidator shouldWaitValidator = null, bool waitForEvent = false)
            : base(waitForEvent, validator, canDoValidator, shouldWaitValidator)
        {
            Step = step;
        }
        public override void DoAction(Agent agent)
        {
            Step?.Execute(agent);
        }
    }

    public class TimedWebAction : AbstractWebAction
    {
        public System.Threading.Timer Timer { get; set; }
        public TimedWebAction()
        {

        }
        public TimedWebAction(IWebValidator validator = null, IWebValidator canDoValidator = null, IWebValidator shouldWaitValidator = null, bool waitForEvent = false)
            : base(waitForEvent, validator, canDoValidator, shouldWaitValidator)
        {
        }
        public override void DoAction(Agent agent)
        {
            var ret = Validator.Validate(agent);
            if (ret) return;
            if (!IsNull(Timer)) return;
            TimerCallback callback = TimerCallback;
            Timer = new System.Threading.Timer(callback, agent, 500, 500);
        }
        public void TimerCallback(object argument)
        {
            if (!Validator.Validate((Agent) argument)) return;
            if (IsNull(Timer))
            {
                //NOOP
            }
            else
            {
                Timer.Dispose();
                Timer = null;
                ((Agent)argument).CompletedWaitAction();
            }
        }
    }

}
