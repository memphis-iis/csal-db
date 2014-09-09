using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CSALMongo.Model;

namespace CSALMongoWebAPI.Controllers {
    public class StudentsController : Util.CSALBaseController {
        // GET api/students
        public IEnumerable<Student> Get() {
            return GetDatabase().FindStudents();
        }

        // GET api/students/5
        public Student Get(string id) {
            return GetDatabase().FindStudent(id);
        }

        // POST api/students/5
        public void Post(string id, [FromBody]string value) {
            //TODO: parse JSON in to instance and call save (and unit test)
        }
    }
}
