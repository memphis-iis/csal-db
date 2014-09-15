using System;
using System.IO;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MongoDB.Driver;
using MongoDB.Bson;

//TODO: Insure documentation on all public namespaces, classes, and methods
//     (ALSO top-level documentation directory with compiled help - tag version for that)

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
            SAMPLE_RAW_USER = sampleRaw["UserID"].AsString;
            SAMPLE_RAW_LESSON = sampleRaw["LessonID"].AsString;

            if (String.IsNullOrWhiteSpace(SAMPLE_RAW_USER)) {
                throw new InvalidDataException("Could not find Sample Raw Act Student");
            }
            if (String.IsNullOrWhiteSpace(SAMPLE_RAW_LESSON)) {
                throw new InvalidDataException("Could not find Sample Raw Act Lesson");
            }
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
