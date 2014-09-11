using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CSALMongo;

namespace CSALMongoWebAPI.Controllers {
    /// <summary>
    /// Our Home controller is actually where we render the active HTML5 web
    /// GUI.  Although the word "Home" might be poorly chosen, it gives a
    /// nice parity with the web API.  For instance, you would access the
    /// lessons page at /Home/Lessons and the API for a list of lessons in
    /// JSON at /api/lessons.
    /// </summary>
    public class HomeController : Controller {
        public ActionResult Index() {
            return View("Index");
        }

        public ActionResult Classes() {
            return View("Classes", new ClassesController().Get());
        }

        public ActionResult ClassDetails(string id) {
            return View("ClassDetail", new ClassesController().Get(id));
        }

        public ActionResult Lessons() {
            return View("Lessons", new LessonsController().Get());
        }

        public ActionResult LessonDetails(string id) {
            var lesson = new CSALMongo.Model.Lesson();
            return View("LessonDetail", new LessonsController().Get(id));
        }

        public ActionResult Students() {
            return View("Students", new StudentsController().Get());
        }

        public ActionResult StudentDetails(string id) {
            var controller = new StudentsController();
            var student = controller.Get(id);

            var studentTurns = controller.DBConn().FindTurns(null, student.UserID);

            var modelObj = new ExpandoObject();
            var modelDict = (IDictionary<string, object>)modelObj;
            modelDict["Student"] = student;
            modelDict["Turns"] = studentTurns;

            return View("StudentDetail", modelObj);
        }
    }
}
