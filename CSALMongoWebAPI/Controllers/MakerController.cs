using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Diagnostics;

using System.Diagnostics.CodeAnalysis;

using CSALMongo.Model;

namespace CSALMongoWebAPI.Controllers {
    [ExcludeFromCodeCoverage]
    public class MakerController : Util.CSALBaseController {
        // POST api/maketestdata/42
        public Dictionary<string, string> Post(int id, [FromBody]string value) {
            if (id != 42) {
                throw new Exception("Nice Try!");
            }

            Debug.Print("Received " + value);

            DateTime now = DateTime.Now;

            const int TURN_ID_START = StudentLessonActs.TURN_ID_START;

            var db = DBConn();

            db.SaveClass(new Class { ClassID = "funky/class!", Location = "loc1", Students = new List<string> { "good/student*", "s2", "s3" }, Lessons = new List<string> { "l1", "l2", "l3" }, TeacherName = "teach" });
            db.SaveClass(new Class { ClassID = "c2", Location = "loc2", Students = new List<string> { "good/student*", "s2", "s3" }, Lessons = new List<string> { "l1", "l2", "l3" }, TeacherName = "teach" });
            db.SaveClass(new Class { ClassID = "c3", Location = "loc3", Students = new List<string> { "good/student*", "s2", "s3" }, Lessons = new List<string> { "l1", "l2", "l3" }, TeacherName = "teach" });

            db.SaveLesson(new Lesson { LessonID = "http://10.0.0.1/lesson/one", Students = new List<string> { "good/student*", "s2", "s2" }, TurnCount = 0, AttemptTimes = new List<DateTime>(), StudentsAttempted = new List<string>(), StudentsCompleted = new List<string>() });
            db.SaveLesson(new Lesson { LessonID = "l2", Students = new List<string> { "good/student*", "s2", "s2" }, TurnCount = 0, AttemptTimes = new List<DateTime>(), StudentsAttempted = new List<string>(), StudentsCompleted = new List<string>() });
            db.SaveLesson(new Lesson { LessonID = "l3", Students = new List<string> { "good/student*", "s2", "s2" }, TurnCount = 0, AttemptTimes = new List<DateTime>(), StudentsAttempted = new List<string>(), StudentsCompleted = new List<string>() });

            db.SaveStudent(new Student { UserID = "good/student*", FirstName = "First", LastName = "Student", TurnCount = 0 });
            db.SaveStudent(new Student { UserID = "s2", FirstName = "Middle", LastName = "Student", TurnCount = 0 });
            db.SaveStudent(new Student { UserID = "s3", FirstName = "Last", LastName = "Student", TurnCount = 0 });

            db.SaveRawStudentLessonAct(SAMPLE_RAW.Replace("$LESSONID$", "http://10.0.0.1/lesson/one").Replace("$USERID$", "good/student*").Replace("$TURNID$", TURN_ID_START.ToString()));
            db.SaveRawStudentLessonAct(SAMPLE_RAW.Replace("$LESSONID$", "l2").Replace("$USERID$", "good/student*").Replace("$TURNID$", TURN_ID_START.ToString()));
            db.SaveRawStudentLessonAct(SAMPLE_RAW.Replace("$LESSONID$", "l2").Replace("$USERID$", "good/student*").Replace("$TURNID$", (TURN_ID_START+1).ToString()));
            db.SaveRawStudentLessonAct(SAMPLE_RAW_COMPLETE.Replace("$LESSONID$", "http://10.0.0.1/lesson/one").Replace("$USERID$", "good/student*").Replace("$TURNID$", (TURN_ID_START+1).ToString()));

            //Turn saved with no matching class, student, or lesson
            db.SaveRawStudentLessonAct(SAMPLE_RAW.Replace("$LESSONID$", "LMISS").Replace("$USERID$", "LOCMISS-CMISS-SMISS").Replace("$TURNID$", TURN_ID_START.ToString()));
            db.SaveRawStudentLessonAct(SAMPLE_RAW.Replace("$LESSONID$", "LMISS").Replace("$USERID$", "LOCMISS-CMISS-SMISS").Replace("$TURNID$", (TURN_ID_START+1).ToString()));

            return new Dictionary<string, string> { { "val", "woo hoo" }, { "success", "true" } };
        }

