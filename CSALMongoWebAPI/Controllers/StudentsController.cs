using System;
using System.Collections.Generic;
using System.Web.Http;

using Newtonsoft.Json.Linq;

using CSALMongo.Model;

namespace CSALMongoWebAPI.Controllers {
    public class StudentsController : Util.CSALBaseController {
        // GET api/students
        public IEnumerable<Student> Get() {
            return DBConn().FindStudents();
        }

        // GET api/students/5
        public Student Get(string id) {
            id = Util.RenderHelp.URIDecode(id);
            return DBConn().FindStudent(id);
        }

        // POST api/students/5
        public void Post(string id, [FromBody]JToken value) {
            id = Util.RenderHelp.URIDecode(id);
            Student student = Utils.ParseJson<Student>(value.ToString());
            if (student.Id != id) {
                throw new InvalidOperationException("Attempt to save mismatched student");
            }
            DBConn().SaveStudent(student);
        }
    }
}
