using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;

using CSALMongo;

namespace CSALMongoUnitTest {
    // Some of our model classes have non-default property operations. We
    // also want to make sure that BSON/JSON/Object serialization works
    [TestClass]
    public class CSALDatabaseModelTest : CSALDatabaseBase {

        [TestMethod]
        public void TestLessonModel() {
            var db = new CSALDatabase(DB_URL);
            db.saveRawStudentLessonAct(Properties.Resources.SampleRawAct);

            var origLesson = db.findLessons()[0];
            var lesson1 = db.findLessons()[0];
            var lesson2 = db.findLessons()[0];

            Assert.AreEqual(SAMPLE_RAW_LESSON, lesson1.Id);
            Assert.AreEqual(SAMPLE_RAW_LESSON, lesson1.LessonID);
            Assert.AreEqual(SAMPLE_RAW_LESSON, lesson2.Id);
            Assert.AreEqual(SAMPLE_RAW_LESSON, lesson2.LessonID);

            //Need to verify that lesson ID and _id are tied together
            lesson1.Id = "Changed2";
            lesson2.LessonID = "Changed2";

            Assert.AreEqual("Changed2", lesson1.Id);
            Assert.AreEqual("Changed2", lesson1.LessonID);
            Assert.AreEqual("Changed2", lesson2.Id);
            Assert.AreEqual("Changed2", lesson2.LessonID);

            //Also need to check that we didn't somehow break JSON compat
            lesson2.Id = SAMPLE_RAW_LESSON;
            lesson1.LessonID = SAMPLE_RAW_LESSON;
            Assert.AreEqual(getJSON(origLesson), getJSON(lesson1));
            Assert.AreEqual(getJSON(lesson1), getJSON(lesson2));
        }

        [TestMethod]
        public void TestStudentModel() {
            var db = new CSALDatabase(DB_URL);
            db.saveRawStudentLessonAct(Properties.Resources.SampleRawAct);

            var origStudent = db.findStudents()[0];
            var student1 = db.findStudents()[0];
            var student2 = db.findStudents()[0];

            Assert.AreEqual(SAMPLE_RAW_USER, student1.Id);
            Assert.AreEqual(SAMPLE_RAW_USER, student1.UserID);
            Assert.AreEqual(SAMPLE_RAW_USER, student2.Id);
            Assert.AreEqual(SAMPLE_RAW_USER, student2.UserID);

            //Need to verify that lesson ID and _id are tied together
            student1.Id = "Changed2";
            student2.UserID = "Changed2";

            Assert.AreEqual("Changed2", student1.Id);
            Assert.AreEqual("Changed2", student1.UserID);
            Assert.AreEqual("Changed2", student2.Id);
            Assert.AreEqual("Changed2", student2.UserID);

            //Also need to check that we didn't somehow break JSON compat
            student2.Id = SAMPLE_RAW_USER;
            student1.UserID = SAMPLE_RAW_USER;
            Assert.AreEqual(getJSON(origStudent), getJSON(student1));
            Assert.AreEqual(getJSON(student1), getJSON(student2));
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
    }
}
