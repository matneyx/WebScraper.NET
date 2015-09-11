using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WebScraper.NET.Web
{
    public abstract class HtmlElementLocator : ExtensionMethods, IElementLocator<HtmlElement>
    {
        public string Name { get; set; }
        public string ContextKey { get; set; }
        public HtmlElementLocator()
        {

        }
        public HtmlElementLocator(string name = null, string contextKey = null)
        {
            Name = name;
            ContextKey = contextKey;
        }
        public string GetName()
        {
            return Name;
        }
        public HtmlElement Locate(Agent agent)
        {
            HtmlElement ret = null;
            MethodInvoker delegateCall = delegate
            {
                ret = InternalLocate(agent);
                if (!IsNull(ContextKey))
                {
                    agent.RequestContext.Add(ContextKey, ret);
                }

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
        public abstract HtmlElement InternalLocate(Agent agent);
    }

    public class SimpleHtmlElementLocator : HtmlElementLocator
    {
        public IElementMatcher<HtmlElement> Matcher { get; set; }

        public SimpleHtmlElementLocator()
        {

        }
        public SimpleHtmlElementLocator(string name = null, IElementMatcher<HtmlElement> matcher = null
            )
            : base(name)
        {
            Matcher = matcher;
        }

        public override HtmlElement InternalLocate(Agent agent)
        {
            return agent?.WebBrowser.Document?.All.Cast<HtmlElement>().FirstOrDefault(element => Matcher.Match(element));
        }
    }


    public class IdElementLocator : HtmlElementLocator
    {
        public string Id { get; set; }
        public IElementMatcher<HtmlElement> Matcher { get; set; }

        public IdElementLocator()
        {

        }

        public IdElementLocator(string name = null, string id = null, string contextKey = null, IElementMatcher<HtmlElement> matcher = null)
            : base(name, contextKey)
        {
            Id = id;
            Matcher = matcher;
        }

        public override HtmlElement InternalLocate(Agent agent)
        {
            HtmlElement ret = null;
            if (!IsNull(agent?.WebBrowser.Document))
            {
                ret = agent.WebBrowser.Document.GetElementById(Id);
            }
            if (IsNull(ret) || IsNull(Matcher)) return ret;
            if (!Matcher.Match(ret))
            {
                ret = null;
            }
            return ret;
        }
    }

    public class TagElementLocator : HtmlElementLocator
    {
        public string Tag { get; set; }
        public bool Recursive { get; set; }
        public IElementMatcher<HtmlElement> Matcher { get; set; }
        public IChildHtmlElementLocator ChildLocator { get; set; }
        public TagElementLocator()
        {

        }

        public TagElementLocator(string name = null, string tag = null, bool recursive = false, string contextKey = null, IChildHtmlElementLocator childLocator = null, IElementMatcher<HtmlElement> matcher = null)
            : base(name, contextKey)
        {
            Tag = tag;
            Recursive = recursive;
            ChildLocator = childLocator;
            Matcher = matcher;
        }

        private HtmlElement LocateInternal(HtmlDocument document)
        {
            HtmlElement ret = null;
            if (IsNull(document)) return null;
            var matches = new List<HtmlElement>();
            var elements = document.GetElementsByTagName(Tag);
            foreach (HtmlElement element in elements)
            {
                if (IsNull(Matcher) || Matcher.Match(element))
                {
                    ret = ChildLocator == null ? element : ChildLocator.Locate(element);
                    if (null != ret)
                    {
                        break;
                    }
                }
            }
            if (ret == null && Recursive)
            {
                if (!IsNull(document.Window?.Frames) && 0 < document.Window?.Frames?.Count)
                {
                    foreach (HtmlWindow window in document.Window.Frames)
                    {
                        ret = LocateInternal(window.Document);
                        if (IsNull(ret))
                        {
                            break;
                        }
                    }
                }
            }
            return ret;
        }

        public override HtmlElement InternalLocate(Agent agent)
        {
            return IsNull(agent) ? null : LocateInternal(agent.WebBrowser.Document);
        }
    }

}