        private string SAMPLE_RAW = @"{
            'LessonID':'$LESSONID$',
            'UserID':'$USERID$',
            'TurnID':$TURNID$,
            'Duration':2215.2039,
            'Transitions': [
                    {
                    'StateID':'GetTutorHint',
                    'RuleID':'TutorHint',
                    'Actions':[
                        {'Agent':'Tutor','Act':'Speak','Data':'Lets try this together.'},
                        {'Agent':'System','Act':'Display','Data':'Tutor:Lets try this together.'},
                        {'Agent':'Tutor','Act':'Speak','Data':'In which direction is the velocity of the packet , at the point of release?'},
                        {'Agent':'System','Act':'Display','Data':'Tutor:In which direction is the velocity of the packet , at the point of release?'},
                        {'Agent':'System','Act':'WaitForInput','Data':'20'}]
                    },
                {
                'StateID':'GetStudentHint',
                'RuleID':'NoMoreStudentHint',
                'Actions':[
                    {'Agent':'Tutor','Act':'Speak','Data':'Lets try this together.'},
                    {'Agent':'System','Act':'Display','Data':'Tutor:Lets try this together.'},
                    {'Agent':'Tutor','Act':'Speak','Data':'In which direction is the velocity of the packet , at the point of release?'},
                    {'Agent':'System','Act':'Display','Data':'Tutor:In which direction is the velocity of the packet , at the point of release?'},
                    {'Agent':'System','Act':'WaitForInput','Data':'20'}]
                  },
                {
                'StateID':'Hint',
                'RuleID':'TryAnotherHint',
                'Actions':[
                    {'Agent':'Tutor','Act':'Speak','Data':'Lets try this together.'},
                    {'Agent':'System','Act':'Display','Data':'Tutor:Lets try this together.'},
                    {'Agent':'Tutor','Act':'Speak','Data':'In which direction is the velocity of the packet , at the point of release?'},
                    {'Agent':'System','Act':'Display','Data':'Tutor:In which direction is the velocity of the packet , at the point of release?'},
                    {'Agent':'System','Act':'WaitForInput','Data':'20'}]
                   }
                ],
            'Input':{
                'AllText':'I dont know.# So what? # # # # # # # # # I think it is horizontal dircection.#',
                'CurrentText':'I think it is horizontal direction.',
                'Event':''},
            'Assessments':[
                {
                'TargetID':'Expectation 1',
                'AnswerType':'Expectation-Good',
                'Threshold':0,
                'RegExMatch':0,
                'LSAMatch':0,
                'Match':0,
                'Pass':false
                    },
                {
                'TargetID':'Hint 2',
                'AnswerType':'Hint-Bad',
                'Threshold':1,
                'RegExMatch':0,
                'LSAMatch':0,
                'Match':0,
                'Pass':false
                    },
                {
                'TargetID':'Hint 2',
                'AnswerType':'Hint-Good',
                'Threshold':0.75,
                'RegExMatch':0,
                'LSAMatch':0,
                'Match':0,
                'Pass':false
                    },
                {
                'TargetID':'TutoringPack TP1',
                'AnswerType':'Main-Good',
                'Threshold':0.9,
                'RegExMatch':0,
                'LSAMatch':0,
                'Match':0,
                'Pass':false
                    }
                ],
            'ErrorMessage':null,
            'WarningMessage':null
        }";

        private string SAMPLE_RAW_COMPLETE = @"{
            'LessonID':'$LESSONID$',
            'UserID':'$USERID$',
            'TurnID':$TURNID$,
            'Duration':1.0,
            'Transitions': [
                    {
                    'StateID':'GetTutorHint',
                    'RuleID':'TutorHint',
                    'Actions':[
                        {'Agent':'System','Act':'End','Data':''}
                    ]
                    }
                ],
            'Input':{
                'AllText':'I dont know.# So what? # # # # # # # # # I think it is horizontal dircection.#',
                'CurrentText':'I think it is horizontal direction.',
                'Event':''},
            'Assessments':[
                {
                'TargetID':'Expectation 1',
                'AnswerType':'Expectation-Good',
                'Threshold':0,
                'RegExMatch':0,
                'LSAMatch':0,
                'Match':0,
                'Pass':false
                    }
                ],
            'ErrorMessage':null,
            'WarningMessage':null
        }";
    }
}
