using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSALMongo;
using CSALMongo.Model;
using CSALMongoWebAPI.Controllers;

namespace CSALMongoWebAPI.Tests.Controllers {
    [TestClass]
    public class ClassesControllerTest : Util.BaseControllerTest {
        [TestMethod]
        public void Get() {
            var controller = new ClassesController();
            controller.AppSettings = this.AppSettings;

            //Initially no classes
            var noClasses = controller.Get();
            Assert.AreEqual(0, noClasses.Count());

            //Now add some classes
            var db = new CSALDatabase(DB_URL);
            db.SaveClass(new Class { ClassID = "c1", Location = "l1", TeacherName = "t1", Students = new List<string> { "sa", "sb" } });
            db.SaveClass(new Class { ClassID = "c2", Location = "l2", TeacherName = "t2", Students = new List<string> { "sc", "sd" } });

            var twoClasses = controller.Get().OrderBy(c => c.Id).ToList();
            Assert.AreEqual(2, twoClasses.Count);
            Assert.AreEqual("c1", twoClasses[0].Id);
            Assert.AreEqual("c2", twoClasses[1].Id);
        }

        [TestMethod]
        public void GetById() {
            var controller = new ClassesController();
            controller.AppSettings = this.AppSettings;

            //Initially no classes
            Assert.IsNull(controller.Get("not-there"));

            //Now add some classes
            var db = new CSALDatabase(DB_URL);
            db.SaveClass(new Class { ClassID = "c1", Location = "l1", TeacherName = "t1", Students = new List<string> { "sa", "sb" } });
            db.SaveClass(new Class { ClassID = "c2", Location = "l2", TeacherName = "t2", Students = new List<string> { "sc", "sd" } });

            //Still missing
            Assert.IsNull(controller.Get("not-there"));

            //Find what we inserted
            var oneClass = controller.Get("c1");
            Assert.AreEqual("c1", oneClass.Id);
            Assert.AreEqual("l1", oneClass.Location);
        }

        [TestMethod]
        public void PostById() {
            var controller = new ClassesController();
            controller.AppSettings = this.AppSettings;

            Assert.IsNull(controller.Get("single-id"));

            controller.Post("single-id", @"{
                _id: 'single-id', 
                ClassID: 'single-id', 
                TeacherName: 'teach',
                Location: 'someloc',
                Students: ['s1', 's2'],
                Lessons: ['l1', 'l2']
            }");

            Class clazz = controller.Get("single-id");

            Assert.AreEqual("single-id", clazz.Id);
            Assert.AreEqual("single-id", clazz.ClassID);
            Assert.AreEqual("teach", clazz.TeacherName);
            Assert.AreEqual("someloc", clazz.Location);
            CollectionAssert.AreEquivalent(new string[] { "s1", "s2" }, clazz.Students);
            CollectionAssert.AreEquivalent(new string[] { "l1", "l2" }, clazz.Lessons);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void BadPostById() {
            var controller = new ClassesController();
            controller.AppSettings = this.AppSettings;

            Assert.IsNull(controller.Get("single-id"));

            //Post to the wrong ID for the save
            controller.Post("wrong-id", @"{
                _id: 'single-id', 
                ClassID: 'single-id', 
                TeacherName: 'teach',
                Location: 'someloc',
                Students: ['s1', 's2'],
                Lessons: ['l1', 'l2']
            }");
        }
    }
}
