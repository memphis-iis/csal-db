using System;
using System.Collections.Generic;
using System.Web.Http;

using Newtonsoft.Json.Linq;

using CSALMongo.Model;

namespace CSALMongoWebAPI.Controllers {
    public class ClassesController : Util.CSALBaseController {
        // GET api/classes
        public IEnumerable<Class> Get() {
            return DBConn().FindClasses();
        }

        // GET api/classes/5
        public Class Get(string id) {
            id = Util.RenderHelp.URIDecode(id);
            return DBConn().FindClass(id);
        }

        // POST api/classes/5
        public void Post(string id, [FromBody]JToken value) {
            id = Util.RenderHelp.URIDecode(id).ToLower();
            Class clazz = Utils.ParseJson<Class>(value.ToString());
            if (clazz.Id != id) {
                throw new InvalidOperationException("Attempt to save mismatched class");
            }
            DBConn().SaveClass(clazz);
        }
    }
}
