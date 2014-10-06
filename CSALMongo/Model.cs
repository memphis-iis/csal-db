using System;
using System.Collections.Generic;

using System.Diagnostics;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;


// This namespace is for model classes used by CSALDatabase.  Note that the
// data for a turn is modeled via TurnModel. ALSO note that turns are sent to
// us via raw JSON.

namespace CSALMongo.Model {
    public static class Utils {
        public static TModel ParseJson<TModel>(string json) {
            var doc = BsonDocument.Parse(json);
            //If they do a GET/modify/POST, then might have a field name $id
            //which is NOT the same as _id - we remove it for them
            if (doc.Contains("$id")) {
                doc.Remove("$id");
            }
            //If they use the .NET-ism of Id instead of _id, we fix that as well
            if (doc.Contains("Id")) {
                doc.Add("_id", doc.GetValue("Id"));
                doc.Remove("Id");
            }
            return BsonSerializer.Deserialize<TModel>(doc);
        }

        public static String LessonIDSort(string lessonID) {
            string ret = lessonID;
            if (String.IsNullOrWhiteSpace(ret))
                return ret;

            ret = ret.Trim().ToLowerInvariant();
            if (!ret.StartsWith("lesson"))
                return ret;

            return ret.Substring(6).PadLeft(8, '0');
        }
    }

    public class Class: IComparable<Class> {
        //MongoDB ID (_id)
        public string Id { get; set; }

        public string ClassID { get { return Id; } set { Id = value; } }
        public string TeacherName { get; set; }
        public string Location { get; set; }
        public string MeetingTime { get; set; }
        public List<string> Students { get; set; }
        public List<string> Lessons { get; set; }
        public Boolean? AutoCreated { get; set; }

        int IComparable<Class>.CompareTo(Class other) {
            return String.Compare(ClassID, other.ClassID, true);
        }
    }

    public class Lesson : IComparable<Lesson> {
        //MongoDB ID
        public string Id { get; set; }

        public string LessonID { get { return Id; } set { Id = value; } }
        public string ShortName { get; set; }
        public DateTime? LastTurnTime { get; set; }
        public int? TurnCount { get; set; }
        public List<String> Students { get; set; }
        public List<DateTime> AttemptTimes { get; set; }
        public List<String> StudentsAttempted { get; set; }
        public List<String> StudentsCompleted { get; set; }
        public List<String> URLs { get; set; }
        public Boolean? AutoCreated { get; set; }

        int IComparable<Lesson>.CompareTo(Lesson other) {
            int r = Utils.LessonIDSort(LessonID).CompareTo(Utils.LessonIDSort(other.LessonID));
            if (r != 0)
                return r;
            return String.Compare(LessonID, other.LessonID, true);
        }
    }

    public class Student : IComparable<Student> {
        //MongoDB ID (_id)
        public string Id { get; set; }
        
        public string UserID { get {return Id;} set {Id = value;} }
        public DateTime? LastTurnTime { get; set; }
        public int? TurnCount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Boolean? AutoCreated { get; set; }

        int IComparable<Student>.CompareTo(Student other) {
            return String.Compare(UserID, other.UserID, true);
        }
    }

    public class StudentLessonActs : IComparable<StudentLessonActs> {
        //MongoDB ID (_id)
        public string Id { get; set; }

        public string LessonID { get; set; }
        public string UserID { get; set; }
        public DateTime? LastTurnTime { get; set; }
        public int TurnCount { get; set; }
        public List<TurnModel.ConvLog> Turns { get; set; }
        public int Attempts { get; set; }
        public int Completions { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }

        //In millisecs
        public double TotalDuration() {
            double tot = 0.0;
            if (Turns != null) {
                foreach (var turn in Turns) {
                    tot += turn.Duration;
                }
            }
            return tot;
        }

        //In millisecs
        public double MeanDuration() {
            if (Turns == null || Turns.Count < 1)
                return 0.0;
            return TotalDuration() / Turns.Count;
        }

        //Index of the beginning of the last attempt
        public int LastAttemptIndex() {
            if (Turns.Count < 1)
                return -1;

            int start = Turns.Count - 1;
            while (start > 0 && Turns[start].TurnID != 0) {
                start--;
            }

            return start;
        }

