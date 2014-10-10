﻿using System;
using System.IO;
using System.Text;
using System.Net;
using System.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Mvc;

using System.Diagnostics;

using CSALMongo.Model;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//TODO: on class matrix, spark up the student listing
//TODO: on class matrix, any resp rate should have a tooltip

//TODO: total time on lesson shouldn't use duration

//TODO: document proper posting from python script

//TODO: For any (lesson,student) tuple, we need list of questions with
//      correct, correct 2 tries, incorrect. Use for the Andrew Graph,
//      questions vexing many students, etc, etc.  See above for question
//      location

//TODO: logging of actions, esp login, queries, and errors?

namespace CSALMongoWebAPI.Controllers {
    /// <summary>
    /// Our Home controller is actually where we render the active HTML5 web
    /// GUI.  Although the word "Home" might be poorly chosen, it gives a
    /// nice parity with the web API.  For instance, you would access the
    /// lessons page at /Home/Lessons and the API for a list of lessons in
    /// JSON at /api/lessons.
    /// </summary>
    public class HomeController : Controller {
        //The ignore-case comparison we use
        protected const StringComparison IC = StringComparison.InvariantCultureIgnoreCase;

        protected ClassesController classesCtrl;
        protected LessonsController lessonsCtrl;
        protected StudentsController studentsCtrl;
        protected List<String> adminEmails;

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

        //Handy method for tests to override
        protected virtual string CurrentUserEmail() {
            return Session["UserEmail"] as string;
        }

        protected bool NeedLogin() {
            //We require a user email
            return String.IsNullOrWhiteSpace(CurrentUserEmail());
        }

        protected bool IsAdmin() {
            //There must be a logged in email to be admin
            var email = CurrentUserEmail();
            if (String.IsNullOrWhiteSpace(email)) {
                return false;
            }

            //Lazy admin email get
            if (adminEmails == null) {
                adminEmails = new List<string>();
            }
            if (adminEmails.Count < 1) {
                string allAdmins = ClassesCtrl.AppSettings["AdminEmails"];
                if (!String.IsNullOrWhiteSpace(allAdmins)) {
                    foreach (string one in allAdmins.Split(',')) {
                        adminEmails.Add(one.Trim().ToLower());
                    }
                }
            }

            //Check for match
            email = email.Trim().ToLower();
            foreach(string check in adminEmails) {
                if (check.ToLower() == email)
                    return true;
            }
            return false;
        }

        protected ActionResult LoginRedir() {
            string redir = Request.Url.AbsoluteUri;
            string login = Url.Action("login", "home", null, Request.Url.Scheme);

            var uri = new UriBuilder(login);
            uri.Query = "redir=" + Uri.EscapeDataString(redir);

            return new RedirectResult(uri.Uri.ToString());
        }

        protected ActionResult ErrorView(string errorMsg) {
            //TODO: should really log error message and current URL
            var modelObj = new ExpandoObject();
            var modelDict = (IDictionary<string, object>)modelObj;
            modelDict["ErrorMessage"] = errorMsg;
            return View("Error", modelObj);
        }

        public ActionResult Logout() {
            Session["UserEmail"] = "";
            Session["UserName"] = "";
            Session["IsAdmin"] = false;
            return RedirectToAction("Index");
        }

        public ActionResult Login() {
            //Use the app settings from the classes controller - note that
            //this is an arbitrary choice
            var settings = ClassesCtrl.AppSettings;

            var callback = Url.Action("oauth2callback", "home", null, Request.Url.Scheme);
            var redir = ""; //TODO: get this somehow (and use it in redirects below)

            //Note that a login consists of a correct redirection to 
            var url = new UriBuilder("https://accounts.google.com/o/oauth2/auth");
            
            StringBuilder query = new StringBuilder();
            query.Append("client_id=" + Uri.EscapeDataString(settings["GoogleClientID"]) + "&");
            query.Append("scope=" + Uri.EscapeDataString("https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile") + "&");
            query.Append("redirect_uri=" + Uri.EscapeDataString(callback) + "&");
            query.Append("state=" + Uri.EscapeDataString(redir) + "&");
            query.Append("response_type=code&");
            query.Append("approval_prompt=auto");

            url.Query = query.ToString();

            //Before redirecting, we clear the current session - we just
            //ignore whatever the action wants us to do
            Logout();

            //Now we redirect them to google - which should eventually
            //redirect back to our OAuth2Callback
            return Redirect(url.ToString());
        }

