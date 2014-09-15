using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            db.SaveStudent(new Student { UserID = "u1" });
            db.SaveStudent(new Student { UserID = "u2" });

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
            db.SaveStudent(new Student { UserID = "u1" });
            db.SaveStudent(new Student { UserID = "u2" });

            //Still missing
            Assert.IsNull(controller.Get("not-there"));

            //Find what we inserted
            var oneStudent = controller.Get("u2");
            Assert.AreEqual("u2", oneStudent.Id);
        }

        [TestMethod]
        public void PostById() {
            var controller = new StudentsController();
            controller.AppSettings = this.AppSettings;

            Assert.IsNull(controller.Get("single-id"));

            controller.Post("single-id", @"{
                _id: 'single-id', 
                UserID: 'single-id', 
                LastTurnTime: ISODate('2012-05-02T13:07:17.000Z'), 
                TurnCount: 42, 
                FirstName: 'Fozzy',
                LastName: 'Bear'
            }");

            Student student = controller.Get("single-id");

            Assert.AreEqual("single-id", student.Id);
            Assert.AreEqual("single-id", student.UserID);
            Assert.AreEqual(new DateTime(2012, 5, 2, 13, 7, 17), student.LastTurnTime);
            Assert.AreEqual(42, student.TurnCount);
            Assert.AreEqual("Fozzy", student.FirstName);
            Assert.AreEqual("Bear", student.LastName);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void BadPostById() {
            var controller = new StudentsController();
            controller.AppSettings = this.AppSettings;

            Assert.IsNull(controller.Get("single-id"));

            //Try to save to wrong ID
            controller.Post("wrong-id", @"{
                _id: 'single-id', 
                UserID: 'single-id', 
                LastTurnTime: ISODate('2012-05-02T13:07:17.000Z'), 
                TurnCount: 42, 
                FirstName: 'Fozzy',
                LastName: 'Bear'
            }");
        }
    }
}
