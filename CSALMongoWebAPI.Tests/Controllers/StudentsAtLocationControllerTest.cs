using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSALMongoWebAPI.Controllers;
using CSALMongo;
using CSALMongo.Model;

namespace CSALMongoWebAPI.Tests.Controllers {
    [TestClass]
    public class StudentsAtLocationControllerTest : Util.BaseControllerTest {
        [TestMethod]
        public void Get() {
            var controller = new StudentsAtLocationController();
            controller.AppSettings = this.AppSettings;

            //Initially no lessons
            Assert.AreEqual(0, controller.Get(null).Count);
            Assert.AreEqual(0, controller.Get("").Count);
            Assert.AreEqual(0, controller.Get("nowhere").Count);

            //Now add some students in a class
            var db = new CSALDatabase(DB_URL);
            db.SaveStudent(new Student { UserID = "u1" });
            db.SaveStudent(new Student { UserID = "u2" });
            db.SaveClass(new Class { ClassID = "c1", Location = "somewhere", Students = new List<string> { "u1", "u2" } });

            Assert.AreEqual(0, controller.Get("nowhere").Count);

            var twoStudents = controller.Get("somewhere").OrderBy(c => c.Id).ToList();
            Assert.AreEqual(2, twoStudents.Count);
            Assert.AreEqual("u1", twoStudents[0].Id);
            Assert.AreEqual("u2", twoStudents[1].Id);
        }
    }
}
