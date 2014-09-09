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
    public class StudentsControllerTest : Util.BaseControllerTest {
        [TestMethod]
        public void Get() {
            var controller = new StudentsController();
            controller.AppSettings = this.AppSettings;

            //Initially no lessons
            var noStudents = controller.Get();
            Assert.AreEqual(0, noStudents.Count());

            //Now add some lessons
            var db = new CSALDatabase(DB_URL);
            db.SaveStudent(new Student { UserID = "u1", Lessons = new List<string> { "a" } });
            db.SaveStudent(new Student { UserID = "u2", Lessons = new List<string> { "b" } });

            var twoStudents = controller.Get().OrderBy(c => c.Id).ToList();
            Assert.AreEqual(2, twoStudents.Count);
            Assert.AreEqual("u1", twoStudents[0].Id);
            Assert.AreEqual("u2", twoStudents[1].Id);
        }

        [TestMethod]
        public void GetById() {
            var controller = new StudentsController();
            controller.AppSettings = this.AppSettings;

            //Initially no classes
            Assert.IsNull(controller.Get("not-there"));

            //Now add some classes
            var db = new CSALDatabase(DB_URL);
            db.SaveStudent(new Student { UserID = "u1", Lessons = new List<string> { "a" } });
            db.SaveStudent(new Student { UserID = "u2", Lessons = new List<string> { "b" } });

            //Still missing
            Assert.IsNull(controller.Get("not-there"));

            //Find what we inserted
            var oneStudent = controller.Get("u2");
            Assert.AreEqual("u2", oneStudent.Id);
            CollectionAssert.AreEquivalent(new List<string> { "b" }, oneStudent.Lessons);
        }

        [TestMethod]
        public void PostById() {
            //TODO: test when implemented
        }
    }
}
