using System;
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
        public void TestClassIsATeacher() {
            var clazz = new CSALMongo.Model.Class { ClassID = "TestClass", Location = "RightHere"};

            //Default/null value for teacher means no teacher...
            Assert.IsFalse(clazz.IsATeacher(null));
            Assert.IsFalse(clazz.IsATeacher(""));
            Assert.IsFalse(clazz.IsATeacher(" "));
            Assert.IsFalse(clazz.IsATeacher("a"));

            //And so does empty string
            clazz.TeacherName = "";
            Assert.IsFalse(clazz.IsATeacher(null));
            Assert.IsFalse(clazz.IsATeacher(""));
            Assert.IsFalse(clazz.IsATeacher(" "));
            Assert.IsFalse(clazz.IsATeacher("a"));

            //Check for single value
            clazz.TeacherName = "teach1";
            Assert.IsFalse(clazz.IsATeacher(null));
            Assert.IsFalse(clazz.IsATeacher(""));
            Assert.IsFalse(clazz.IsATeacher(" "));
            Assert.IsFalse(clazz.IsATeacher("z"));
            Assert.IsTrue(clazz.IsATeacher("teach1"));

            //Check for two values
            clazz.TeacherName = "teach1,teach2";
            Assert.IsFalse(clazz.IsATeacher(null));
            Assert.IsFalse(clazz.IsATeacher(""));
            Assert.IsFalse(clazz.IsATeacher(" "));
            Assert.IsFalse(clazz.IsATeacher("teach3"));
            Assert.IsTrue(clazz.IsATeacher("teach1"));
            Assert.IsTrue(clazz.IsATeacher("teach2"));
        }

        [TestMethod]
        public void TestStudentLessonActTotalTime() {
            var studentLesson = new CSALMongo.Model.StudentLessonActs();
            studentLesson.Turns = new List<CSALMongo.TurnModel.ConvLog>();

            double start_time = 160050031753.0; //Just a date chosen for testing
            double hr_ms = 60.0 * 60.0 * 1000.0; //1 hour in millisecond

            const int TURN_ID_START = CSALMongo.Model.StudentLessonActs.TURN_ID_START;

            studentLesson.Turns.Add(new CSALMongo.TurnModel.ConvLog { TurnID = TURN_ID_START, DBTimestamp = start_time, Duration = 100.0 });

            start_time += (2 * hr_ms);

            studentLesson.Turns.Add(new CSALMongo.TurnModel.ConvLog { TurnID = TURN_ID_START, DBTimestamp = start_time, Duration = 100.0 });
            studentLesson.Turns.Add(new CSALMongo.TurnModel.ConvLog { TurnID = 2, DBTimestamp = start_time + 101, Duration = 100.0 });

            Assert.IsTrue(MsClose(301.0, studentLesson.TotalTime(0)));
            Assert.IsTrue(MsClose(201.0, studentLesson.CurrentTotalTime()));
        }

        private bool MsClose(double msExp, double msAct) {
            bool ret = Math.Abs(msExp - msAct) <= 0.0001;
            if (!ret) {
                Console.Error.WriteLine(String.Format("Double's aren't close: Exp={0:0.00000}, Act={1:0.00000}", msExp, msAct));
            }
            return ret;
        }
    }
}
