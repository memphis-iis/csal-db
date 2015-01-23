using System;
using System.Collections.Generic;

using MongoDB.Bson;

namespace CSALMongo.TurnModel {
    /// <summary>
    /// This namespace is for model classes as posted to CSALDatabase in JSON format
    /// to record a user turn from ACE.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc {
        //Special class for namespace documentation
    }

    /// <summary>
    /// Top-level instance for turns posted to database - see CSALDatabase and Model for
    /// code interpreting these data structures
    /// </summary>
    public class ConvLog {
        /// <summary>
        /// Start-of-epoch to be used for our DBTimestamp
        /// </summary>
        public const int EPOCH_YR = 2010;

        /// <summary>MongoDB key</summary>
        public string Id { get; set; }

        /// <summary>ID of lesson in question</summary>
        public string LessonID { set; get; }
        /// <summary>User ID (subject ID) of student</summary>
        public string UserID { set; get; }
        /// <summary>Identifier of turn, greater than or equal to 0</summary>
        public int TurnID { set; get; }
        /// <summary>Length of action(s) specified in milliseconds</summary>
        public double Duration { set; get; }
        /// <summary>Transition list</summary>
        public List<TransitionLog> Transitions { set; get; }
        /// <summary>Input specified</summary>
        public InputLog Input { set; get; }
        /// <summary>All assessment data</summary>
        public List<AssessmentLog> Assessments { set; get; }
        /// <summary>Error message for current turn</summary>
        public string ErrorMessage { get; set; }
        /// <summary>Warning message for current turn</summary>
        public string WarningMessage { get; set; }
        /// <summary>Timestamp applied before DB storage - not usually
        /// supplied in the POST'ed JSON.  It is the number of milliseconds
        /// since EPOCH_YR</summary>
        public double DBTimestamp { get; set; }

        /// <summary>
        /// Translation helper DBTimestamp
        /// </summary>
        /// <returns>DBTimestamp interpreted as a DateTime</returns>
        public DateTime DBDateTime() {
            return new DateTime(EPOCH_YR, 1, 1).AddMilliseconds(DBTimestamp);
        }

        /// <summary>
        /// The list of all actions in this conv log - and insure every member and property are non-null
        /// </summary>
        /// <returns>List of all actions identified.  Should never be null</returns>
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

    /// <summary>Transition as specified by state, rule, and a list of action</summary>
    public class TransitionLog {
        /// <summary>State ID</summary>
        public string StateID { get; set; }
        /// <summary>Rule ID</summary>
        public string RuleID { get; set; }
        /// <summary>List of actions</summary>
        public List<ActionLog> Actions { get; set; }

    }

    /// <summary>Single action instance</summary>
    public class ActionLog {
        /// <summary>Agent performing action</summary>
        public string Agent { get; set; }
        /// <summary>What was the action</summary>
        public string Act { get; set; }
        /// <summary>Action-dependent data</summary>
        public string Data { get; set; }
    }

    /// <summary>
    /// Single event instance
    /// </summary>
    public class InputLog {
        /// <summary>All test</summary>
        public string AllText { get; set; }
        /// <summary>Current Text</summary>
        public string CurrentText { get; set; }
        /// <summary>Event</summary>
        public string Event { get; set; }
        /// <summary>Presentation ID</summary>
        public string PresentationID { get; set; }
        /// <summary>Presentation History</summary>
        public string PresentationHistory { get; set; }
    }

    /// <summary>Single instance of assessment</summary>
    public class AssessmentLog {
        /// <summary>Target ID</summary>
        public string TargetID { get; set; }
        /// <summary>Answer Type</summary>
        public string AnswerType { get; set; }
        /// <summary>Threshold</summary>
        public double Threshold { get; set; }
        /// <summary>RegEx match</summary>
        public double RegExMatch { get; set; }
        /// <summary>LSA match</summary>
        public double LSAMatch { get; set; }
        /// <summary>Match</summary>
        public double Match { get; set; }
        /// <summary>Pass?</summary>
        public bool Pass { get; set; }
    }
}
