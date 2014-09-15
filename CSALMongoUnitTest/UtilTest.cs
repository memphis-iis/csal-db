using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MongoDB.Bson;

using CSALMongo;

namespace CSALMongoUnitTest {
    [TestClass]
    public class UtilTest {
        private BsonDocument topLevel;
        private BsonDocument subLevel;

        [TestInitialize]
        public void SetUp() {
            topLevel = BsonDocument.Parse(@"{
                'nullDoc': null,
                'emptyDoc': {},
                'emptyArray': [],
                'realArray': [1,2,3],
                'numVal': 42
            }");
            subLevel = BsonDocument.Parse("{'isDoc':'yes'}");

            topLevel.Add("testchild", subLevel);
        }

        [TestMethod]
        public void TestExtractDoc() {
            //null parms get null returns
            Assert.IsNull(Util.ExtractDoc(null, "notthere"));
            Assert.IsNull(Util.ExtractDoc(new BsonDocument(), null));
            Assert.IsNull(Util.ExtractDoc(new BsonDocument(), ""));

            //Should get an empty document
            Assert.AreEqual(0, Util.ExtractDoc(topLevel, "nullDoc").ElementCount);
            Assert.AreEqual(0, Util.ExtractDoc(topLevel, "emptyDoc").ElementCount);
            Assert.AreEqual(0, Util.ExtractDoc(topLevel, "emptyArray").ElementCount);
            Assert.AreEqual(0, Util.ExtractDoc(topLevel, "realArray").ElementCount);
            Assert.AreEqual(0, Util.ExtractDoc(topLevel, "numVal").ElementCount);

            //Should get a "real" document
            Assert.AreEqual("yes", Util.ExtractDoc(topLevel, "testchild").GetValue("isDoc").AsString);
        }

        [TestMethod]
        public void TestExtractArray() {
            //null parms get null returns
            Assert.IsNull(Util.ExtractArray(null, "notthere"));
            Assert.IsNull(Util.ExtractArray(new BsonDocument(), null));
            Assert.IsNull(Util.ExtractArray(new BsonDocument(), ""));

            //Should get an empty array
            Assert.AreEqual(0, Util.ExtractArray(topLevel, "nullDoc").Count);
            Assert.AreEqual(0, Util.ExtractArray(topLevel, "emptyDoc").Count);
            Assert.AreEqual(0, Util.ExtractArray(topLevel, "emptyArray").Count);
            Assert.AreEqual(0, Util.ExtractArray(topLevel, "numVal").Count);

            //Should get a "real" array
            var realList = new List<int>();
            foreach(BsonValue i in Util.ExtractArray(topLevel, "realArray")) {
                realList.Add(i.AsInt32);
            }
            CollectionAssert.AreEquivalent(new int[] { 1, 2, 3 }, realList);
        }
    }
}
