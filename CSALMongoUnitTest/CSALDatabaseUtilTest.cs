using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSALMongo;

namespace CSALMongoUnitTest {
    /// <summary>
    /// Test class for testing utility methods of the CSALDatabase class.
    /// Note that we do this via an inherit-and-expose strategy
    /// </summary>
    [TestClass]
    public class CSALDatabaseUtilTest : CSALDatabaseBase {
        private class CSALDBTester : CSALDatabase {
            public CSALDBTester(string url)
                : base(url) {
                //Nothing
            }

            public string ExtractLessonIDExp(string fullLessonID) {
                return ExtractLessonID(fullLessonID);
            }
        }

        private CSALDBTester csaldb;

        [TestInitialize]
        public void AdditionSetUp() {
            csaldb = new CSALDBTester(DB_URL);
        }

        [TestMethod]
        public void TestExtractLessonID() {
            Assert.AreEqual(null, csaldb.ExtractLessonIDExp(null));
            Assert.AreEqual("", csaldb.ExtractLessonIDExp(""));
            Assert.AreEqual("  ", csaldb.ExtractLessonIDExp("  "));
            Assert.AreEqual("ftp://host.com/lesson1/1", csaldb.ExtractLessonIDExp("ftp://host.com/lesson1/1"));
            Assert.AreEqual("http://host.com/nope/1", csaldb.ExtractLessonIDExp("http://host.com/nope/1"));
            Assert.AreEqual("http://host/lesson1", csaldb.ExtractLessonIDExp("http://host/lesson1"));
            Assert.AreEqual("lesson1", csaldb.ExtractLessonIDExp("http://host.com/lesson1/1"));
        }
    }
}
