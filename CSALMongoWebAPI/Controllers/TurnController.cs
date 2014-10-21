using System;
using System.Web.Http;

using Newtonsoft.Json.Linq;

using CSALMongo.Model;
using CSALMongo.TurnModel;

namespace CSALMongoWebAPI.Controllers {
    public class TurnController : Util.CSALBaseController {
        // POST api/turn
        public void Post([FromBody]JToken value) {
            DBConn().SaveRawStudentLessonAct(value.ToString());
        }

        // GET lesson/user
        public StudentLessonActs Get(string id, string id2) {
            string lessonID = Util.RenderHelp.URIDecode(id);
            string userID = Util.RenderHelp.URIDecode(id2);

            if (String.IsNullOrWhiteSpace(lessonID) || String.IsNullOrWhiteSpace(userID)) {
                return null;
            }

            var allTurns = DBConn().FindTurns(lessonID, userID);

            if (allTurns == null || allTurns.Count < 1)
                return null;

            return allTurns[0];
        }
    }
}
