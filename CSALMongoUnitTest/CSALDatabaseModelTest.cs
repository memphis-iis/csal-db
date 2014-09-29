using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSALMongo;

namespace CSALMongoUnitTest {
    // Some of our model classes have non-default property operations. We
    // also want to make sure that BSON/JSON/Object serialization works
    [TestClass]
    public class CSALDatabaseModelTest : CSALDatabaseBase {

        [TestMethod]
        public void TestLessonModel() {
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct(Properties.Resources.SampleRawAct);

            var origLesson = db.FindLessons()[0];
            var lesson1 = db.FindLessons()[0];
            var lesson2 = db.FindLessons()[0];

            Assert.AreEqual(SAMPLE_RAW_LESSON.ToLowerInvariant(), lesson1.Id);
            Assert.AreEqual(SAMPLE_RAW_LESSON.ToLowerInvariant(), lesson1.LessonID);
            Assert.AreEqual(SAMPLE_RAW_LESSON.ToLowerInvariant(), lesson2.Id);
            Assert.AreEqual(SAMPLE_RAW_LESSON.ToLowerInvariant(), lesson2.LessonID);

            //Need to verify that lesson ID and _id are tied together
            lesson1.Id = "changed2";
            lesson2.LessonID = "changed2";

            Assert.AreEqual("changed2", lesson1.Id);
            Assert.AreEqual("changed2", lesson1.LessonID);
            Assert.AreEqual("changed2", lesson2.Id);
            Assert.AreEqual("changed2", lesson2.LessonID);

            //Also need to check that we didn't somehow break JSON compat
            lesson2.Id = SAMPLE_RAW_LESSON;
            lesson1.LessonID = SAMPLE_RAW_LESSON;
            Assert.AreEqual(GetJSON(origLesson), GetJSON(lesson1));
            Assert.AreEqual(GetJSON(lesson1), GetJSON(lesson2));
        }

        [TestMethod]
        public void TestStudentModel() {
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct(Properties.Resources.SampleRawAct);

            var origStudent = db.FindStudents()[0];
            var student1 = db.FindStudents()[0];
            var student2 = db.FindStudents()[0];

            Assert.AreEqual(SAMPLE_RAW_USER.ToLowerInvariant(), student1.Id);
            Assert.AreEqual(SAMPLE_RAW_USER.ToLowerInvariant(), student1.UserID);
            Assert.AreEqual(SAMPLE_RAW_USER.ToLowerInvariant(), student2.Id);
            Assert.AreEqual(SAMPLE_RAW_USER.ToLowerInvariant(), student2.UserID);

            //Need to verify that lesson ID and _id are tied together
            student1.Id = "changed2";
            student2.UserID = "changed2";

            Assert.AreEqual("changed2", student1.Id);
            Assert.AreEqual("changed2", student1.UserID);
            Assert.AreEqual("changed2", student2.Id);
            Assert.AreEqual("changed2", student2.UserID);

            //Also need to check that we didn't somehow break JSON compat
            student2.Id = SAMPLE_RAW_USER;
            student1.UserID = SAMPLE_RAW_USER;
            Assert.AreEqual(GetJSON(origStudent), GetJSON(student1));
            Assert.AreEqual(GetJSON(student1), GetJSON(student2));
        }

        [TestMethod]
        public void TestClassModel() {
            var clazz = new CSALMongo.Model.Class { ClassID="TestClass", Location="RightHere", TeacherName="Teach" };

            Assert.AreEqual("TestClass", clazz.Id);
            Assert.AreEqual("TestClass", clazz.ClassID);

            clazz.Id = "TestClass2";

            Assert.AreEqual("TestClass2", clazz.Id);
            Assert.AreEqual("TestClass2", clazz.ClassID);
        }

        [TestMethod]
        public void TestStudentActTotalDuration() {
            var turns = new CSALMongo.Model.StudentLessonActs();
            Assert.AreEqual(0.0, turns.TotalDuration());

            turns.Turns = new List<CSALMongo.TurnModel.ConvLog>();

            turns.Turns.Add(new CSALMongo.TurnModel.ConvLog());
            Assert.AreEqual(0.0, turns.TotalDuration());

            turns.Turns[0].Duration = 1.0;
            Assert.AreEqual(1.0, turns.TotalDuration());

            turns.Turns.Add(new CSALMongo.TurnModel.ConvLog { Duration = 2.0 });
            Assert.AreEqual(3.0, turns.TotalDuration());

            turns.Turns.Add(new CSALMongo.TurnModel.ConvLog());
            Assert.AreEqual(3.0, turns.TotalDuration());
        }

        [TestMethod]
        public void TestStudentActMeanDuration() {
            var turns = new CSALMongo.Model.StudentLessonActs();
            Assert.AreEqual(0.0, turns.MeanDuration());

            turns.Turns = new List<CSALMongo.TurnModel.ConvLog>();

            turns.Turns.Add(new CSALMongo.TurnModel.ConvLog());
            Assert.AreEqual(0.0, turns.MeanDuration());

            turns.Turns[0].Duration = 1.0;
            Assert.AreEqual(1.0, turns.MeanDuration());

            turns.Turns.Add(new CSALMongo.TurnModel.ConvLog { Duration = 2.0 });
            Assert.AreEqual(1.5, turns.MeanDuration());

            turns.Turns.Add(new CSALMongo.TurnModel.ConvLog());
            Assert.AreEqual(1.0, turns.MeanDuration());
        }
    }
}
