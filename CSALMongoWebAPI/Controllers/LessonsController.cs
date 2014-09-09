using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CSALMongo.Model;

namespace CSALMongoWebAPI.Controllers {
    public class LessonsController : Util.CSALBaseController {
        // GET api/lessons
        public IEnumerable<Lesson> Get() {
            return GetDatabase().FindLessons();
        }

        // GET api/lessons/5
        public Lesson Get(string id) {
            return GetDatabase().FindLesson(id);
        }

        // POST api/lessons/5
        public void Post(string id, [FromBody]string value) {
            //TODO: parse JSON in to instance and call save (and unit test)
        }
    }
}
