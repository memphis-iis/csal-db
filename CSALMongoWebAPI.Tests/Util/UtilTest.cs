using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSALMongoWebAPI.Util;

namespace CSALMongoWebAPI.Tests.Util {
    [TestClass]
    public class UtilTest {
        [TestMethod]
        public void URIEncoding() {
            URIRoundTrip(null);
            URIRoundTrip(" ");
            URIRoundTrip("");
            URIRoundTrip("Hello World");
        }
        private void URIRoundTrip(string src) {
            Assert.AreEqual(src, RenderHelp.URIDecode(RenderHelp.URIEncode(src)));
        }

        [TestMethod]
        public void HumanDuration() {
            Assert.AreEqual("", RenderHelp.HumanDuration(-1.0));
            Assert.AreEqual("", RenderHelp.HumanDuration(0.0));
            Assert.AreEqual("< 1 min", RenderHelp.HumanDuration(2.0));
            Assert.AreEqual("2 mins", RenderHelp.HumanDuration(129.2 * 1000.0));
            Assert.AreEqual("4 hrs", RenderHelp.HumanDuration(4.1 * 60 * 60 * 1000.0));
        }
    }
}