        public ActionResult OAuth2Callback() {
            //Simple wrapper around what we actually to help with readability
            //(otherwise everything will be arbitrarily next an extra level deep)
            try {
                return DoOauth2Callback();
            }
            catch (WebException webEx) {
                try {
                    Debug.Print(new StreamReader(webEx.Response.GetResponseStream()).ReadToEnd());
                }
                catch (Exception) {
                    Debug.Print("Couldn't output web exception response stream");
                }
                return ErrorView(webEx.Message);
            }
            catch (Exception e) {
                return ErrorView(e.Message);
            }
        }

        private ActionResult DoOauth2Callback() {
            //Google could actually send an error
            string error = Request.QueryString.Get("error");
            if (!String.IsNullOrWhiteSpace(error)) {
                return ErrorView("Google Error: " + error);
            }

            //We expect AT LEAST "code" and possibly "state" in the query string
            string code = Request.QueryString.Get("code");
            if (String.IsNullOrWhiteSpace(code)) {
                return ErrorView("Google Login didn't provide a code");
            }

            string redir = Request.QueryString.Get("state");
            if (String.IsNullOrWhiteSpace(redir)) {
                redir = Url.Action("index", "home", null, Request.Url.Scheme);
            }

            //Use the app settings from the classes controller - note that
            //this is an arbitrary choice
            var settings = ClassesCtrl.AppSettings;

            //Use the code passed to us to get an access token
            string accessToken = null;

            using (var client = new WebClient()) {
                //We need the full, absolute URI for this redirect but
                //without the query string
                var redirUri = new UriBuilder(Request.Url.AbsoluteUri);
                redirUri.Query = "";

                var tokenRequestForm = new NameValueCollection();
                tokenRequestForm.Add("code", code);
                tokenRequestForm.Add("client_id", settings["GoogleClientID"]);
                tokenRequestForm.Add("client_secret", settings["GoogleClientSecret"]);
                tokenRequestForm.Add("redirect_uri", redirUri.Uri.ToString());
                tokenRequestForm.Add("grant_type", "authorization_code");

                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                var rawTokenBody = client.UploadValues(
                    new UriBuilder("https://accounts.google.com/o/oauth2/token").Uri,
                    "POST",
                    tokenRequestForm);

                //Parse to get the access token
                var tokenBody = JObject.Parse(Encoding.UTF8.GetString(rawTokenBody));
                accessToken = (string)tokenBody["access_token"];
                if (String.IsNullOrWhiteSpace(accessToken)) {
                    return ErrorView("Couldn't log you in (missing access token response from Google)");
                }
            }

            //Now use the access token to get the user email and user name
            //note that we need to add a header
            string userEmail = null;
            string userName = null;

            using (var client = new WebClient()) {
                var infoRequest = new UriBuilder("https://www.googleapis.com/oauth2/v2/userinfo");
                infoRequest.Query = "access_token=" + Uri.EscapeDataString(accessToken);

                client.Headers.Add("Authorization", "Bearer " + accessToken);
                string rawInfoBody = client.DownloadString(infoRequest.Uri);

                var infoBody = JObject.Parse(rawInfoBody);

                userEmail = (string)infoBody["email"];
                if (String.IsNullOrWhiteSpace(userEmail)) {
                    return ErrorView("No user email specified for login");
                }

                userName = (string)infoBody["name"];
                if (String.IsNullOrWhiteSpace(userName)) {
                    userName = userEmail; //We're ok as long as we have an email
                }
            }

            //Made it! Set up the session and redirect to where we started
            Session["UserEmail"] = userEmail;
            Session["UserName"] = userName;
            Session["IsAdmin"] = IsAdmin();
            return Redirect(redir);
        }

        public ActionResult Index() {
            if (NeedLogin()) {
                return LoginRedir();
            }
            return View("Index");
        }

        //This brings a very silly test page that should be removed sometime after integration
        public ActionResult Testing() {
            if (NeedLogin()) {
                return LoginRedir();
            }
            if (!IsAdmin()) {
                return RedirectToAction("Index");
            }

            return View("Testing");
        }

