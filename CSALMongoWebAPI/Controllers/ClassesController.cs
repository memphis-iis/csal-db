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
            return GetDatabase().findClasses();
        }

        // GET api/classes/5
        public Class Get(string id) {
            return GetDatabase().findClass(id);
        }

        // POST api/classes/5
        public void Post(string id, [FromBody]string value) {
            //TODO: parse JSON in to instance and call save
        }
    }
}
