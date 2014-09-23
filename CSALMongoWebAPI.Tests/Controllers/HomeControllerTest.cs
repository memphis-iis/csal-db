using System;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSALMongo;
using CSALMongo.Model;
using CSALMongoWebAPI.Controllers;

/****************************************************************************
 * IMPORTANT!!! IMPORTANT!!! IMPORTANT!!!
 * These test are basically only exercising the Home controller to make sure
 * exceptions aren't thrown. While the Web API controllers can be fairly
 * well testing, the Home controller is for the GUI.  Passing tests from this
 * module do ***NOT*** mean that the GUI works.
****************************************************************************/

namespace CSALMongoWebAPI.Tests.Controllers {
    [TestClass]
    public class HomeControllerTest : Util.BaseControllerTest {
        class TestHomeController : HomeController {
            protected override string CurrentUserEmail() {
                return "AlwayLoggedIn@test.com";
            }
        }

        protected HomeController controller;

        [TestInitialize]
        public void LocalSetUp() {
            controller = new TestHomeController();

            //Our unusual pattern here is to insure the HomeController's lazy
            //create fires and that a set operation doesn't break anything

            var cc = controller.ClassesCtrl;
            cc.AppSettings = this.AppSettings;
            controller.ClassesCtrl = cc;

            var lc = controller.LessonsCtrl;
            lc.AppSettings = this.AppSettings;
            controller.LessonsCtrl = lc;

            var sc = controller.StudentsCtrl;
            sc.AppSettings = this.AppSettings;
            controller.StudentsCtrl = sc;
        }

        [TestMethod]
        public void Index() {
            ActionResult act = controller.Index();
            Assert.IsNotNull(act);
        }

        [TestMethod]
        public void Testing() {
            ActionResult act = controller.Testing();
            Assert.IsNotNull(act);
        }

        [TestMethod]
        public void Classes() {
            ActionResult act = controller.Classes();
            Assert.IsNotNull(act);
        }

        [TestMethod]
        public void ClassDetails() {
            ActionResult act = controller.ClassDetails("noclass");
            Assert.AreEqual(act.GetType(), typeof(HttpNotFoundResult));

            var db = new CSALDatabase(DB_URL);
            db.SaveClass(new Class { ClassID = "someclass" });
            act = controller.ClassDetails("someclass");
            Assert.AreNotEqual(act.GetType(), typeof(HttpNotFoundResult));
        }

        [TestMethod]
        public void Lessons() {
            ActionResult act = controller.Lessons();
            Assert.IsNotNull(act);
        }

        [TestMethod]
        public void LessonDetails() {
            ActionResult act = controller.LessonDetails("nolessons");
            Assert.AreEqual(act.GetType(), typeof(HttpNotFoundResult));

            var db = new CSALDatabase(DB_URL);
            db.SaveLesson(new Lesson { LessonID = "somelesson" });
            act = controller.LessonDetails("somelesson");
            Assert.AreNotEqual(act.GetType(), typeof(HttpNotFoundResult));
        }

        [TestMethod]
        public void Students() {
            ActionResult act = controller.Students();
            Assert.IsNotNull(act);
        }

        [TestMethod]
        public void StudentDetails() {
            ActionResult act = controller.StudentDetails("nostudent");
            Assert.AreEqual(act.GetType(), typeof(HttpNotFoundResult));

            var db = new CSALDatabase(DB_URL);
            db.SaveStudent(new Student { UserID = "somestudent" });
            act = controller.StudentDetails("somestudent");
            Assert.AreNotEqual(act.GetType(), typeof(HttpNotFoundResult));
        }
    }
}
