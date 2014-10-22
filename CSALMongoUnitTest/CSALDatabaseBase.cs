using System;
using System.IO;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MongoDB.Driver;
using MongoDB.Bson;

namespace CSALMongoUnitTest {
    // Base test class with setup code to read sample instance and clear
    // previous DB contents.
    public class CSALDatabaseBase {
        public const string DB_URL = "mongodb://localhost:27017/csaltest";

        protected MongoDatabase testDB = null;

        protected string SAMPLE_RAW_USER = "";
        protected string SAMPLE_RAW_LESSON = "";

        [TestInitialize]
        public void SetUp() {
            var url = new MongoUrl(DB_URL);
            testDB = new MongoClient(url).GetServer().GetDatabase(url.DatabaseName);
            testDB.Drop();

            var sampleRaw = BsonDocument.Parse(GetSampleRawAct());
            SAMPLE_RAW_USER = sampleRaw["UserID"].AsString.ToLowerInvariant();
            SAMPLE_RAW_LESSON = sampleRaw["LessonID"].AsString.ToLowerInvariant();

            Assert.IsFalse(String.IsNullOrWhiteSpace(SAMPLE_RAW_USER));
            Assert.IsFalse(String.IsNullOrWhiteSpace(SAMPLE_RAW_LESSON));
        }

        [TestCleanup]
        public void TearDown() {
            testDB = null;
        }

        protected string GetJSON(object obj) {
            string ret = obj.ToBsonDocument().ToJson();
            Debug.Print(ret);  //Nice to see if test fails
            return ret;
        }

        protected string GetSampleRawAct() {
            return Properties.Resources.SampleRawAct;
        }
    }
}
