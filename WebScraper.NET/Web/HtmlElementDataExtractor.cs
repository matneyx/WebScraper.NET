using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WebScraper.NET.Web
{
    public interface IHtmlElementDataExtractor<V> : IDataExtractor<HtmlElement, V>
    {
    }
    public abstract class AbstractHtmlElementDataExtractor<V> : ExtensionMethods, IHtmlElementDataExtractor<V>
    {
        public string Part { get; set; }
        public AbstractHtmlElementDataExtractor()
        {

        }
        public AbstractHtmlElementDataExtractor(string part = null)
        {
            this.Part = part;
        }
        public string GetString(HtmlElement element)
        {
            var ret = string.Empty;
            if (IsNull(element)) return ret;
            if (":outerhtml".Equals(Part))
            {
                ret = element.OuterHtml;
            }
            else if (":outertext".Equals(Part))
            {
                ret = element.OuterText;
            }
            else if (":innerhtml".Equals(Part))
            {
                ret = element.InnerHtml;
            }
            else if (":innertext".Equals(Part))
            {
                ret = element.InnerText;
            }
            else
            {
                ret = element.GetAttribute(Part);
            }
            return ret;
        }
        public abstract V Extract(HtmlElement element);
    }
    public abstract class AbstractUrlHtmlElementDataExtractor<TV> : ExtensionMethods, IHtmlElementDataExtractor<TV>
    {
        public AbstractUrlHtmlElementDataExtractor()
        {

        }
        public Uri GetUrl(HtmlElement element)
        {
            return element?.Document?.Window?.Url;
        }
        public abstract TV Extract(HtmlElement element);
    }
    public abstract class AbstractCookieHtmlElementDataExtractor<V> : ExtensionMethods, IHtmlElementDataExtractor<V>
    {
        public AbstractCookieHtmlElementDataExtractor()
        {

        }
        public string GetCookie(HtmlElement element)
        {
            return element?.Document?.Cookie;
        }
        public abstract V Extract(HtmlElement element);
    }

    public class StringHtmlElementDataExtractor : AbstractHtmlElementDataExtractor<string>
    {
        public StringHtmlElementDataExtractor()
        {

        }
        public StringHtmlElementDataExtractor(string part = ":innertext")
            : base(part: part)
        {
        }
        public override string Extract(HtmlElement element)
        {
            return GetString(element);
        }

    }

    public class BooleanHtmlElementDataExtractor : AbstractHtmlElementDataExtractor<bool>
    {
        public BooleanHtmlElementDataExtractor()
        {

        }
        public BooleanHtmlElementDataExtractor(string part = ":innertext")
            : base(part: part)
        {
        }
        public override bool Extract(HtmlElement element)
        {
            var text = GetString(element);
            var ret = (!IsNull(text) && !text.Trim().ToLower().Equals("false"));
            return ret;
        }

    }

    public class BooleanUrlHtmlElementDataExtractor : AbstractUrlHtmlElementDataExtractor<bool>
    {
        public Regex Matcher { get; set; }
        public bool ShouldMatch { get; set; }
        public BooleanUrlHtmlElementDataExtractor()
        {
            ShouldMatch = true;
        }
        public BooleanUrlHtmlElementDataExtractor(Regex matcher = null, bool shouldMatch = true)
        {
            Matcher = matcher;
            ShouldMatch = shouldMatch;
        }
        public override bool Extract(HtmlElement element)
        {
            var url = GetUrl(element);
            if (IsNull(Matcher)) return false;
            var ret = Matcher.IsMatch(url.ToString());
            if (!ShouldMatch)
            {
                ret = !ret;
            }
            return ret;
        }

    }

    public class BooleanCookieHtmlElementDataExtractor : AbstractCookieHtmlElementDataExtractor<Boolean>
    {
        public Regex Matcher { get; set; }
        public bool ShouldMatch { get; set; }
        public BooleanCookieHtmlElementDataExtractor()
        {
            ShouldMatch = true;
        }
        public BooleanCookieHtmlElementDataExtractor(Regex matcher = null, bool shouldMatch = true)
        {
            Matcher = matcher;
            ShouldMatch = shouldMatch;
        }
        public override bool Extract(HtmlElement element)
        {

            var cookie = GetCookie(element);
            if (IsNull(Matcher)) return false;
            var ret = Matcher.IsMatch(cookie);
            if (!ShouldMatch)
            {
                ret = !ret;
            }
            return ret;
        }

    }

    public class ListHtmlElementDataExtractor<V> : AbstractHtmlElementDataExtractor<List<V>>
    {
        public IElementMatcher<HtmlElement> Matcher { get; set; }
        public IHtmlElementDataExtractor<V> Extractor { get; set; }
        public ElementTarget Target { get; set; }
        public ListHtmlElementDataExtractor()
        {

        }
        public ListHtmlElementDataExtractor(IElementMatcher<HtmlElement> matcher = null, ElementTarget target = ElementTarget.Self, IHtmlElementDataExtractor<V> extractor = null)
        {
            Matcher = matcher;
            Target = target;
            Extractor = extractor;
        }
        public override List<V> Extract(HtmlElement element)
        {
            var ret = new List<V>();
            if (Target.Equals(ElementTarget.Self))
            {
                if (IsNull(Matcher) || Matcher.Match(element))
                {
                    ret.Add(Extractor.Extract(element));
                }
            }
            else
            {
                foreach (HtmlElement childNode in Target.Equals(ElementTarget.AllChildren) ? element.All : element.Children)
                {
                    if (!IsNull(Matcher) && !Matcher.Match(childNode))
                    {
                        continue;
                    }
                    ret.Add(Extractor.Extract(childNode));
                }
            }
            return ret;
        }

    }

}
