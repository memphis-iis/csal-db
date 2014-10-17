using System;
using System.Collections.Generic;

namespace CSALMongo.TurnModel {
    public class ConvLog {
        //Start-of-epoch to be used for our DBTimestamp
        public const int EPOCH_YR = 2010;

        //MongoDB ID (_id)
        public string Id { get; set; }

        public string LessonID { set; get; }
        public string UserID { set; get; }
        public int TurnID { set; get; }
        public double Duration { set; get; }
        public List<TransitionLog> Transitions { set; get; }
        public InputLog Input { set; get; }
        public List<AssessmentLog> Assessments { set; get; }
        public string ErrorMessage { get; set; }
        public string WarningMessage { get; set; }
        public double DBTimestamp { get; set; }

        public DateTime DBDateTime() {
            return new DateTime(EPOCH_YR, 1, 1).AddMilliseconds(DBTimestamp);
        }

        /// <summary>
        /// The list of all actions in this conv log - and insure every member and property are non-null
        /// </summary>
        /// <returns></returns>
        public List<ActionLog> AllValidActions() {
            var ret = new List<ActionLog>();

            if (Transitions != null && Transitions.Count > 0) {
                foreach (var tran in Transitions) {
                    if (tran.Actions != null && tran.Actions.Count > 0) {
                        foreach (var act in tran.Actions) {
                            if (act.Act == null) act.Act = "";
                            if (act.Agent == null) act.Agent = "";
                            if (act.Data == null) act.Data = "";
                            ret.Add(act);
                        }
                    }
                }
            }

            return ret;
        }
    }

    public class TransitionLog {
        public string StateID { get; set; }
        public string RuleID { get; set; }
        public List<ActionLog> Actions { get; set; }

    }

    public class ActionLog {
        public string Agent { get; set; }
        public string Act { get; set; }
        public string Data { get; set; }
    }

    public class InputLog {
        public string AllText { get; set; }
        public string CurrentText { get; set; }
        public string Event { get; set; }
    }

    public class AssessmentLog {
        public string TargetID { get; set; }
        public string AnswerType { get; set; }
        public double Threshold { get; set; }
        public double RegExMatch { get; set; }
        public double LSAMatch { get; set; }
        public double Match { get; set; }
        public bool Pass { get; set; }
    }
}
