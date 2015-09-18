using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Moq;
using NUnit.Framework;
using WebScraper.Web;
using static System.Threading.Thread;

namespace WebScraper.NET.Tests
{
    [TestFixture]
    public class AccessTimingTests
    {
        [Test]
        public void AccessTimingTest()
        {
            var mockAccessTiming = new Mock<AccessTiming>(new Uri("http://www.google.com"), 1000);

            Assert.AreEqual("http://www.google.com/", mockAccessTiming.Object.URI.ToString());
            Assert.AreEqual("1000",mockAccessTiming.Object.TimingInTicks.ToString());
            Assert.AreEqual(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),mockAccessTiming.Object.StartTime.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),mockAccessTiming.Object.CurrentStartTime.ToString(CultureInfo.InvariantCulture));

        }

        [Test]
        public void MarkTimingTest()
        {
            var mockAccessTiming = new Mock<AccessTiming>(new Uri("http://www.google.com"),1000);

            var now = mockAccessTiming.Object.StartTime;

            mockAccessTiming.Object.MarkTiming();

            var newNow = 1000 + (DateTime.UtcNow.Ticks - now.Ticks);

            Assert.AreEqual(newNow.ToString(), mockAccessTiming.Object.TimingInTicks.ToString());


        }


        [Test]
        public void StartTimingTest()
        {
            var mockAccessTiming = new Mock<AccessTiming>(new Uri("http://www.google.com"), 1000);

            Sleep(1000);

            mockAccessTiming.Object.StartTiming();
            var now = DateTime.UtcNow;

            Assert.AreEqual(now.ToString(CultureInfo.InvariantCulture), mockAccessTiming.Object.CurrentStartTime.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void AddTimingTest()
        {
            var mockAccessTiming = new Mock<AccessTiming>(new Uri("http://www.google.com"), 1000);

            mockAccessTiming.Object.AddTiming(mockAccessTiming.Object);

            Assert.AreEqual("2000", mockAccessTiming.Object.TimingInTicks.ToString());
        }

        [Test]
        public void GetTimingInSecondsTest()
        {
            var mockAccessTiming = new Mock<AccessTiming>(new Uri("http://www.google.com"), 1000);

            Assert.AreEqual("0.0001", mockAccessTiming.Object.GetTimingInSeconds().ToString(CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    public class AgentTests
    {


        [Test]
        [RequiresSTA]
        public void AgentTest()
        {
            var mockAgent = new Mock<Agent>(new WebBrowser {Name = "Chrome"});

            Assert.NotNull(mockAgent.Object.WebBrowser);
            Assert.AreEqual("Chrome",mockAgent.Object.WebBrowser.Name);
        }

        [Test]
        public void InitTest()
        {
            var mockAgent = new Mock<Agent> {CallBase = true};

            mockAgent.Object.Init();

            mockAgent.Object.RequestContext.Add("Waffles",null);
            mockAgent.Object.Outputs.Add("Mouth",null);

            Assert.IsTrue(mockAgent.Object.RequestContext.ContainsKey("Waffles"));
            Assert.IsTrue(mockAgent.Object.Outputs.ContainsKey("Mouth"));
           
            // Assert.AreEqual("",mockAgent.Object.Trigger);
            // TODO : Figure out how to test an AutoResetEvent that has been set to false
        }

        
    }
}
