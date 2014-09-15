using System.Web.Http;

namespace CSALMongoWebAPI.Controllers {
    public class TurnController : Util.CSALBaseController {
        // POST api/turn
        public void Post([FromBody]string value) {
            DBConn().SaveRawStudentLessonAct(value);
        }
    }
}
