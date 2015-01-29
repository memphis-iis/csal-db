using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestBadStartupEmpty() {
            var db = new CSALDatabase("");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestBadStartupInvalid() {
            var db = new CSALDatabase("sftp://www.google.com/testdb");
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
        public void TestIndexes() {
            var db = new CSALDatabase(DB_URL);
            db.InsureIndexes();
            Assert.IsTrue(true); //If insuring indexes didn't fail, we're OK :)
        }

        [TestMethod]
        public void TestSaveReadingTargetExisting() {
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct(Properties.Resources.SampleRawAct);
            var students = db.FindStudents();
            Assert.AreEqual(1, students.Count);
            Assert.IsNotNull(students[0].ReadingURLs);

            DateTime preWrite = DateTime.Now.AddSeconds(-1);

            db.SaveStudentReadingTarget("{UserID:'" + SAMPLE_RAW_USER + "', TargetURL:'http://test/a'}");
            students = db.FindStudents();
            Assert.AreEqual(1, students.Count);
            var student = students[0];

            Assert.AreEqual(1, student.TurnCount);
            Assert.AreEqual(SAMPLE_RAW_USER.ToLowerInvariant(), student.UserID);

            Assert.AreEqual(1, student.ReadingURLs.Count);
            var visit = student.ReadingURLs[0];

            Assert.AreEqual(visit.TargetURL, "http://test/a");
            Assert.IsTrue(preWrite < visit.VisitTime);
        }

        [TestMethod]
        public void TestSaveReadingTargetMissing() {
            var db = new CSALDatabase(DB_URL);
            var students = db.FindStudents();
            Assert.AreEqual(0, students.Count);

            DateTime preWrite = DateTime.Now.AddSeconds(-1);

            db.SaveStudentReadingTarget("{UserID:'" + SAMPLE_RAW_USER + "', TargetURL:'http://test/a'}");
            students = db.FindStudents();
            Assert.AreEqual(1, students.Count);
            var student = students[0];

            Assert.AreEqual(0, student.TurnCount);
            Assert.AreEqual(SAMPLE_RAW_USER.ToLowerInvariant(), student.UserID);

            Assert.AreEqual(1, student.ReadingURLs.Count);
            var visit = student.ReadingURLs[0];

            Assert.AreEqual(visit.TargetURL, "http://test/a");
            Assert.IsTrue(preWrite < visit.VisitTime);

            db.SaveStudentReadingTarget("{UserID:'" + SAMPLE_RAW_USER + "', TargetURL:'http://test/b'}");
            students = db.FindStudents();
            Assert.AreEqual(1, students.Count);
            student = students[0];

            Assert.AreEqual(0, student.TurnCount);
            Assert.AreEqual(SAMPLE_RAW_USER.ToLowerInvariant(), student.UserID);

            Assert.AreEqual(2, student.ReadingURLs.Count);

            Assert.AreEqual(student.ReadingURLs[0].TargetURL, "http://test/a");
            Assert.AreEqual(student.ReadingURLs[1].TargetURL, "http://test/b");
            
            Assert.IsTrue(preWrite < student.ReadingURLs[0].VisitTime);
            Assert.IsTrue(preWrite < student.ReadingURLs[1].VisitTime);
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
            Assert.AreEqual(SAMPLE_RAW_LESSON.ToLowerInvariant(), lessons[0].LessonID);
            CollectionAssert.AreEquivalent(new string[] { SAMPLE_RAW_USER.ToLowerInvariant() }, lessons[0].Students);

            var students = db.FindStudents();
            Assert.AreEqual(1, students.Count);
            Assert.AreEqual(ITS, students[0].TurnCount);
            Assert.AreEqual(SAMPLE_RAW_USER.ToLowerInvariant(), students[0].UserID);

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
            const int TURN_ID_START = CSALMongo.Model.StudentLessonActs.TURN_ID_START;
            db.SaveRawStudentLessonAct("{'LessonID': 'lesson', 'UserID': 'memphis-semiotics-fozzy-bear', 'TurnID': " + TURN_ID_START.ToString() + "}");

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
            //Turn ID of 0 - we should show one attempt and 0 completions
            Assert.AreEqual(1, turns[0].Attempts);
            Assert.AreEqual(0, turns[0].Completions);
        }

        [TestMethod]
        public void TestAttemptedRawActs() {
            var db = new CSALDatabase(DB_URL);

            var turns = db.FindTurns(null, null);
            Assert.AreEqual(0, turns.Count);

            const int TURN_ID_START = CSALMongo.Model.StudentLessonActs.TURN_ID_START;

            var attempted = new CSALMongo.TurnModel.ConvLog {
                UserID = "memphis-semiotics-fozzy-bear",
                LessonID = "lesson",
                TurnID = TURN_ID_START
            };

            var completion = new CSALMongo.TurnModel.ConvLog {
                UserID = "memphis-semiotics-fozzy-bear",
                LessonID = "lesson",
                TurnID = TURN_ID_START + 1,
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

            completion.TurnID = TURN_ID_START;
            db.SaveRawStudentLessonAct(completion.ToJson());
            turns = db.FindTurns(null, null);
            Assert.AreEqual(1, turns.Count);
            Assert.AreEqual(3, turns[0].Attempts);
            Assert.AreEqual(3, turns[0].Completions);

            //We have saved five turns
            Assert.AreEqual(5, turns[0].Turns.Count);

            //Final test - make sure that we don't break on poorly formed transitions/actions
            //Note that now we need to start checking stuff with raw turn access

            db.SaveRawStudentLessonAct(@"{
                'UserID': 'memphis-semiotics-fozzy-bear',
                'LessonID': 'lesson',
                'TurnID': $TURN_ID_START,
                'Transitions': 42
            }".Replace("$TURN_ID_START", TURN_ID_START.ToString()));
            var rawTurns = db.FindTurnsRaw(null, null);
            Assert.AreEqual(1, rawTurns.Count);
            Assert.AreEqual(4, rawTurns[0].GetValue("Attempts", -1).AsInt32);
            Assert.AreEqual(3, rawTurns[0].GetValue("Completions", -1).AsInt32);
            Assert.AreEqual(6, rawTurns[0].GetValue("Turns").AsBsonArray.Count);

            db.SaveRawStudentLessonAct(@"{
                'UserID': 'memphis-semiotics-fozzy-bear',
                'LessonID': 'lesson',
                'TurnID': 10,
                'Transitions': [42]
            }");
            rawTurns = db.FindTurnsRaw(null, null);
            Assert.AreEqual(1, rawTurns.Count);
            Assert.AreEqual(4, rawTurns[0].GetValue("Attempts", -1).AsInt32);
            Assert.AreEqual(3, rawTurns[0].GetValue("Completions", -1).AsInt32);
            Assert.AreEqual(7, rawTurns[0].GetValue("Turns").AsBsonArray.Count);

            db.SaveRawStudentLessonAct(@"{
                'UserID': 'memphis-semiotics-fozzy-bear',
                'LessonID': 'lesson',
                'TurnID': 12,
                'Transitions': [{'Actions': 42}]
            }");
            rawTurns = db.FindTurnsRaw(null, null);
            Assert.AreEqual(1, rawTurns.Count);
            Assert.AreEqual(4, rawTurns[0].GetValue("Attempts", -1).AsInt32);
            Assert.AreEqual(3, rawTurns[0].GetValue("Completions", -1).AsInt32);
            Assert.AreEqual(8, rawTurns[0].GetValue("Turns").AsBsonArray.Count);

            db.SaveRawStudentLessonAct(@"{
                'UserID': 'memphis-semiotics-fozzy-bear',
                'LessonID': 'lesson',
                'TurnID': 12,
                'Transitions': [{'Actions': [42]}]
            }");
            rawTurns = db.FindTurnsRaw(null, null);
            Assert.AreEqual(1, rawTurns.Count);
            Assert.AreEqual(4, rawTurns[0].GetValue("Attempts", -1).AsInt32);
            Assert.AreEqual(3, rawTurns[0].GetValue("Completions", -1).AsInt32);
            Assert.AreEqual(9, rawTurns[0].GetValue("Turns").AsBsonArray.Count);

            db.SaveRawStudentLessonAct(@"{
                'UserID': 'memphis-semiotics-fozzy-bear',
                'LessonID': 'lesson',
                'TurnID': 13,
                'Transitions': [{'Actions': [{'Agent':'system', 'Act':'end'}]}]
            }");
            rawTurns = db.FindTurnsRaw(null, null);
            Assert.AreEqual(1, rawTurns.Count);
            Assert.AreEqual(4, rawTurns[0].GetValue("Attempts", -1).AsInt32);
            Assert.AreEqual(4, rawTurns[0].GetValue("Completions", -1).AsInt32);
            Assert.AreEqual(10, rawTurns[0].GetValue("Turns").AsBsonArray.Count);

            //Make sure fozzy bear is in memphis
            bool foundFozzy = false;
            foreach (var student in db.FindStudentsByLocation("memphis")) {
                if (student.UserID == "fozzy-bear") {
                    foundFozzy = true;
                    break;
                }
            }
            Assert.IsTrue(foundFozzy);
        }

        [TestMethod]
        public void TestRawActComplexUserDefaultValsForMissing() {
            var db = new CSALDatabase(DB_URL);

            var turns = db.FindTurns(null, null);
            Assert.AreEqual(0, turns.Count);

            const int TURN_ID_START = CSALMongo.Model.StudentLessonActs.TURN_ID_START;

            var attempted = new CSALMongo.TurnModel.ConvLog {
                UserID = "memphis-semiotics-fozzy-bear",
                LessonID = "lesson",
                TurnID = TURN_ID_START
            };

            db.SaveRawStudentLessonAct(attempted.ToJson());
            attempted.TurnID++;
            db.SaveRawStudentLessonAct(attempted.ToJson());

            //Make sure DB looks correct
            Assert.IsNull(db.FindClass("semiotics-miss"));
            Assert.IsNull(db.FindLesson("lesson-miss"));
            Assert.IsNull(db.FindStudent("fozzy-bear-miss"));

            Assert.IsNotNull(db.FindClass("semiotics"));
            Assert.IsNotNull(db.FindLesson("lesson"));
            Assert.IsNotNull(db.FindStudent("fozzy-bear"));

            Assert.AreEqual(1, db.FindTurns(null, null).Count);

            //Class defaults
            var clazz = db.FindClass("semiotics");
            Assert.AreEqual(1, clazz.Lessons.Count);
            Assert.AreEqual(1, clazz.Students.Count);

            //Lesson defaults
            var lesson = db.FindLesson("lesson");
            Assert.AreEqual(1, lesson.AttemptTimes.Count);
            Assert.AreEqual(1, lesson.Students.Count);
            Assert.AreEqual(1, lesson.StudentsAttempted.Count);
            Assert.AreEqual(0, lesson.StudentsCompleted.Count);

            //Student defaults
            var student = db.FindStudent("fozzy-bear");
            Assert.AreEqual(2, student.TurnCount);
        }

        [TestMethod]
        public void TestRawActSimpleUserDefaultValsForMissing() {
            var db = new CSALDatabase(DB_URL);

            Assert.AreEqual(0, db.FindTurns(null, null).Count);

            const int TURN_ID_START = CSALMongo.Model.StudentLessonActs.TURN_ID_START;

            var attempted = new CSALMongo.TurnModel.ConvLog {
                UserID = "fozzy-bear",
                LessonID = "lesson",
                TurnID = TURN_ID_START
            };

            db.SaveRawStudentLessonAct(attempted.ToJson());
            attempted.TurnID++;
            db.SaveRawStudentLessonAct(attempted.ToJson());

            //Make sure DB looks correct
            Assert.AreEqual(0, db.FindClasses().Count);
            Assert.IsNull(db.FindLesson("lesson-miss"));
            Assert.IsNull(db.FindStudent("fozzy-bear-miss"));

            Assert.IsNotNull(db.FindLesson("lesson"));
            Assert.IsNotNull(db.FindStudent("fozzy-bear"));

            Assert.AreEqual(1, db.FindTurns(null, null).Count);

            //Lesson defaults
            var lesson = db.FindLesson("lesson");
            Assert.AreEqual(1, lesson.AttemptTimes.Count);
            Assert.AreEqual(1, lesson.Students.Count);
            Assert.AreEqual(1, lesson.StudentsAttempted.Count);
            Assert.AreEqual(0, lesson.StudentsCompleted.Count);

            //Student defaults
            var student = db.FindStudent("fozzy-bear");
            Assert.AreEqual(2, student.TurnCount);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void TestBadRawActNull() {
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void TestBadRawActEmpty() {
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct("");
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadRawActMissingUserID() {
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct("{'LessonID': 'lesson', 'UserID': ''}");
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadTurnMissingLessonID() {
            var db = new CSALDatabase(DB_URL);
            db.SaveRawStudentLessonAct("{'LessonID': '', 'UserID': 'user'}");
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
        public void TestMultiLessonLookup() {
            var db = new CSALDatabase(DB_URL);
            Assert.IsNull(db.FindLesson("key1"));
            Assert.IsNull(db.FindLesson("key2"));
            Assert.AreEqual(0, db.FindLessonNames().Count);

            var lesson1 = new CSALMongo.Model.Lesson { LessonID = "key1", TurnCount = 42, Students = new List<String> { "sa", "sb" } };
            db.SaveLesson(lesson1);

            var dict = db.FindLessonNames();
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("key1", dict["key1"]);

            var lesson2 = new CSALMongo.Model.Lesson { LessonID = "key2", ShortName = "Name2", TurnCount = 42, Students = new List<String> { "sa", "sb" } };
            db.SaveLesson(lesson2);

            dict = db.FindLessonNames();
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("key1", dict["key1"]);
            Assert.AreEqual("Name2", dict["key2"]);
        }

        [TestMethod]
        public void TestSingleLessonComplexID() {
            
            var db = new CSALDatabase(DB_URL);
            Assert.IsNull(db.FindLesson(""));
            Assert.IsNull(db.FindLesson("key"));

            string simpleId = "lesson25";
            Assert.IsNull(db.FindLesson(simpleId));

            var idList = new List<string> { 
                simpleId, 
                "http://somewhere/good/scripts/Lesson25/activity", 
                "http://elsewhere/bad/scripts/Lesson25/activity/doh"
            };

            const int TURN_ID_START = CSALMongo.Model.StudentLessonActs.TURN_ID_START;

            foreach (var lessonId in idList) {
                var attempted = new CSALMongo.TurnModel.ConvLog {
                    UserID = "memphis-semiotics-fozzy-bear",
                    LessonID = lessonId,
                    TurnID = TURN_ID_START
                };

                db.SaveRawStudentLessonAct(attempted.ToJson());
            }

            Assert.AreEqual(1, db.FindLessons().Count);

            var lesson2 = db.FindLesson(simpleId);
            Assert.IsNotNull(lesson2);

            Assert.AreEqual(GetJSON(lesson2), GetJSON(lesson2));
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadSingleLessonSaveNull() {
            var db = new CSALDatabase(DB_URL);
            db.SaveLesson(null);
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadSingleLessonSaveNoID() {
            var db = new CSALDatabase(DB_URL);
            db.SaveLesson(new CSALMongo.Model.Lesson { LessonID = "", TurnCount = 6 });
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
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadSingleStudentSaveNoID() {
            var db = new CSALDatabase(DB_URL);
            db.SaveStudent(new CSALMongo.Model.Student { UserID = "" });
        }

        [TestMethod]
        public void TestStudentByLocation() {
            var db = new CSALDatabase(DB_URL);

            var students = db.FindStudentsByLocation("no-where");
            Assert.AreEqual(0, students.Count);

            var evenStudents = new List<string>();
            var oddStudents = new List<string>();

            for (int i = 1; i < 2048; ++i) {
                string key = "student" + i.ToString();
                if (i % 2 == 0) evenStudents.Add(key);
                else            oddStudents.Add(key);

                db.SaveStudent(new CSALMongo.Model.Student { UserID = key, FirstName = "Test", LastName = key, TurnCount = 0 });
            }

            db.SaveClass(new CSALMongo.Model.Class { ClassID = "a", Students = new List<string> { }, Location = "no-where" });
            db.SaveClass(new CSALMongo.Model.Class { ClassID = "b", Students = oddStudents, Location = "odd" });
            db.SaveClass(new CSALMongo.Model.Class { ClassID = "c", Students = evenStudents, Location = "even" });

            students = db.FindStudentsByLocation("no-where");
            Assert.AreEqual(0, students.Count);

            students = db.FindStudentsByLocation("odd");
            var foundKeys = (from s in students.AsQueryable() select s.Id).ToList<string>();
            CollectionAssert.AreEquivalent(oddStudents, foundKeys);

            students = db.FindStudentsByLocation("even");
            foundKeys = (from s in students.AsQueryable() select s.Id).ToList<string>();
            CollectionAssert.AreEquivalent(evenStudents, foundKeys);
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
        }

        [TestMethod]
        [ExpectedException(typeof(CSALDatabaseException))]
        public void TestBadSingleClassSaveNoID() {
            var db = new CSALDatabase(DB_URL);
            db.SaveClass(new CSALMongo.Model.Class{ ClassID = "" });
        }
    }
}
