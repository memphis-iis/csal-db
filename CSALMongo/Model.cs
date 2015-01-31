using System;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace CSALMongo.Model {
    /// <summary>
    /// This namespace is for model classes used by CSALDatabase.  Note that the
    /// data for a turn is modeled via TurnModel. ALSO note that turns are sent to
    /// us via raw JSON.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc {
        //Special class for namespace documentation
    }


    /// <summary>
    /// Utility functions that make working with this model easier
    /// </summary>
    public static class Utils {
        /// <summary>
        /// Given a chunk of JSON, parse it and return the specified type. Note
        /// that this helper utilizes the BsonDocument functionality from the C#
        /// MongoDB driver, NOT the JSON library used by ASP.NET (and our Web API).
        /// Also note that the JSON is tranlated on the fly to handle things like 
        /// the fields $id, Id, and _id.
        /// </summary>
        /// <typeparam name="TModel">The type to return</typeparam>
        /// <param name="json">The JSON to parse</param>
        /// <returns>The populated instance of the type TModel</returns>
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

        /// <summary>
        /// Translation helper that returns a string-sortable version of a
        /// lesson ID
        /// </summary>
        /// <param name="lessonID">Lesson ID to translate</param>
        /// <returns>The string to use for sorting</returns>
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

    /// <summary>
    /// A class of students and lessons
    /// </summary>
    public class Class: IComparable<Class> {
        /// <summary>
        /// The MongoDB ID (_id)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The storage key for the class (note the interaction with Id)
        /// </summary>
        public string ClassID { get { return Id; } set { Id = value; } }
        
        /// <summary>
        /// Name of the teacher for this class. Note that this should be an
        /// email AND that it controls access to the class data in the DB GUI.
        /// Also note that we check that the email is CONTAINED in this string,
        /// so you may use a delimited string of emails
        /// </summary>
        public string TeacherName { get; set; }

        public bool IsATeacher(string toTest) {
            if (String.IsNullOrWhiteSpace(toTest) || String.IsNullOrWhiteSpace(TeacherName)) {
                return false;
            }

            //We know we actually have real strings
            const StringComparison CMP = StringComparison.InvariantCultureIgnoreCase;
            return TeacherName.IndexOf(toTest, CMP) >= 0;
        }

        /// <summary>
        /// Location of the class.  Note that this can be used to get a list
        /// of student names across classes
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// Convenience field mainly for display to teachers
        /// </summary>
        public string MeetingTime { get; set; }

        /// <summary>
        /// List of students enrolled in this class
        /// </summary>
        public List<string> Students { get; set; }
        
        /// <summary>
        /// List of lessons in this class
        /// </summary>
        public List<string> Lessons { get; set; }
        
        /// <summary>
        /// If true, then this class was created automatically as the result
        /// of posted turn data
        /// </summary>
        public Boolean? AutoCreated { get; set; }

        int IComparable<Class>.CompareTo(Class other) {
            return String.Compare(ClassID, other.ClassID, true);
        }
    }

    /// <summary>
    /// A lesson presented to a student in a class
    /// </summary>
    public class Lesson : IComparable<Lesson> {
        /// <summary>
        /// The MongoDB ID (_id)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The storage key for a lesson - note the interaction with Id
        /// </summary>
        public string LessonID { get { return Id; } set { Id = value; } }

        /// <summary>
        /// Name displayed to user in the GUI
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Last time a turn was saved to the database (in StudentLessonActs)
        /// Note that this field might initially be populated with local time, but
        /// will be stored in Mongo and retrieved as UTC
        /// </summary>
        public DateTime? LastTurnTime { get; set; }

        /// <summary>
        /// Number of turns posted to the database
        /// </summary>
        public int? TurnCount { get; set; }

        /// <summary>
        /// List of students (by ID) that have been assigned this lesson
        /// </summary>
        public List<String> Students { get; set; }

        /// <summary>
        /// Each time a student attempts this lesson, the date/time is appended to this list
        /// Note that this field might initially be populated with local time, but
        /// will be stored in Mongo and retrieved as UTC
        /// </summary>
        public List<DateTime> AttemptTimes { get; set; }

        /// <summary>
        /// List of students (by ID) that have attempted this lesson. Size
        /// should always be less than or equal to size of Students
        /// </summary>
        public List<String> StudentsAttempted { get; set; }

        /// <summary>
        /// List of students (by ID) that have completed this lesson. Size
        /// should always be less than or equal to size of Students and StudentsAttempted
        /// </summary>
        public List<String> StudentsCompleted { get; set; }
        
        /// <summary>
        /// List of URL's seen for this lesson's ID.  Because the lesson ID is
        /// sent as a URL and then extracted, there should be at least one URL
        /// in this list if at least one Turn has been posted for the lesson.
        /// </summary>
        public List<String> URLs { get; set; }

        /// <summary>
        /// If true, then this lesson was created automatically as the result
        /// of posted turn data
        /// </summary>
        public Boolean? AutoCreated { get; set; }

        int IComparable<Lesson>.CompareTo(Lesson other) {
            int r = Utils.LessonIDSort(LessonID).CompareTo(Utils.LessonIDSort(other.LessonID));
            if (r != 0)
                return r;
            return String.Compare(LessonID, other.LessonID, true);
        }
    }

    /// <summary>
    /// A student enrolled in a class and assigned lessons
    /// </summary>
    public class Student : IComparable<Student> {
        /// <summary>
        /// The MongoDB ID (_id)
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// The storage key for a student (AKA a subject ID) - note
        /// the interaction with Id
        /// </summary>
        public string UserID { get {return Id;} set {Id = value;} }

        /// <summary>
        /// The time the last turn was posted to the database for this student
        /// Note that this field might initially be populated with local time, but
        /// will be stored in Mongo and retrieved as UTC
        /// </summary>
        public DateTime? LastTurnTime { get; set; }
        
        /// <summary>
        /// Number of turns posted for this student
        /// </summary>
        public int? TurnCount { get; set; }

        /// <summary>
        /// Student's first name - note the GUI only displays subject ID and
        /// in some instances (the student/lesson drill down) actively hides
        /// the first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Student's last name
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// True if the student was created automatically when a turn was posted
        /// </summary>
        public Boolean? AutoCreated { get; set; }

        /// <summary>
        /// List of MediaVisit instances posted to the database to maintain a
        /// reading history for this student
        /// </summary>
        public List<MediaVisit> ReadingURLs { get; set; }

        int IComparable<Student>.CompareTo(Student other) {
            return String.Compare(UserID, other.UserID, true);
        }
    }

    /// <summary>
    /// Represents a visit to a resource - currently only used for Student.ReadingURLs
    /// </summary>
    public class MediaVisit {
        /// <summary>
        /// URL visited
        /// </summary>
        public string TargetURL { get; set; }
        /// <summary>
        /// Time of visit (generally per the DB server time)
        /// Note that this field might initially be populated with local time, but
        /// will be stored in Mongo and retrieved as UTC
        /// </summary>
        public DateTime VisitTime { get; set; }
    }

    /// <summary>
    /// Collection of turns posted for a lesson/student combination
    /// </summary>
    public class StudentLessonActs : IComparable<StudentLessonActs> {
        /// <summary>
        /// The Turn ID signalling the start of an attempt
        /// </summary>
        public const int TURN_ID_START = 1;

        /// <summary>
        /// The MongoDB ID (_id) - composed of Lesson ID and User ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Lesson ID of turns posted
        /// </summary>
        public string LessonID { get; set; }

        /// <summary>
        /// User ID for turns posted
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// Date/time of last turn posted
        /// Note that this field might initially be populated with local time, but
        /// will be stored in Mongo and retrieved as UTC
        /// </summary>
        public DateTime? LastTurnTime { get; set; }

        /// <summary>
        /// Number of turns posted
        /// </summary>
        public int TurnCount { get; set; }

        /// <summary>
        /// Actual list of turns posted (see the CSALMongo.TurnModel
        /// namespace for details)
        /// </summary>
        public List<TurnModel.ConvLog> Turns { get; set; }
        
        /// <summary>
        /// Number of attempts (lesson starts)
        /// </summary>
        public int Attempts { get; set; }

        /// <summary>
        /// Number of lesson completions
        /// </summary>
        public int Completions { get; set; }

        /// <summary>
        /// Number of correct answers for last (most recent) lesson attempt
        /// </summary>
        public int CorrectAnswers { get; set; }

        /// <summary>
        /// Number of incorrect answers for last (most recent) lesson attempt
        /// </summary>
        public int IncorrectAnswers { get; set; }

        /// <summary>
        /// Index of the beginning of the last attempt
        /// </summary>
        /// <returns>Index that is the beginning of the last attempt at the lesson. -1 if this index doesn't exist</returns>
        public int LastAttemptIndex() {
            if (Turns.Count < 1)
                return -1;

            int start = Turns.Count - 1;
            while (start > 0 && Turns[start].TurnID != TURN_ID_START) {
                start--;
            }

            return start;
        }

        /// <summary>
        /// Return true if the lesson described by turns between indexes start
        /// and end is completed.
        /// </summary>
        /// <param name="start">Index where checking begins</param>
        /// <param name="end">Last index checked - if less than 0, then index is assumed to be last in list</param>
        /// <returns>True of the lesson specified has been completed</returns>
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

        /// <summary>
        /// Return true if last lesson (as identified by LastAttemptIndex)
        /// has been completed (as identified by SequenceCompleted)
        /// </summary>
        /// <returns>True if last attempt was completed</returns>
        public bool LastCompleted() {
            return SequenceCompleted(LastAttemptIndex());
        }
        
        /// <summary>
        /// Because things can hit the server out of order (or in case of test
        /// data, simultaneously), we insure that the DB timestamps are correct
        /// by using the Duration field.  Note that this is a hack to approximate
        /// time in the event we didn't receive the turns with correct ordering/spacing
        /// </summary>
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

        /// <summary>
        /// Return time spent reading (in ms) during lesson described by Turns stored
        /// from index start to index end
        /// </summary>
        /// <param name="start">Index where checking begins</param>
        /// <param name="end">Last index checked - if less than 0, then index is assumed to be last in list</param>
        /// <returns>Reading time in milliseconds</returns>
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

        /// <summary>
        /// Total time spent reading (as calculated by ReadingTime) on last
        /// lesson (as identified by LastAttemptIndex) 
        /// </summary>
        /// <returns>Time spent reading in milliseconds</returns>
        public double CurrentReadingTime() {
            return ReadingTime(LastAttemptIndex());
        }

        /// <summary>
        /// Return total time spent (in ms) on the lesson described by Turns stored
        /// from index start to index end
        /// </summary>
        /// <param name="start">Index where checking begins</param>
        /// <param name="last">Last index checked - if less than 0, then index is assumed to be last in list</param>
        /// <returns>Total time in milliseconds</returns>
        public double TotalTime(int start, int last = -1) {
            //Before we do anything, fix up any timestamps that are
            //OBVIOUSLY out of whack
            FixupTimestamps();

            //We IGNORE all turns with ID 0
            var turnsToCheck = Turns;
            if (Turns.Count > 0) {
                turnsToCheck = turnsToCheck.Where(x => x.TurnID != 0).ToList();
            }

            if (last < 0)
                last = turnsToCheck.Count - 1;

            if (start < 0 || start > last) {
                return 0.0;
            }
            else if (start == last) {
                return turnsToCheck[start].Duration;
            }

            double startTime = turnsToCheck[start].DBTimestamp;
            double endTime = turnsToCheck[last].DBTimestamp;

            double totalTime = (endTime - startTime) + turnsToCheck[last].Duration;

            //With two or more turns, we might have multiple attempts. As a result,
            //we need to subtract the elapsed time between the end of one attempt
            //and the beginning of the next
            for (int i = start + 1; i <= last; ++i) {
                var prev = turnsToCheck[i - 1];
                var curr = turnsToCheck[i];

                if (curr.TurnID == TURN_ID_START) {
                    var elap = curr.DBTimestamp - prev.DBTimestamp;
                    if (elap > 0.0) {
                        totalTime -= elap;
                    }
                    //If the previous turn had a duration, we should use that
                    //as part of the time 
                    totalTime += prev.Duration;
                }
            }
            
            return totalTime;
        }

        /// <summary>
        /// Total time spent on lesson (as calculated by TotalTime) on last
        /// lesson (as identified by LastAttemptIndex) 
        /// </summary>
        /// <returns>Time spent in milliseconds</returns>
        public double CurrentTotalTime() {
            return TotalTime(LastAttemptIndex());
        }

        /// <summary>
        /// String representation of the path taken by the lesson attempt
        /// recorded by Turns from index start to index end
        /// </summary>
        /// <param name="start">Index where checking begins</param>
        /// <param name="end">Last index checked - if less than 0, then index
        /// is assumed to be last in list</param>
        /// <returns>A string representing the lesson path.  Each character in
        /// the string is a path change: E=Easy, M=Medium, and H=Hard.  All
        /// lessons are assumed to be begin in M</returns>
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
        /// <returns>Path as specified</returns>
        public string CurrentLessonPath() {
            return LessonPath(LastAttemptIndex());
        }

        /// <summary>
        /// Return the correct answer rate (from 0.0 to 1.0 inclusive) for the
        /// last lesson
        /// </summary>
        /// <returns>correct answer rate (from 0.0 to 1.0 inclusive)</returns>
        public double CorrectAnswerRate() {
            if (CorrectAnswers < 1)
                return 0.0;

            double tot = (double)(CorrectAnswers + IncorrectAnswers);
            return (double)CorrectAnswers / tot;
        }

        /// <summary>
        ///  Return the correct answer rate (from 0.0 to 1.0 inclusive) for the
        /// lesson recorded by the Turns from index start to index end
        /// </summary>
        /// <param name="start">Index where checking begins</param>
        /// <param name="end">Last index checked - if less than 0, then index
        /// is assumed to be last in list</param>
        /// <param name="noAnswers">The value to return if there are no answers
        /// for the given indexes</param>
        /// <returns>The correct answer rate (0.0 to 1.0 inclusive) for the turns
        /// from index start to index end.  If no answers are found in the given
        /// range, then noAnswers is returned</returns>
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
