using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Newtonsoft.Json.Linq;

using CSALMongo.Model;
using CSALMongo.TurnModel;

namespace CSALMongoWebAPI.Controllers {
    public class TurnRollupController : Util.CSALBaseController {
        public class RollupItem {
            public static RollupItem CreateItem(StudentLessonActs parent, int start, int end) {
                var turns = parent.Turns;
                var b = turns[start];
                var e = turns[end];

                string answerRate = "";
                double answerRateAct = parent.AdhocCorrectAnswerRate(start, end, double.NaN);
                if (!double.IsNaN(answerRateAct)) {
                    answerRate = answerRateAct.ToString();
                }

                return new RollupItem { 
                    StartTime = b.DBDateTime().ToString(),
                    EndTime = e.DBDateTime().AddMilliseconds(e.Duration).ToString(),
                    CorrectAnswerRate = answerRate,
                    LessonPath = parent.LessonPath(start, end),
                    ReadingTime = Util.RenderHelp.HumanDuration(parent.ReadingTime(start, end)),
                    TotalTime = Util.RenderHelp.HumanDuration(parent.TotalTime(start, end)),
                    Completed = parent.SequenceCompleted(start, end)
                };
            }

            public string StartTime  { get; set; }
            public string EndTime  { get; set; }
            public string CorrectAnswerRate { get; set; }
            public string LessonPath { get; set; }
            public string ReadingTime { get; set; }
            public string TotalTime { get; set; }

            public bool Completed { get; set; }
        }

        // GET lesson/user
        public List<RollupItem> Get(string id, string id2) {
            string lessonID = Util.RenderHelp.URIDecode(id);
            string userID = Util.RenderHelp.URIDecode(id2);

            if (String.IsNullOrWhiteSpace(lessonID) || String.IsNullOrWhiteSpace(userID)) {
                return null;
            }

            var allTurns = DBConn().FindTurns(lessonID, userID);

            if (allTurns == null || allTurns.Count < 1)
                return null;

            var parent = allTurns[0];
            var turns = parent.Turns;
            if (turns == null || turns.Count < 1)
                return null;

            //We just skip Turn ID 0 for our rollup
            turns = turns.Where(x => x.TurnID != 0).ToList();
            if (turns.Count < 1)
                return null;

            if (turns.Count == 1) {
                return new List<RollupItem> { RollupItem.CreateItem(parent, 0, 0) };
            }

            //So now we know we have at least 2 turns (that aren't ID 0)
            
            var ret = new List<RollupItem>();
            
            //Start after the first item if the list begins with TID of 0 -
            //this makes the loop below much simpler
            int start = 0;
            int list_start = 0;
            if (turns[0].TurnID == StudentLessonActs.TURN_ID_START)
                list_start = 1;

            for (int i = list_start; i < turns.Count; ++i) {
                if (turns[i].TurnID == StudentLessonActs.TURN_ID_START) {
                    ret.Add(RollupItem.CreateItem(parent, start, i - 1));
                    start = i;
                }
            }

            //Add the final rollup item
            ret.Add(RollupItem.CreateItem(parent, start, turns.Count - 1));

            return ret;
        }
    }
}
