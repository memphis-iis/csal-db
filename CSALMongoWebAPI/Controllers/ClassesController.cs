using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CSALMongo;
using CSALMongo.Model;

namespace CSALMongoWebAPI.Controllers {
    public class ClassesController : Util.CSALBaseController {
        // GET api/classes
        public IEnumerable<Class> Get() {
            return null; //TODO
        }

        // GET api/classes/5
        public string Get(int id) {
            return "value";
        }

        // POST api/classes/5
        public void Post(int id, [FromBody]string value) {
        }
    }
}
