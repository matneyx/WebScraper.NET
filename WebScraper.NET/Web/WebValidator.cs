using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace WebScraper.NET.Web
{
    public delegate bool ValueValidateDelegate(string value);

    public interface IWebValidator
    {
        bool Validate(Agent agent);
    }
    public abstract class AbstractWebValidator : ExtensionMethods, IWebValidator
    {
        public bool Validate(Agent agent)
        {
            var ret = false;
            MethodInvoker delegateCall = delegate
            {
                ret = InternalValidate(agent);
            };
            if (agent.WebBrowser.InvokeRequired)
            {
                agent.WebBrowser.Invoke(delegateCall);
            }
            else
            {
                delegateCall();
            }
            return ret;
        }
        public abstract bool InternalValidate(Agent agent);

    }
    public class TitleWebValidator : AbstractWebValidator
    {

        public string Title { get; set; }
        public Regex TitleRegex { get; set; }
        public TitleWebValidator()
        {

        }
        public TitleWebValidator(string title = null, Regex titleRegex = null)
        {
            Title = title;
            TitleRegex = titleRegex;
        }
        public override bool InternalValidate(Agent agent)
        {
            var ret = false;
            if (IsNull(agent.WebBrowser.Document)) return ret;
            var title = agent.WebBrowser.Document.Title;
            if (null != Title)
            {
                ret = title.Equals(Title);
            }
            if (null != TitleRegex)
            {
                ret = TitleRegex.IsMatch(title);
            }
            return ret;
        }

    }
    public class LocatorCheckValidator : AbstractWebValidator
    {
        public HtmlElementLocator Locator { get; set; }
        public LocatorCheckValidator()
        {

        }
        public LocatorCheckValidator(HtmlElementLocator locator = null)
        {
            Locator = locator;
        }
        public override bool InternalValidate(Agent agent)
        {
            var element = Locator.Locate(agent);
            return !IsNull(element);
        }

    }

    public class ValueCheckValidator : AbstractWebValidator
    {
        public HtmlElementLocator Locator { get; set; }
        public string AttributeName { get; set; }
        public string Value { get; set; }
        public Regex ValueRegex { get; set; }
        public ValueCheckValidator(HtmlElementLocator locator = null, string attributeName = null, string value = null, Regex valueRegex = null)
        {
            Locator = locator;
            AttributeName = attributeName;
            Value = value;
            ValueRegex = valueRegex;
        }
        public override bool InternalValidate(Agent agent)
        {
            var ret = false;
            if (IsNull(Locator)) return ret;
            var element = Locator.Locate(agent);
            if (IsNull(element)) return ret;
            if (IsNull(AttributeName))
            {
                ret = ValueRegex?.IsMatch(element.InnerText) ?? element.InnerText.Equals(Value);
            }
            else
            {
                var value = element.GetAttribute(AttributeName);
                ret = ValueRegex?.IsMatch(value) ?? value.Equals(Value);
            }
            return ret;
        }
    }

    public class StyleCheckValidator : AbstractWebValidator
    {

        public HtmlElementLocator Locator { get; set; }
        public string Value { get; set; }
        public Regex ValueRegex { get; set; }
        public ValueValidateDelegate CheckDelegate { get; set; }
        public StyleCheckValidator(HtmlElementLocator locator = null, string value = null, Regex valueRegex = null, ValueValidateDelegate checkDelegate = null)
        {
            Locator = locator;
            Value = value;
            ValueRegex = valueRegex;
            CheckDelegate = checkDelegate;
        }
        public override bool InternalValidate(Agent agent)
        {
            var ret = false;
            if (IsNull(Locator)) return ret;
            var element = Locator.Locate(agent);
            if (IsNull(element)) return ret;
            var value = element.Style;
            if (IsNull(CheckDelegate))
            {
                if (IsNull(value)) return ret;
                ret = ValueRegex?.IsMatch(value) ?? value.Equals(Value);
            }
            else
            {
                ret = CheckDelegate(value);
            }
            return ret;
        }
    }

    public class TimedProxyWebValidator : AbstractWebValidator
    {
        public IWebValidator Validator { get; set; }
        public System.Threading.Timer Timer { get; set; }
        public TimedProxyWebValidator(IWebValidator validator = null)
        {
            Validator = validator;
        }
        public override bool InternalValidate(Agent agent)
        {

            if (Validator.Validate(agent)) return true;
            if (IsNull(Timer)) return false;
            TimerCallback callback = TimerCallback;
            Timer = new System.Threading.Timer(callback, agent, 500, 500);
            return false;
        }
        public void TimerCallback(object argument)
        {
            if (!Validator.Validate((Agent) argument)) return;
            Timer.Dispose();
            Timer = null;
            ((Agent)argument).CompletedWaitAction();
        }
    }


}
