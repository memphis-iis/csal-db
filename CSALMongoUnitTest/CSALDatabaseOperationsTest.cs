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
    // Test actual DB operations
    [TestClass]
    public class CSALDatabaseOperationsTest: CSALDatabaseBase {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestBadStartupNull() {
            var db = new CSALDatabase(null);
            Assert.IsFalse(true); //Should not be here
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestBadStartupEmpty() {
            var db = new CSALDatabase("");
            Assert.IsFalse(true); //Should not be here
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestBadStartupInvalid() {
            var db = new CSALDatabase("sftp://www.google.com/testdb");
            Assert.IsFalse(true); //Should not be here
        }

        [TestMethod]
        public void TestEmpties() {
            var db = new CSALDatabase(DB_URL);
            Assert.AreEqual(0, db.FindLessons().Count);
            Assert.AreEqual(0, db.FindStudents().Count);
            Assert.AreEqual(0, db.FindTurns(null, null).Count);
            Assert.AreEqual(0, db.FindTurns("Wakka", "Wakka").Count);
        }

        [TestMethod]
        public void TestRawActSave() {
            var db = new CSALDatabase(DB_URL);

            const int ITS = 128;

            for (int i = 0; i < ITS; ++i) {
                db.SaveRawStudentLessonAct(Properties.Resources.SampleRawAct);
            }

            var lessons = db.FindLessons();
            Assert.AreEqual(1, lessons.Count);
            Assert.AreEqual(ITS, lessons[0].TurnCount);
            Assert.AreEqual(SAMPLE_RAW_LESSON, lessons[0].LessonID);
            CollectionAssert.AreEquivalent(new string[] { SAMPLE_RAW_USER }, lessons[0].Students);

            var students = db.FindStudents();
            Assert.AreEqual(1, students.Count);
            Assert.AreEqual(ITS, students[0].TurnCount);
            Assert.AreEqual(SAMPLE_RAW_USER, students[0].UserID);

            //Raw has "simple" user ID, so no class/location information
            var classes = db.FindClasses();
            Assert.AreEqual(0, classes.Count);

            var turns = db.FindTurns(null, null);
            Assert.AreEqual(1, turns.Count);
            Assert.AreEqual(1, db.FindTurns(SAMPLE_RAW_LESSON, null).Count);
            Assert.AreEqual(1, db.FindTurns(null, SAMPLE_RAW_USER).Count);
            Assert.AreEqual(1, db.FindTurns(SAMPLE_RAW_LESSON, SAMPLE_RAW_USER).Count);

            Assert.AreEqual(SAMPLE_RAW_USER, turns[0].UserID);
            Assert.AreEqual(SAMPLE_RAW_LESSON, turns[0].LessonID);
            Assert.AreEqual(ITS, turns[0].Turns.Count);

            //The sample act has no completion action and is turn ID 4 - so no attempts and no completions
            Assert.AreEqual(0, turns[0].Attempts);
            Assert.AreEqual(0, turns[0].Completions);
        }

        [TestMethod]
        public void TestMinimalRawAct() {
            //Note our use of the "extended" user id
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct("{'LessonID': 'lesson', 'UserID': 'memphis-semiotics-fozzy-bear', 'TurnID': 1}");

            var lessons = db.FindLessons();
            Assert.AreEqual(1, lessons.Count);
            Assert.AreEqual(1, lessons[0].TurnCount);
            Assert.AreEqual("lesson", lessons[0].LessonID);
            CollectionAssert.AreEquivalent(new string[] { "fozzy-bear" }, lessons[0].Students);

            var students = db.FindStudents();
            Assert.AreEqual(1, students.Count);
            Assert.AreEqual(1, students[0].TurnCount);
            Assert.AreEqual("fozzy-bear", students[0].UserID);

            var classes = db.FindClasses();
            Assert.AreEqual(1, classes.Count);
            Assert.AreEqual("semiotics", classes[0].ClassID);
            Assert.AreEqual("memphis", classes[0].Location);
            CollectionAssert.AreEquivalent(new string[] { "fozzy-bear" }, classes[0].Students);
            CollectionAssert.AreEquivalent(new string[] { "lesson" }, classes[0].Lessons);

            var turns = db.FindTurns(null, null);
            Assert.AreEqual(1, turns.Count);
            Assert.AreEqual(1, db.FindTurns("lesson", null).Count);
            Assert.AreEqual(1, db.FindTurns(null, "fozzy-bear").Count);
            Assert.AreEqual(1, db.FindTurns("lesson", "fozzy-bear").Count);

            Assert.AreEqual("fozzy-bear", turns[0].UserID);
            Assert.AreEqual("lesson", turns[0].LessonID);
            Assert.AreEqual(1, turns[0].Turns.Count);
            //Turn ID of 1 - we should show one attempt and 0 completions
            Assert.AreEqual(1, turns[0].Attempts);
            Assert.AreEqual(0, turns[0].Completions);
        }

        [TestMethod]
        public void TestAttemptedRawActs() {
            var db = new CSALDatabase(DB_URL);

            var turns = db.FindTurns(null, null);
            Assert.AreEqual(0, turns.Count);

            var attempted = new CSALMongo.TurnModel.ConvLog {
                UserID = "memphis-semiotics-fozzy-bear",
                LessonID = "lesson",
                TurnID = 1
            };

            var completion = new CSALMongo.TurnModel.ConvLog {
                UserID = "memphis-semiotics-fozzy-bear",
                LessonID = "lesson",
                TurnID = 2,
                Transitions = new List<CSALMongo.TurnModel.TransitionLog> {
                    new CSALMongo.TurnModel.TransitionLog { 
                        StateID="TestEnding", 
                        RuleID="TestHint", 
                        Actions=new List<CSALMongo.TurnModel.ActionLog> {
                            new CSALMongo.TurnModel.ActionLog { Agent="System", Act="End", Data="Doesn't Matter"}
                        }
                    }
                }
            };

            db.SaveRawStudentLessonAct(attempted.ToJson());
            turns = db.FindTurns(null, null);
            Assert.AreEqual(1, turns.Count);
            Assert.AreEqual(1, turns[0].Attempts);
            Assert.AreEqual(0, turns[0].Completions);

            db.SaveRawStudentLessonAct(completion.ToJson());
            turns = db.FindTurns(null, null);
            Assert.AreEqual(1, turns.Count);
            Assert.AreEqual(1, turns[0].Attempts);
            Assert.AreEqual(1, turns[0].Completions);

            db.SaveRawStudentLessonAct(attempted.ToJson());
            turns = db.FindTurns(null, null);
            Assert.AreEqual(1, turns.Count);
            Assert.AreEqual(2, turns[0].Attempts);
            Assert.AreEqual(1, turns[0].Completions);

            completion.TurnID++;
            db.SaveRawStudentLessonAct(completion.ToJson());
            turns = db.FindTurns(null, null);
            Assert.AreEqual(1, turns.Count);
            Assert.AreEqual(2, turns[0].Attempts);
            Assert.AreEqual(2, turns[0].Completions);

            completion.TurnID = 1;
            db.SaveRawStudentLessonAct(completion.ToJson());
            turns = db.FindTurns(null, null);
            Assert.AreEqual(1, turns.Count);
            Assert.AreEqual(3, turns[0].Attempts);
            Assert.AreEqual(3, turns[0].Completions);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void TestBadRawActNull() {
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct(null);
            Assert.IsFalse(true); //Should not be here
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void TestBadRawActEmpty() {
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct("");
            Assert.IsFalse(true); //Should not be here
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadRawActMissingUserID() {
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct("{'LessonID': 'lesson', 'UserID': ''}");
            Assert.IsFalse(true); //Should not be here
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadTurnMissingLessonID() {
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct("{'LessonID': '', 'UserID': 'user'}");
            Assert.IsFalse(true); //Should not be here
        }

        [TestMethod]
        public void TestSingleLesson() {
            var db = new CSALDatabase(DB_URL);
            Assert.IsNull(db.FindLesson(""));
            Assert.IsNull(db.FindLesson("key"));

            var lesson = new CSALMongo.Model.Lesson { LessonID = "key", TurnCount = 42, Students = new List<String> { "sa", "sb" } };

            db.SaveLesson(lesson);
            var lesson2 = db.FindLesson("key");
            Assert.IsNotNull(lesson2);

            Assert.AreEqual(GetJSON(lesson), GetJSON(lesson2));
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadSingleLessonSaveNull() {
            var db = new CSALDatabase(DB_URL);
            db.SaveLesson(null);
            Assert.IsFalse(true); //shouldn't be here
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadSingleLessonSaveNoID() {
            var db = new CSALDatabase(DB_URL);
            db.SaveLesson(new CSALMongo.Model.Lesson { LessonID = "", TurnCount = 6 });
            Assert.IsFalse(true); //shouldn't be here
        }

        [TestMethod]
        public void TestSingleStudent() {
            var db = new CSALDatabase(DB_URL);
            Assert.IsNull(db.FindStudent(""));
            Assert.IsNull(db.FindStudent("key"));

            var student = new CSALMongo.Model.Student { UserID = "key", TurnCount = 42 };

            db.SaveStudent(student);
            var student2 = db.FindStudent("key");
            Assert.IsNotNull(student2);

            Assert.AreEqual(GetJSON(student), GetJSON(student2));
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadSingleStudentSaveNull() {
            var db = new CSALDatabase(DB_URL);
            db.SaveStudent(null);
            Assert.IsFalse(true); //shouldn't be here
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadSingleStudentSaveNoID() {
            var db = new CSALDatabase(DB_URL);
            db.SaveStudent(new CSALMongo.Model.Student { UserID = "" });
            Assert.IsFalse(true); //shouldn't be here
        }

        // Classes aren't (currently) involved in raw act saving, so we don't
        // generate any to test findClasses in TestRawActSave.
        [TestMethod]
        public void TestMultipleClasses() {
            var db = new CSALDatabase(DB_URL);

            Assert.AreEqual(0, db.FindClasses().Count);

            //NOTE - three classes in key order
            var madeClasses = new List<CSALMongo.Model.Class>();
            foreach(var key in new string[] {"key1", "key2", "key3"}) {
                madeClasses.Add(new CSALMongo.Model.Class {
                    ClassID = key,
                    Location = "loc" + key,
                    TeacherName = "teach" + key,
                    Students = new List<String> { "a"+key, "b"+key }
                });
                db.SaveClass(madeClasses.Last());
            }
            Assert.AreEqual(3, madeClasses.Count);

            var foundClasses = db.FindClasses().OrderBy(e => e.Id).ToList();
            Assert.AreEqual(madeClasses.Count, foundClasses.Count);

            //Note our hard-coded list size - just making sure we haven't done
            //anything hinky since we created the list
            for (int i = 0; i < 3; ++i) {
                Assert.AreEqual(GetJSON(madeClasses[i]), GetJSON(foundClasses[i]));
            }
        }

        [TestMethod]
        public void TestSingleClass() {
            var db = new CSALDatabase(DB_URL);
            Assert.IsNull(db.FindClass(""));
            Assert.IsNull(db.FindClass("key"));

            var clazz = new CSALMongo.Model.Class { 
                ClassID = "key", Location = "Somewhere", TeacherName = "Teach", 
                Students = new List<String> { "a", "b" } 
            };

            db.SaveClass(clazz);
            var clazz2 = db.FindClass("key");
            Assert.IsNotNull(clazz2);

            Assert.AreEqual(GetJSON(clazz), GetJSON(clazz2));
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadSingleClassSaveNull() {
            var db = new CSALDatabase(DB_URL);
            db.SaveClass(null);
            Assert.IsFalse(true); //shouldn't be here
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadSingleClassSaveNoID() {
            var db = new CSALDatabase(DB_URL);
            db.SaveClass(new CSALMongo.Model.Class{ ClassID = "" });
            Assert.IsFalse(true); //shouldn't be here
        }
    }
}
