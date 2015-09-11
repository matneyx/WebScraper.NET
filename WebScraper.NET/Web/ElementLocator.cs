namespace WebScraper.NET.Web
{
    public interface IElementLocator<out T>
    {
        string GetName();
        T Locate(Agent agent);
    }
    public interface IElementMatcher<in T>
    {
        string GetName();
        bool Match(T element);
    }
    public interface IDataExtractor<in T, out TV>
    {
        TV Extract(T element);
    }
    public enum ElementTarget
    {
        Self, Children, AllChildren
    }
}
