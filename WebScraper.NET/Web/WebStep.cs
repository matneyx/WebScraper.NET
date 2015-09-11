using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Spring.Expressions;

namespace WebScraper.NET.Web
{
    public interface IWebStep
    {
        string GetName();
        void Execute(Agent agent);
        bool Validate(Agent agent);
    }

    public abstract class AbstractWebStep : ExtensionMethods, IWebStep
    {
        public string Name { get; set; }
        public AbstractWebStep()
        {

        }
        public AbstractWebStep(string name = null)
        {
            Name = name;
        }

        public string GetName()
        {
            return Name;
        }

        public void Execute(Agent agent)
        {
            MethodInvoker delegateCall = delegate
            {
                InternalExecute(agent);
            };
            if (agent.WebBrowser.InvokeRequired)
            {
                agent.WebBrowser.Invoke(delegateCall);
            }
            else
            {
                delegateCall();
            }
        }
        public abstract void InternalExecute(Agent agent);
        public abstract bool Validate(Agent agent);

    }

    public class UrlWebStep : AbstractWebStep
    {
        public string Url { get; set; }
        public IWebValidator Validator { get; set; }

        public UrlWebStep()
        {

        }
        public UrlWebStep(string name = null, string url = null, IWebValidator validator = null)
            : base(name)
        {
            Url = url;
            Validator = validator;
        }
        public override void InternalExecute(Agent agent)
        {
            agent.WebBrowser.Navigate(Url);
        }
        public override bool Validate(Agent agent)
        {
            return Validator?.Validate(agent) ?? true;
        }
    }
    public class FormWebStep : AbstractWebStep
    {
        public HtmlElementLocator ElementLocator { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public IWebValidator Validator { get; set; }
        public IWebCallback PreElementLocatorCallback { get; set; }

        public FormWebStep()
        {

        }
        public FormWebStep(string name = null, HtmlElementLocator locator = null, Dictionary<string, string> parameters = null, string method = "submit", IWebValidator validator = null, IWebCallback preElementLocatorCallback = null)
            : base(name)
        {
            ElementLocator = locator;
            Method = method;
            Parameters = parameters;
            Validator = validator;
            PreElementLocatorCallback = preElementLocatorCallback;
        }
        public override void InternalExecute(Agent agent)
        {
            HtmlElement element;
            if (null != Parameters)
            {
                foreach (var key in Parameters.Keys)
                {
                    element = agent?.WebBrowser.Document?.GetElementById(key);
                    if (null == element) continue;
                    var value = Parameters[key];
                    var valueObj = agent.RequestContext.ContainsKey(value) ? agent.RequestContext[value] : null;
                    object exprObj = null;
                    try
                    {
                        exprObj = ExpressionEvaluator.GetValue(agent, value);
                        if (!IsNull(exprObj))
                        {
                            value = exprObj.ToString();
                        }
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                    if (IsNull(exprObj))
                    {
                        if (!IsNull(valueObj))
                        {
                            value = valueObj.ToString();
                        }
                    }
                    else
                    {
                        value = exprObj.ToString();
                    }
                    element.SetAttribute("value", value);
                }
            }
            PreElementLocatorCallback?.Callback(agent);
            if (IsNull(ElementLocator)) return;
            element = ElementLocator.Locate(agent);
            element?.InvokeMember(Method);
        }
        public override bool Validate(Agent agent)
        {
            return Validator?.Validate(agent) ?? true;
        }
    }
    public class ClickWebStep : AbstractWebStep
    {
        public HtmlElementLocator ElementLocator { get; set; }
        public string Method { get; set; }
        public IWebValidator Validator { get; set; }

        public ClickWebStep()
        {

        }
        public ClickWebStep(string name = null, HtmlElementLocator locator = null, string method = "click", IWebValidator validator = null)
            : base(name)
        {
            ElementLocator = locator;
            Method = method;
            Validator = validator;
        }
        public override void InternalExecute(Agent agent)
        {
            if (IsNull(ElementLocator)) return;
            var element = ElementLocator.Locate(agent);
            element?.InvokeMember(Method);
        }
        public override bool Validate(Agent agent)
        {
            return Validator?.Validate(agent) ?? true;
        }
    }
    public class CookieClearWebStep : AbstractWebStep
    {
        public IWebValidator Validator { get; set; }

        public CookieClearWebStep()
        {

        }
        public CookieClearWebStep(string name = null)
            : base(name)
        {
        }
        public override void InternalExecute(Agent agent)
        {
            if (!IsNull(agent.WebBrowser.Document)) agent.WebBrowser.Document.Cookie = null;
        }

        public override bool Validate(Agent agent)
        {
            return Validator?.Validate(agent) ?? true;
        }
    }
    public class MonitorWebStep : AbstractWebStep
    {
        public int SleepTime { get; set; }
        public int MaxCount { get; set; }
        public IWebValidator Validator { get; set; }
        public MonitorWebStep()
        {

        }
        public MonitorWebStep(string name = null, int sleepTime = 500, int maxCount = 120, IWebValidator validator = null)
            : base(name)
        {
            SleepTime = sleepTime;
            MaxCount = maxCount;
            Validator = validator;
        }
        public override void InternalExecute(Agent agent)
        {
            var count = 0;
            var done = false;
            MethodInvoker delegateCall = delegate
            {
                if (Validator.Validate(agent))
                {
                    done = true;
                    Console.WriteLine("Thread Completed");
                }
                else
                {
                    Console.WriteLine("Thread Started");
                }
            };
            while (!done && count < MaxCount)
            {
                Thread.Sleep(SleepTime);
                if (agent.WebBrowser.InvokeRequired)
                {
                    agent.WebBrowser.Invoke(delegateCall);
                }
                else
                {
                    delegateCall();
                }
                count++;
            }
        }
        public override bool Validate(Agent agent)
        {
            return Validator?.Validate(agent) ?? true;
        }
    }
    public class TimedProxyWebStep : AbstractWebStep
    {
        public System.Threading.Timer Timer { get; set; }
        public IWebStep Step { get; set; }
        public IWebValidator Validator { get; set; }
        public TimedProxyWebStep(IWebStep webStep = null, IWebValidator validator = null)
        {
            Step = webStep;
            Validator = validator;
        }
        public override void InternalExecute(Agent agent)
        {
            var ret = Validator.Validate(agent);
            if (ret || !IsNull(Timer)) return;
            TimerCallback callback = TimerCallback;
            Timer = new System.Threading.Timer(callback, agent, 500, 500);
        }

        public override bool Validate(Agent agent)
        {
            return Validator?.Validate(agent) ?? true;
        }
        public void TimerCallback(object argument)
        {
            if (!Validator.Validate((Agent) argument)) return;
            Timer.Dispose();
            Timer = null;
            Step.Execute((Agent)argument);
        }
    }

}
