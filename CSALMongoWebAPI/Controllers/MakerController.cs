using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Diagnostics;

using CSALMongo.Model;

//TODO: remove this silly class, the Testing action in Home, and the Testing view

namespace CSALMongoWebAPI.Controllers {
    public class MakerController : Util.CSALBaseController {
        // POST api/maketestdata/42
        public Dictionary<string, string> Post(int id, [FromBody]string value) {
            if (id != 42) {
                throw new Exception("Nice Try!");
            }

            Debug.Print("Received " + value);

            DateTime now = DateTime.Now;

            var db = DBConn();

            db.SaveClass(new Class { ClassID = "c1", Location = "loc1", Students = new List<string> { "s1", "s2", "s3" }, Lessons = new List<string> { "l1", "l2", "l3" }, TeacherName = "teach" });
            db.SaveClass(new Class { ClassID = "c2", Location = "loc2", Students = new List<string> { "s1", "s2", "s3" }, Lessons = new List<string> { "l1", "l2", "l3" }, TeacherName = "teach" });
            db.SaveClass(new Class { ClassID = "c3", Location = "loc3", Students = new List<string> { "s1", "s2", "s3" }, Lessons = new List<string> { "l1", "l2", "l3" }, TeacherName = "teach" });

            db.SaveLesson(new Lesson { LessonID = "l1", Students = new List<string> { "s1", "s2", "s2" }, TurnCount = 0, AttemptTimes = new List<DateTime>(), StudentsAttempted =new List<string>(), StudentsCompleted = new List<string>() });
            db.SaveLesson(new Lesson { LessonID = "l2", Students = new List<string> { "s1", "s2", "s2" }, TurnCount = 0, AttemptTimes = new List<DateTime>(), StudentsAttempted = new List<string>(), StudentsCompleted = new List<string>() });
            db.SaveLesson(new Lesson { LessonID = "l3", Students = new List<string> { "s1", "s2", "s2" }, TurnCount = 0, AttemptTimes = new List<DateTime>(), StudentsAttempted = new List<string>(), StudentsCompleted = new List<string>() });

            db.SaveStudent(new Student { UserID = "s1", FirstName = "First", LastName = "Student", TurnCount = 0 });
            db.SaveStudent(new Student { UserID = "s2", FirstName = "Middle", LastName = "Student", TurnCount = 0 });
            db.SaveStudent(new Student { UserID = "s3", FirstName = "Last", LastName = "Student", TurnCount = 0 });

            db.SaveRawStudentLessonAct(SAMPLE_RAW.Replace("$LESSONID$", "l1").Replace("$USERID$", "s1").Replace("$TURNID$", "1"));
            db.SaveRawStudentLessonAct(SAMPLE_RAW.Replace("$LESSONID$", "l2").Replace("$USERID$", "s1").Replace("$TURNID$", "1"));
            db.SaveRawStudentLessonAct(SAMPLE_RAW.Replace("$LESSONID$", "l2").Replace("$USERID$", "s1").Replace("$TURNID$", "2"));
            db.SaveRawStudentLessonAct(SAMPLE_RAW_COMPLETE.Replace("$LESSONID$", "l1").Replace("$USERID$", "s1").Replace("$TURNID$", "3"));

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
