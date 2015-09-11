using System;
using System.Text.RegularExpressions;

namespace WebScraper.NET.Web
{
    public abstract class CookieLocator : ExtensionMethods, IElementLocator<String>
    {
        public string Name { get; set; }
        public string ContextKey { get; set; }
        public CookieLocator()
        {

        }
        public CookieLocator(string name = null, string contextKey = null)
        {
            Name = name;
            ContextKey = contextKey;
        }
        public string GetName()
        {
            return Name;
        }
        public abstract string Locate(Agent agent);
    }

    public class CookieElementLocator : CookieLocator
    {
        public Regex NameRegex { get; set; }

        public CookieElementLocator()
        {

        }
        public CookieElementLocator(string name = null, Regex nameRegex = null, string contextKey = null)
            : base(name, contextKey)
        {
            NameRegex = nameRegex;
        }

        public override string Locate(Agent agent)
        {
            var ret = string.Empty;
            var cookieValue = agent?.WebBrowser.Document?.Cookie;
            if (!IsNull(cookieValue) && !IsNull(NameRegex))
            {
                if (NameRegex.IsMatch(cookieValue))
                {
                    ret = NameRegex.Match(cookieValue).Value;
                }
            }
            if (!IsNull(ContextKey))
            {
                agent?.RequestContext.Add(ContextKey, ret);
            }
            return ret;
        }
    }

}
