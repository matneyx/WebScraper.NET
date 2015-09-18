﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;

namespace WebScraper.Web
{
    public delegate bool ValueValidateDelegate(String value);

    public interface WebValidator
    {
        bool validate(Agent agent);
    }
    public abstract class AbstractWebValidator : WebValidator
    {
        public bool validate(Agent agent)
        {
            bool ret = false;
            MethodInvoker delegateCall = delegate
            {
                ret = internalValidate(agent);
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
        public abstract bool internalValidate(Agent agent);

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
            this.Title = title;
            this.TitleRegex = titleRegex;
        }
        public override bool internalValidate(Agent agent)
        {
            bool ret = false;
            string title = agent.WebBrowser.Document.Title;
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
        public override bool internalValidate(Agent agent)
        {
            bool ret = false;
            HtmlElement element = Locator.locate(agent);
            ret = null != element;
            return ret;
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
            this.Locator = locator;
            this.AttributeName = attributeName;
            this.Value = value;
            this.ValueRegex = valueRegex;
        }
        public override bool internalValidate(Agent agent)
        {
            bool ret = false;
            if (null != Locator)
            {
                HtmlElement element = Locator.locate(agent);
                if (null != element)
                {
                    if (null == AttributeName)
                    {
                        if (null == ValueRegex)
                        {
                            ret = element.InnerText.Equals(Value);
                        }
                        else
                        {
                            ret = ValueRegex.IsMatch(element.InnerText);
                        }
                    }
                    else
                    {
                        String value = element.GetAttribute(AttributeName);
                        if (null != value)
                        {
                            if (null == ValueRegex)
                            {
                                ret = value.Equals(Value);
                            }
                            else
                            {
                                ret = ValueRegex.IsMatch(value);
                            }
                        }

                    }
                }
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
            this.Locator = locator;
            this.Value = value;
            this.ValueRegex = valueRegex;
            this.CheckDelegate = checkDelegate;
        }
        public override bool internalValidate(Agent agent)
        {
            bool ret = false;
            if (null != Locator)
            {
                HtmlElement element = Locator.locate(agent);
                if (null != element)
                {
                    String value = element.Style;
                    if (null == CheckDelegate)
                    {
                        if (null != value)
                        {
                            if (null == ValueRegex)
                            {
                                ret = value.Equals(Value);
                            }
                            else
                            {
                                ret = ValueRegex.IsMatch(value);
                            }
                        }
                    }
                    else
                    {
                        ret = CheckDelegate(value);
                    }
                }
            }
            return ret;
        }
    }

    public class TimedProxyWebValidator : AbstractWebValidator
    {
        public WebValidator Validator { get; set; }
        public System.Threading.Timer Timer { get; set; }
        public TimedProxyWebValidator(WebValidator validator = null)
        {
            Validator = validator;
        }
        public override bool internalValidate(Agent agent)
        {
            bool ret = false;
            ret = Validator.validate(agent);
            if (!ret)
            {
                if (null == Timer)
                {
                    TimerCallback callback = timerCallback;
                    Timer = new System.Threading.Timer(callback, agent, 500, 500);
                }
            }
            return ret;
        }
        public void timerCallback(Object argument)
        {
            if (Validator.validate((Agent)argument))
            {
                Timer.Dispose();
                Timer = null;
                ((Agent)argument).CompletedWaitAction();
            }
        }
    }


}
