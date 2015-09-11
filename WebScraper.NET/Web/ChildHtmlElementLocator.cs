using System.Linq;
using System.Windows.Forms;

namespace WebScraper.NET.Web
{
    public interface IChildHtmlElementLocator
    {
        string GetName();
        HtmlElement Locate(HtmlElement parent);
    }

    public abstract class AbstractChildHtmlElementLocator : IChildHtmlElementLocator
    {
        public string Name { get; set; }
        public AbstractChildHtmlElementLocator()
        {

        }
        public AbstractChildHtmlElementLocator(string name = null)
        {
            Name = name;
        }
        public string GetName()
        {
            return Name;
        }
        public abstract HtmlElement Locate(HtmlElement parent);
    }

    public class SimpleChildHtmlElementLocator : AbstractChildHtmlElementLocator
    {
        public IElementMatcher<HtmlElement> Matcher { get; set; }
        public SimpleChildHtmlElementLocator()
        {

        }
        public SimpleChildHtmlElementLocator(string name = null, IElementMatcher<HtmlElement> matcher = null)
        {
            Matcher = matcher;
        }
        public override HtmlElement Locate(HtmlElement parent)
        {
            return parent?.All.Cast<HtmlElement>().FirstOrDefault(child => Matcher.Match(child));
        }

    }

}
