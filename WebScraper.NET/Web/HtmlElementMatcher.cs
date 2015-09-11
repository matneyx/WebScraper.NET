using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WebScraper.NET.Web
{
    public abstract class AbstractHtmlElementMatcher : ExtensionMethods, IElementMatcher<HtmlElement>
    {
        public string Name { get; set; }
        public AbstractHtmlElementMatcher()
        {

        }
        public AbstractHtmlElementMatcher(string name = null)
        {
            Name = name;
        }
        public string GetName()
        {
            return Name;
        }
        public abstract bool Match(HtmlElement element);
    }

    public class AttributeHtmlElementMatcher : AbstractHtmlElementMatcher
    {
        public Regex TagValueRegex { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        public Dictionary<string, Regex> AttributeRegexs { get; set; }

        public AttributeHtmlElementMatcher()
        {

        }
        public AttributeHtmlElementMatcher(string name = null, string attribute = null, string value = null)
            : base(name)
        {
            Attributes = new Dictionary<string, string> {[attribute] = value};
        }
        public AttributeHtmlElementMatcher(string name = null, Regex tagValueRegex = null, Dictionary<string, string> attributes = null, Dictionary<string, Regex> attributeRegexs = null)
            : base(name)
        {
            TagValueRegex = tagValueRegex;
            Attributes = attributes;
            AttributeRegexs = attributeRegexs;
        }

        public override bool Match(HtmlElement element)
        {
            var foundAtr = true;
            if (!IsNull(Attributes))
            {
                foreach (var key in Attributes.Keys)
                {
                    var value = element.GetAttribute(key);
                    if (value.Equals(Attributes[key])) continue;
                    foundAtr = false;
                    break;
                }
            }
            var foundAtrReg = true;
            if (!IsNull(AttributeRegexs))
            {
                foreach (var key in AttributeRegexs.Keys)
                {
                    var value = element.GetAttribute(key);
                    var regex = AttributeRegexs[key];
                    if (regex.IsMatch(value)) continue;
                    foundAtrReg = false;
                    break;
                }
            }
            var foundValue = true;
            if (!IsNull(TagValueRegex))
            {
                foundValue = !IsNull(element.InnerText) && TagValueRegex.IsMatch(element.InnerText);
            }

            return foundAtr && foundValue && foundAtrReg;
        }
    }

    public class IdHtmlElementMatcher : AttributeHtmlElementMatcher
    {
        public IdHtmlElementMatcher()
        {

        }
        public IdHtmlElementMatcher(string name = null, string value = null)
            : base(name, "id", value)
        {
        }

    }

}
