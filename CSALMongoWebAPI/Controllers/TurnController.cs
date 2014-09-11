using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CSALMongo;
using CSALMongo.Model;

namespace CSALMongoWebAPI.Controllers {
    public class TurnController : Util.CSALBaseController {
        // POST api/turn
        public void Post([FromBody]string value) {
            DBConn().SaveRawStudentLessonAct(value);
        }
    }
}
