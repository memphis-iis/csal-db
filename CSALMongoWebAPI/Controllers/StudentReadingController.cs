using System;
using System.Collections.Generic;
using System.Web.Http;

using Newtonsoft.Json.Linq;

using CSALMongo.Model;

namespace CSALMongoWebAPI.Controllers {
    public class StudentReadingController : Util.CSALBaseController {
        // GET api/StudentReading/subjectid
        public List<MediaVisit> Get(string id) {
            id = Util.RenderHelp.URIDecode(id);

            var ret = DBConn().FindStudent(id).ReadingURLs;
            if (ret == null) {
                ret = new List<MediaVisit>();
            }

            return ret;
        }

        // POST api/StudentReading
        // Expecting {UserID: 'someone', TargetURL: 'http://somewhere'}
        public void Post([FromBody]JToken value) {
            DBConn().SaveStudentReadingTarget(value.ToString());
        }
    }
}
