using System.Dynamic;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CSALMongoWebAPI.Controllers {
    /// <summary>
    /// Our Home controller is actually where we render the active HTML5 web
    /// GUI.  Although the word "Home" might be poorly chosen, it gives a
    /// nice parity with the web API.  For instance, you would access the
    /// lessons page at /Home/Lessons and the API for a list of lessons in
    /// JSON at /api/lessons.
    /// </summary>
    public class HomeController : Controller {
        protected ClassesController classesCtrl;
        protected LessonsController lessonsCtrl;
        protected StudentsController studentsCtrl;

        public ClassesController ClassesCtrl {
            set {
                classesCtrl = value;
            }
            get {
                if (classesCtrl == null)
                    classesCtrl = new ClassesController();
                return classesCtrl;
            }
        }

        public LessonsController LessonsCtrl {
            set {
                lessonsCtrl = value;
            }
            get {
                if (lessonsCtrl == null)
                    lessonsCtrl = new LessonsController();
                return lessonsCtrl;
            }
        }

        public StudentsController StudentsCtrl {
            set {
                studentsCtrl = value;
            }
            get {
                if (studentsCtrl == null)
                    studentsCtrl = new StudentsController();
                return studentsCtrl;
            }
        }

        public ActionResult Index() {
            return View("Index");
        }

        //This brings a very silly test page that should be removed sometime after integration
        public ActionResult Testing() {
            return View("Testing");
        }

        public ActionResult Classes() {
            return View("Classes", ClassesCtrl.Get());
        }

        public ActionResult ClassDetails(string id) {
            var clazz = ClassesCtrl.Get(id);
            if (clazz == null) {
                return new HttpNotFoundResult();
            }
            return View("ClassDetail", clazz);
        }

        public ActionResult Lessons() {
            return View("Lessons", LessonsCtrl.Get());
        }

        public ActionResult LessonDetails(string id) {
            var lesson = LessonsCtrl.Get(id);
            if (lesson == null) {
                return new HttpNotFoundResult();
            }

            var lessonTurns = LessonsCtrl.DBConn().FindTurns(lesson.LessonID, null);

            var modelObj = new ExpandoObject();
            var modelDict = (IDictionary<string, object>)modelObj;
            modelDict["Lesson"] = lesson;
            modelDict["Turns"] = lessonTurns;
            
            return View("LessonDetail", modelObj);
        }

        public ActionResult Students() {
            return View("Students", StudentsCtrl.Get());
        }

        public ActionResult StudentDetails(string id) {
            var student = StudentsCtrl.Get(id);
            if (student == null) {
                return new HttpNotFoundResult();
            }

            var studentTurns = StudentsCtrl.DBConn().FindTurns(null, student.UserID);

            var modelObj = new ExpandoObject();
            var modelDict = (IDictionary<string, object>)modelObj;
            modelDict["Student"] = student;
            modelDict["Turns"] = studentTurns;

            return View("StudentDetail", modelObj);
        }
    }
}
