using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSALMongoWebAPI.Controllers;
using CSALMongo;

namespace CSALMongoWebAPI.Tests.Controllers {
    [TestClass]
    public class TurnControllerTest : Util.BaseControllerTest {
        [TestMethod]
        public void Post() {
            var controller = new TurnController();
            controller.AppSettings = this.AppSettings;

            const int ITS = 128;

            for (int i = 0; i < ITS; ++i) {
                controller.Post(GetSampleRawAct());
            }

            var db = new CSALDatabase(DB_URL);

            var lessons = db.FindLessons();
            Assert.AreEqual(1, lessons.Count);
            Assert.AreEqual(ITS, lessons[0].TurnCount);
            Assert.AreEqual(SAMPLE_RAW_LESSON, lessons[0].LessonID);
            CollectionAssert.AreEquivalent(new string[] { SAMPLE_RAW_USER }, lessons[0].Students);

            var students = db.FindStudents();
            Assert.AreEqual(1, students.Count);
            Assert.AreEqual(ITS, students[0].TurnCount);
            Assert.AreEqual(SAMPLE_RAW_USER.ToLowerInvariant(), students[0].UserID.ToLowerInvariant());

            //Raw has "simple" user ID, so no class/location information
            var classes = db.FindClasses();
            Assert.AreEqual(0, classes.Count);

            var turns = db.FindTurns(null, null);
            Assert.AreEqual(1, turns.Count);
            Assert.AreEqual(1, db.FindTurns(SAMPLE_RAW_LESSON, null).Count);
            Assert.AreEqual(1, db.FindTurns(null, SAMPLE_RAW_USER).Count);
            Assert.AreEqual(1, db.FindTurns(SAMPLE_RAW_LESSON, SAMPLE_RAW_USER).Count);

            Assert.AreEqual(SAMPLE_RAW_USER.ToLowerInvariant(), turns[0].UserID.ToLowerInvariant());
            Assert.AreEqual(SAMPLE_RAW_LESSON.ToLowerInvariant(), turns[0].LessonID.ToLowerInvariant());
            Assert.AreEqual(ITS, turns[0].Turns.Count);
        }
    }
}
