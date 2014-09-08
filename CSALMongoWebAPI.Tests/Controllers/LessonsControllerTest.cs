using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSALMongoWebAPI;
using CSALMongoWebAPI.Controllers;
using CSALMongo;
using CSALMongo.Model;

namespace CSALMongoWebAPI.Tests.Controllers {
    [TestClass]
    public class LessonsControllerTest : Util.BaseControllerTest {
        [TestMethod]
        public void Get() {
            var controller = new LessonsController();
            controller.AppSettings = this.AppSettings;

            //Initially no lessons
            var noLessons = controller.Get();
            Assert.AreEqual(0, noLessons.Count());

            //Now add some lessons
            var db = new CSALDatabase(DB_URL);
            db.saveLesson(new Lesson { LessonID = "l1", TurnCount = 1 });
            db.saveLesson(new Lesson { LessonID = "l2", TurnCount = 2 });

            var twoLessons = controller.Get().OrderBy(c => c.Id).ToList();
            Assert.AreEqual(2, twoLessons.Count);
            Assert.AreEqual("l1", twoLessons[0].Id);
            Assert.AreEqual("l2", twoLessons[1].Id);
        }

        [TestMethod]
        public void GetById() {
            var controller = new LessonsController();
            controller.AppSettings = this.AppSettings;

            //Initially no classes
            Assert.IsNull(controller.Get("not-there"));

            //Now add some classes
            var db = new CSALDatabase(DB_URL);
            db.saveLesson(new Lesson { LessonID = "l1", TurnCount = 1 });
            db.saveLesson(new Lesson { LessonID = "l2", TurnCount = 2 });

            //Still missing
            Assert.IsNull(controller.Get("not-there"));

            //Find what we inserted
            var oneLesson = controller.Get("l2");
            Assert.AreEqual("l2", oneLesson.Id);
            Assert.AreEqual(2, oneLesson.TurnCount);
        }

        [TestMethod]
        public void PostById() {
            //TODO: test when implemented
        }
    }
}
