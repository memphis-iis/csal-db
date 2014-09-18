using System.Collections.Generic;

using CSALMongo.Model;

namespace CSALMongoWebAPI.Controllers {
    public class StudentsAtLocationController : Util.CSALBaseController {
        // GET api/students/5
        public List<Student> Get(string id) {
            id = Util.RenderHelp.URIDecode(id);
            return DBConn().FindStudentsByLocation(id);
        }
    }
}
