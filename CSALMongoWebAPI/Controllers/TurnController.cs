using System.Web.Http;

using Newtonsoft.Json.Linq;

//TODO: document proper posting from python script

namespace CSALMongoWebAPI.Controllers {
    public class TurnController : Util.CSALBaseController {
        // POST api/turn
        public void Post([FromBody]JToken value) {
            DBConn().SaveRawStudentLessonAct(value.ToString());
        }
    }
}
