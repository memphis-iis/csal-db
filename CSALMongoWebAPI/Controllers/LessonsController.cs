using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CSALMongoWebAPI.Controllers {
    public class LessonsController : ApiController {
        // GET api/lessons
        public IEnumerable<string> Get() {
            return new string[] { "value1", "value2" };
        }

        // GET api/lessons/5
        public string Get(int id) {
            return "value";
        }

        // POST api/lessons/5
        public void Post(int id, [FromBody]string value) {
        }
    }
}