        public ActionResult Classes() {
            if (NeedLogin()) {
                return LoginRedir();
            }

            var classes = new List<Class>();

            //Only classes we're allowed to see
            bool admin = IsAdmin();
            string email = CurrentUserEmail();
            foreach (Class cls in ClassesCtrl.Get()) {
                if (admin || String.Equals(email, cls.TeacherName, IC)) {
                    classes.Add(cls);
                }
            }

            return View("Classes", classes);
        }

        public ActionResult ClassDetails(string id) {
            if (NeedLogin()) {
                return LoginRedir();
            }
            var clazz = ClassesCtrl.Get(id);
            if (clazz == null) {
                return new HttpNotFoundResult();
            }

            if (!IsAdmin()) {
                if (!String.Equals(CurrentUserEmail(), clazz.TeacherName, IC)) {
                    //Don't have rights to this class
                    return RedirectToAction("Classes");
                }
            }

            //Don't allow null lists
            if (clazz.Lessons == null)
                clazz.Lessons = new List<string>();
            if (clazz.Students == null)
                clazz.Students = new List<string>();

            //SPECIAL: if this is a memphis class with test in the name, we do some filtering
            if (clazz.Location.ToLower().Contains("memphis") && clazz.ClassID.ToLower().Contains("test")) {
                //No carl, test, or non-alpha students then...
                //Sort by len and take top 10
                var nonAlpha = new System.Text.RegularExpressions.Regex("[^A-Z,a-z]+");
                clazz.Students = clazz.Students
                    .Where( s => !(s.Contains("carl") || s.Contains("test") || nonAlpha.IsMatch(s)) )
                    .OrderByDescending(s => s.Length)
                    .Take(10)
                    .ToList();

                //only lessons that match lessonN where N is a number
                var lessonMatch = new System.Text.RegularExpressions.Regex("lesson[0-9]+");
                clazz.Lessons = clazz.Lessons.Where(l => lessonMatch.IsMatch(l)).ToList();
            }

            //Sort info in the class for display purposes
            //Students are easy to sort, but we need a special sort for lessons
            clazz.Lessons = clazz.Lessons.OrderBy(x => Utils.LessonIDSort(x)).ToList();
            clazz.Students.Sort();

            var lessons = new HashSet<String>(clazz.Lessons);
            var students = new HashSet<String>(clazz.Students);

            //Make dictionary of lesson:user 
            var lookup = new Dictionary<Tuple<string, string>, StudentLessonActs>();
            if (lessons.Count > 0 && students.Count > 0) {
                foreach (var turns in LessonsCtrl.DBConn().FindTurns(null, null)) {
                    if (!students.Contains(turns.UserID) || !lessons.Contains(turns.LessonID)) {
                        continue; //Nope
                    }

                    var key = new Tuple<string, string>(turns.LessonID, turns.UserID);
                    lookup[key] = turns;
                }
            }

            //Calculate user and lessons average: first get totals and then
            //calculate the averages. This seemingly strange method means we
            //only need to perform lesson/student nested loop once
            var lessonTots = new Dictionary<string, Tuple<int, int>>();
            var userTots = new Dictionary<string, Tuple<int, int>>();

            //Per-init student dict (since students are the inner loop)
            foreach (string userID in clazz.Students) {
                userTots[userID] = new Tuple<int, int>(0, 0);
            }

            //Find totals
            foreach (string lessonID in clazz.Lessons) {
                lessonTots[lessonID] = new Tuple<int, int>(0, 0);

                foreach (string userID in clazz.Students) {
                    var key = new Tuple<string, string>(lessonID, userID);
                    StudentLessonActs turns;
                    if (lookup.TryGetValue(key, out turns)) {
                        int ca = turns.CorrectAnswers;
                        int ia = turns.IncorrectAnswers;
                        if (ca + ia > 0) {
                            lessonTots[lessonID] = TupleAdd(lessonTots[lessonID], ca, ia);
                            userTots[userID] = TupleAdd(userTots[userID], ca, ia);
                        }
                    }
                }
            }

            //Set up model with all this data
            var modelObj = new ExpandoObject();
            var modelDict = (IDictionary<string, object>)modelObj;
            modelDict["Class"] = clazz;
            modelDict["LUTurns"] = lookup;
            modelDict["LessonNames"] = LessonsCtrl.DBConn().FindLessonNames();
            modelDict["LessonCounts"] = lessonTots;
            modelDict["StudentCounts"] = userTots;

            return View("ClassDetail", modelObj);
        }

