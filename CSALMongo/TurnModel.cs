using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CSALMongo.TurnModel {
    [Serializable]
    public class ConvLog {
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
    }

    [Serializable]
    public class TransitionLog {
        public string StateID { get; set; }
        public string RuleID { get; set; }
        public List<ActionLog> Actions { get; set; }

    }

    [Serializable]
    public class ActionLog {
        public string Agent { get; set; }
        public string Act { get; set; }
        public string Data { get; set; }
    }

    [Serializable]
    public class InputLog {
        public string AllText { get; set; }
        public string CurrentText { get; set; }
        public string Event { get; set; }
    }

    [Serializable]
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
