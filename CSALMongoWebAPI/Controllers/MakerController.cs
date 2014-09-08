using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Diagnostics;

using CSALMongo.Model;

//TODO: remove this silly class

namespace CSALMongoWebAPI.Controllers {
    public class MakerController : Util.CSALBaseController {
        // POST api/maketestdata/42
        public Dictionary<string, string> Post(int id, [FromBody]string value) {
            if (id != 42) {
                throw new Exception("Nice Try!");
            }

            Debug.Print("Received " + value);

            var db = GetDatabase();

            db.saveClass(new Class { ClassID = "c1", Location = "loc1", Students = new List<string> { "s1", "s2", "s3" } });
            db.saveClass(new Class { ClassID = "c2", Location = "loc2", Students = new List<string> { "s1", "s2", "s3" } });
            db.saveClass(new Class { ClassID = "c3", Location = "loc3", Students = new List<string> { "s1", "s2", "s3" } });

            db.saveLesson(new Lesson { LessonID = "l1", Students = new List<string> { "s1", "s2", "s2" } });
            db.saveLesson(new Lesson { LessonID = "l2", Students = new List<string> { "s1", "s2", "s2" } });
            db.saveLesson(new Lesson { LessonID = "l3", Students = new List<string> { "s1", "s2", "s2" } });

            db.saveStudent(new Student { UserID = "s1", Lessons = new List<string> { "l1", "l2", "l3" } });
            db.saveStudent(new Student { UserID = "s2", Lessons = new List<string> { "l1", "l2", "l3" } });
            db.saveStudent(new Student { UserID = "s3", Lessons = new List<string> { "l1", "l2", "l3" } });

            return new Dictionary<string, string> { {"val", "woo hoo"}, {"success", "true"}  };
        }
    }
}