        //Simple helper for adding 2-tuples of ints for our 1-pass averaging above
        private Tuple<int, int> TupleAdd(Tuple<int, int> t, int i1, int i2) {
            return new Tuple<int, int>(t.Item1 + i1, t.Item2 + i2);
        }

        private HashSet<string> AllowedLessons(string email) {
            var allowedLessons = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var cls in ClassesCtrl.Get()) {
                if (String.Equals(email, cls.TeacherName, IC)) {
                    allowedLessons.UnionWith(cls.Lessons);
                }
            }
            return allowedLessons;
        }

        public ActionResult Lessons() {
            if (NeedLogin()) {
                return LoginRedir();
            }

            var lessons = new List<Lesson>();

            //Only lessons we're allowed to see
            bool admin = IsAdmin();
            HashSet<string> allowedLessons = null;
            if (!admin) {
                //Only populate allowedLessons if we're not admin
                allowedLessons = AllowedLessons(CurrentUserEmail());
            }

            foreach (Lesson le in LessonsCtrl.Get()) {
                if (admin || allowedLessons.Contains(le.LessonID)) {
                    lessons.Add(le);
                }
            }

            var modelObj = new ExpandoObject();
            var modelDict = (IDictionary<string, object>)modelObj;
            modelDict["Lessons"] = lessons;
            modelDict["AnswerTots"] = LessonsCtrl.DBConn().FindLessonAnswerTots();

            return View("Lessons", modelObj);
        }

        public ActionResult LessonDetails(string id) {
            if (NeedLogin()) {
                return LoginRedir();
            }

            var lesson = LessonsCtrl.Get(id);
            if (lesson == null) {
                return new HttpNotFoundResult();
            }

            if (!IsAdmin()) {
                if (!AllowedLessons(CurrentUserEmail()).Contains(lesson.LessonID)) {
                    return RedirectToAction("Lessons");
                }
            }

            var lessonTurns = LessonsCtrl.DBConn().FindTurns(lesson.LessonID, null);

            var modelObj = new ExpandoObject();
            var modelDict = (IDictionary<string, object>)modelObj;
            modelDict["Lesson"] = lesson;
            modelDict["Turns"] = lessonTurns;
            
            return View("LessonDetail", modelObj);
        }

        private HashSet<string> AllowedStudents(string email) {
            var allowedStudents = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var cls in ClassesCtrl.Get()) {
                if (String.Equals(email, cls.TeacherName, IC)) {
                    allowedStudents.UnionWith(cls.Students);
                }
            }
            return allowedStudents;
        }

        public ActionResult Students() {
            if (NeedLogin()) {
                return LoginRedir();
            }

            var students = new List<Student>();

            //Only lessons we're allowed to see
            bool admin = IsAdmin();
            HashSet<string> allowedStudents = null;
            if (!admin) {
                //Only populate if we're not admin
                allowedStudents = AllowedStudents(CurrentUserEmail());
            }

            foreach (Student std in StudentsCtrl.Get()) {
                if (admin || allowedStudents.Contains(std.UserID)) {
                    students.Add(std);
                }
            }

            return View("Students", students);
        }

        public ActionResult StudentDetails(string id) {
            if (NeedLogin()) {
                return LoginRedir();
            }

            var student = StudentsCtrl.Get(id);
            if (student == null) {
                return new HttpNotFoundResult();
            }

            if (!IsAdmin()) {
                if (!AllowedStudents(CurrentUserEmail()).Contains(student.UserID)) {
                    return RedirectToAction("Students");
                }
            }

            var studentTurns = StudentsCtrl.DBConn().FindTurns(null, student.UserID);

            var modelObj = new ExpandoObject();
            var modelDict = (IDictionary<string, object>)modelObj;
            modelDict["Student"] = student;
            modelDict["Turns"] = studentTurns;
            modelDict["LessonLookup"] = StudentsCtrl.DBConn().FindLessonNames();

            return View("StudentDetail", modelObj);
        }

        public ActionResult Materials() {
            return View("Materials");
        }
    }
}
