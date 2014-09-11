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
            return DBConn().FindClasses();
        }

        // GET api/classes/5
        public Class Get(string id) {
            return DBConn().FindClass(id);
        }

        // POST api/classes/5
        public void Post(string id, [FromBody]string value) {
            Class clazz = Utils.ParseJson<Class>(value);
            if (clazz.Id != id) {
                throw new InvalidOperationException("Attempt to save mismatched class");
            }
            DBConn().SaveClass(clazz);
        }
    }
}
