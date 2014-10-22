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
        public List<string> ReadingURLs { get; set; }

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

        public bool SequenceCompleted(int start, int end = -1) {
            if (start < 0)
                return false;

            if (end < 0)
                end = Turns.Count - 1;
            if (end < start)
                return false;

            for (int curr = end; end >= start; --end) {
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
            }

            return false;
        }

        //Return true if the last attempt was completed
        public bool LastCompleted() {
            return SequenceCompleted(LastAttemptIndex());
        }

        //Because things can hit the server out of order (or in case of test
        //data, simultaneously), we insure that the DB timestamps are correct
        //by using the Duration field.  Note that this is a hack to approximate
        //time in the event we didn't receive the turns with correct ordering/spacing
        public void FixupTimestamps() {
            for (int i = 1; i < Turns.Count; ++i) {
                var prev = Turns[i - 1];
                var curr = Turns[i];

                double minTime = prev.DBTimestamp + prev.Duration;
                if (curr.DBTimestamp <= minTime) {
                    curr.DBTimestamp = minTime + 200.0; //Add 200 ms for safety
                }
            }
        }

        public double ReadingTime(int start, int end = -1) {
            //Before we do anything, fix up any timestamps that are
            //OBVIOUSLY out of whack
            FixupTimestamps();

            if (start < 0)
                return 0.0;

            if (end < 0)
                end = Turns.Count - 1;
            if (end < start)
                return 0.0;

            double currTime = Turns[start].DBTimestamp;
            double readStart = -1.0;
            double totalRead = 0.0;

            for (int curr = start; curr <= end; ++curr) {
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
                        Debug.WriteLine("Read-End with no matching start - time won't be counted");
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

        //In millisecs
        public double CurrentReadingTime() {
            return ReadingTime(LastAttemptIndex());
        }

        public double TotalTime(int start, int last = -1) {
            //Before we do anything, fix up any timestamps that are
            //OBVIOUSLY out of whack
            FixupTimestamps();

            if (last < 0)
                last = Turns.Count - 1;

            if (start < 0 || start > last) {
                return 0.0;
            }
            else if (start == last) {
                return Turns[start].Duration;
            }

            double startTime = Turns[start].DBTimestamp;
            double endTime = Turns[last].DBTimestamp;

            return (endTime - startTime) + Turns[last].Duration;
        }

        public double CurrentTotalTime() {
            return TotalTime(LastAttemptIndex());
        }

        public string LessonPath(int start, int end = -1) {
            if (start < 0 || start > end)
                return "";

            if (end < 0)
                end = Turns.Count - 1;
            if (end < start)
                return "";

            string lastState = "M";
            string path = "";

            for (int curr = start; curr <= end; ++curr) {
                var turn = Turns[curr];

                foreach (var tran in turn.Transitions) {
                    string ruleID = tran.RuleID;
                    if (String.IsNullOrWhiteSpace(ruleID))
                        continue;

                    ruleID = ruleID.Trim().ToLower();
                    string newState = null;

                    if (ruleID.EndsWith("easy")) newState = "E";
                    else if (ruleID.EndsWith("medium")) newState = "M";
                    else if (ruleID.EndsWith("hard")) newState = "H";

                    if (newState != null && newState != lastState) {
                        path += newState;
                        lastState = newState;
                    }
                }
            }

            return path;
        }

        /// <summary>
        /// Return a string summarizing the student's latest path through the
        /// lesson.  If they stayed in medium the whole time, return empty
        /// string.  If their path was (Start/Medium)=>Hard=>Medium=>Easy return
        /// HME
        /// </summary>
        /// <returns></returns>
        public string CurrentLessonPath() {
            return LessonPath(LastAttemptIndex());
        }

        //Returns rate where 0 <= rate <= 1
        public double CorrectAnswerRate() {
            if (CorrectAnswers < 1)
                return 0.0;

            double tot = (double)(CorrectAnswers + IncorrectAnswers);
            return (double)CorrectAnswers / tot;
        }

        public double AdhocCorrectAnswerRate(int start, int end, double noAnswers = 0.0) {
            if (start < 0)
                return noAnswers;

            if (end < 0)
                end = Turns.Count - 1;
            if (end < start)
                return noAnswers;

            int correct = 0;
            int incorrect = 0;

            for (int i = start; i <= end; ++i) {
                var turn = Turns[i];
                if (turn.Input != null && !String.IsNullOrWhiteSpace(turn.Input.Event)) {
                    string evt = turn.Input.Event.ToLower().Trim();
                    if (evt == "correct") {
                        correct++;
                    }
                    else if (evt.StartsWith("incorrect")) {
                        incorrect++;
                    }
                }
            }

            if (correct < 1) {
                if (incorrect < 1)
                    return noAnswers;
                else
                    return 0.0;
            }
            
            double tot = (double)(correct + incorrect);
            return (double)correct / tot;
        }

        int IComparable<StudentLessonActs>.CompareTo(StudentLessonActs other) {
            int r = Utils.LessonIDSort(LessonID).CompareTo(Utils.LessonIDSort(other.LessonID));
            if (r != 0)
                return r;
            return String.Compare(UserID, other.UserID, true);
        }
    }
}