        //Return true if the last attempt was completed
        public bool LastCompleted() {
            if (Turns.Count < 1)
                return false;

            int curr = Turns.Count - 1;

            while (curr > 0 && Turns[curr].TurnID != 0) {
                foreach (var trans in Turns[curr].Transitions) {
                    foreach (var action in trans.Actions) {
                        string agent = action.Agent;
                        string act = action.Act;

                        if (!String.IsNullOrWhiteSpace(agent) && !String.IsNullOrWhiteSpace(act)) {
                            if (agent.Trim().ToLower() == "system" && act.Trim().ToLower() == "end") {
                                return true;
                            }
                        }
                    }
                }

                curr--;
            }

            return false;
        }

        //In millisecs
        public double CurrentReadingTime() {
            //Before we do anything, fix up any timestamps that are
            //OBVIOUSLY out of whack
            for (int i = 1; i < Turns.Count; ++i) {
                var prev = Turns[i - 1];
                var curr = Turns[i];

                double minTime = prev.DBTimestamp + prev.Duration;
                if (curr.DBTimestamp <= minTime) {
                    curr.DBTimestamp = minTime + 200.0; //Add 200 ms for safety
                }
            }

            int start = LastAttemptIndex();
            if (start < 0)
                return 0.0;

            double currTime = Turns[start].DBTimestamp;
            double readStart = -1.0;
            double totalRead = 0.0;

            for (int curr = start; curr < Turns.Count; ++curr) {
                var turn = Turns[curr];

                //Note that a turn has multiple transitions, so we need to be
                //ready for reading to both start and stop
                bool beginRead = false;
                bool endRead = false;

                foreach (var tran in turn.Transitions) {
                    string ruleID = tran.RuleID;
                    if (String.IsNullOrWhiteSpace(ruleID))
                        continue;

                    ruleID = ruleID.Trim().ToLower();
                    if (ruleID == "read") {
                        beginRead = true;
                    }
                    else if (ruleID.StartsWith("donereading")) {
                        endRead = true;
                    }
                }

                currTime = turn.DBTimestamp;

                //Did they just start reading?
                if (beginRead) {
                    if (readStart >= 0.0) {
                        //We just found a read start with no end - we'll assume as restart
                        totalRead += (currTime - readStart);
                    }
                    readStart = currTime;
                }

                //Advance the clock - note our assumption that reading starts
                //at the beginning of the turn duration and done-reading occurs
                //at the end of the turn duration
                currTime += turn.Duration;

                //Did they just finish reading?
                if (endRead) {
                    if (readStart < 0.0) {
                        //No matching read-start - not much we can do
                        //TODO: log an error? At least add something for unit testing?
                    }
                    else {
                        totalRead += (currTime - readStart);
                    }
                    readStart = -1.0;
                }
            }

            //Found a start-read with no matching end-read - assume it
            if (readStart >= 0.0 && readStart < currTime) {
                totalRead += (currTime - readStart);
            }

            return totalRead;
        }

        /// <summary>
        /// Return a string summarizing the student's latest path through the
        /// lesson.  If they stayed in medium the whole time, return empty
        /// string.  If their path was (Start/Medium)=>Hard=>Medium=>Easy return
        /// HME
        /// </summary>
        /// <returns></returns>
        public string LessonPath() {
            int start = LastAttemptIndex();
            if (start < 0)
                return "";

            string lastState = "M";
            string path = "";

            for (int curr = start; curr < Turns.Count; ++curr) {
                var turn = Turns[curr];

                foreach (var tran in turn.Transitions) {
                    string ruleID = tran.RuleID;
                    if (String.IsNullOrWhiteSpace(ruleID))
                        continue;

                    ruleID = ruleID.Trim().ToLower();
                    string newState = null;

                    if      (ruleID.EndsWith("easy"))   newState = "E";
                    else if (ruleID.EndsWith("medium")) newState = "M";
                    else if (ruleID.EndsWith("hard"))   newState = "H";

                    if (newState != null && newState != lastState) {
                        path += newState;
                        lastState = newState;
                    }
                }
            }

            return path;
        }

        //Returns rate where 0 <= rate <= 1
        public double CorrectAnswerRate() {
            if (CorrectAnswers < 1)
                return 0.0;

            double tot = (double)(CorrectAnswers + IncorrectAnswers);
            return (double)CorrectAnswers / tot;
        }

        int IComparable<StudentLessonActs>.CompareTo(StudentLessonActs other) {
            int r = Utils.LessonIDSort(LessonID).CompareTo(Utils.LessonIDSort(other.LessonID));
            if (r != 0)
                return r;
            return String.Compare(UserID, other.UserID, true);
        }
    }
}
