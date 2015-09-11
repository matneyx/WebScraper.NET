using System.Threading;
using System.Windows.Forms;

namespace WebScraper.NET.Web
{
    public interface IWebCallback
    {
        void Callback(Agent agent);
    }
    public class BackgroundInvoke : ExtensionMethods, IWebCallback
    {
        public string ElementId { get; set; }
        public string MethodName { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }

        public BackgroundInvoke(string elementId = null, string methodName = "click", string attributeName = null, string attributeValue = null)
        {
            ElementId = elementId;
            MethodName = methodName;
            AttributeName = attributeName;
            AttributeValue = attributeValue;
        }
        public void Callback(Agent agent)
        {
            MethodInvoker delegateCall = delegate
            {
                var element = agent.WebBrowser.Document?.GetElementById(ElementId);
                if (IsNull(element)) return;
                if (!IsNull(AttributeName))
                {
                    element?.SetAttribute(AttributeName, AttributeValue);
                }
                element?.InvokeMember(MethodName);
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
    }
    public class BlockingCallback : IWebCallback
    {
        public void Callback(Agent agent)
        {
            Thread.Sleep(10000);
        }
    }
    public class SendKeysCallback : IWebCallback
    {
        public string SendKey { get; set; }
        public SendKeysCallback(string sendKey = null)
        {
            SendKey = sendKey;
        }

        public void Callback(Agent agent)
        {
            agent.WebBrowser.Focus();
            SendKeys.SendWait(SendKey);
        }
    }
}
