using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CSALMongo.Model;

namespace CSALMongoWebAPI.Controllers {
    public class StudentsAtLocationController : Util.CSALBaseController {
        // GET api/students/5
        public List<Student> Get(string id) {
            return DBConn().FindStudentsByLocation(id);
        }
    }
}
