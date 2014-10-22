using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

//TODO: startup logic - DB InsureIndexes

//TODO: unit tests for correct/incorrect, reading time, path, lesson tots, +coverage

namespace CSALMongo {
    /// <summary>
    /// Exception thrown when CSAL Database-specific exceptions occur
    /// </summary>
    public class CSALDatabaseException : Exception {
        public CSALDatabaseException(string msg): base(msg) {
            //Nothing currently
        }
    }

    /// <summary>
    /// Main interface to the CSAL MongoDB database.  You should instantiate
    /// with a MongoDB URL (e.g. mongodb://localhost:27017/testdb), insert
    /// raw turn data via JSON, and the findXXX methods for querying the
    /// resulting data
    /// </summary>
    public class CSALDatabase {
        private MongoDatabase mongoDatabase;

        //Note that we are currently separating our entities into different
        //collections - which IS considered best practice.  HOWEVER, also
        //note that if performance becomes a problem with the multiple ops
        //in saveRawStudentLessonAct we could move everything into one
        //collection and start using the bulk update API.
        //***IF*** you do that, please remember that you'll need to handle
        //key collisions and document "typing"
        public const string STUDENT_COLLECTION = "students";
        public const string LESSON_COLLECTION = "lessons";
        public const string STUDENT_ACT_COLLECTION = "studentActions";
        public const string CLASS_COLLECTION = "classes";

        /// <summary>
        /// MongoDB URL specifying MongoDB database to target.  Note that you
        /// should specify a database in URL (although it is technically
        /// optional in the MongoDB spec)
        /// </summary>
        public string ServerURL { get; set; }

        /// <summary>
        /// To construct an instance of this class, you supply a valid
        /// MongoDB url that includes a database name
        /// (e.g. mongodb://localhost:27017/testdb)
        /// </summary>
        /// <param name="url">MongoDB URL (including database name)</param>
        public CSALDatabase(string url) {
            this.ServerURL = url;
            
            var mongoUrl = new MongoUrl(url);
            var client = new MongoClient(mongoUrl);
            var server = client.GetServer();
            this.mongoDatabase = server.GetDatabase(mongoUrl.DatabaseName);
        }

        /// <summary>
        /// This very hacky function manually insures all indexes that we want
        /// in the MongoDB collections that we manage.  Should really only be
        /// called once in a blue moon.  Currently called on app startup by our
        /// web api app.
        /// </summary>
        public void InsureIndexes() {
            mongoDatabase.GetCollection(STUDENT_COLLECTION).CreateIndex("UserID");
            
            mongoDatabase.GetCollection(LESSON_COLLECTION).CreateIndex("LessonID");
            mongoDatabase.GetCollection(LESSON_COLLECTION).CreateIndex("Students");
            
            mongoDatabase.GetCollection(CLASS_COLLECTION).CreateIndex("ClassID");
            mongoDatabase.GetCollection(CLASS_COLLECTION).CreateIndex("Location");
            mongoDatabase.GetCollection(CLASS_COLLECTION).CreateIndex("Students");
            mongoDatabase.GetCollection(CLASS_COLLECTION).CreateIndex("Lessons");

            mongoDatabase.GetCollection(STUDENT_ACT_COLLECTION).CreateIndex("LessonID");
            mongoDatabase.GetCollection(STUDENT_ACT_COLLECTION).CreateIndex("UserID");
        }

        /// <summary>
        /// Accept a raw JSON data record describing a single CSAL
        /// student/lesson interaction. The JSON record is expected to be in
        /// the format described in the document "CSAL Data".  Note that in
        /// practice just about any record format will be saved in an effort
        /// to preserve as much data as possible in the event of a system bug.
        /// HOWEVER, the top-level fields LessonID and UserID MUST be present
        /// </summary>
        /// <param name="jsonDataRecord">Proper JSON record formatted as above.</param>
        public void SaveRawStudentLessonAct(string jsonDataRecord) {
            var doc = BsonDocument.Parse(jsonDataRecord);
            
            string fullLessonID = doc.GetValue("LessonID", "").AsString;
            string fullUserID = doc.GetValue("UserID", "").AsString;

            if (String.IsNullOrWhiteSpace(fullLessonID))
                throw new CSALDatabaseException("No lesson ID specified for Student-Lesson Act");
            if (String.IsNullOrWhiteSpace(fullUserID))
                throw new CSALDatabaseException("No user ID specified for Student-Lesson Act");

            //Always lower case for case-insensitive lookup
            fullLessonID = fullLessonID.ToLowerInvariant();
            fullUserID = fullUserID.ToLowerInvariant();

            string lessonID = ExtractLessonID(fullLessonID);

            string lessonURLSeen = null;
            if (lessonID != fullLessonID) {
                lessonURLSeen = fullLessonID;
            }

            string locationID = "";
            string classID = "";
            string userID = fullUserID;

            //Note that if there are at least 2 dashes, we have a "complex"
            //user ID of the form locationid-classid-userid
            string[] userFlds = fullUserID.Split('-');
            if (userFlds.Length > 2) {
                locationID = userFlds[0].Trim();
                classID = userFlds[1].Trim();
                //Note that we could have a dash in the user name
                userID = String.Join("-", userFlds.Skip(2));
            }

            string studentLessonID = userID + ":" + lessonID;
            var now = DateTime.Now;

            //Note that we might receive a DBTimestamp in the post as an
            //override, but if not we just calculate it ourselves
            double dbTimestamp = -1.0;
            BsonValue postedTimestamp;
            if (doc.TryGetValue("DBTimestamp", out postedTimestamp)) {
                if (postedTimestamp.IsDouble)
                    dbTimestamp = postedTimestamp.AsDouble;
            }

            //If we DIDN'T get a timestamp, that xlate now to epoch-based timestamp
            if (dbTimestamp <= 0.0) {
                var epochStart = new DateTime(TurnModel.ConvLog.EPOCH_YR, 1, 1);
                dbTimestamp = (now - epochStart).TotalMilliseconds;
            }

            doc["DBTimestamp"] = dbTimestamp;
            
            dynamic rawInfo = RawContents(doc);
            bool isAttempt = rawInfo.IsAttempt;
            bool isCompletion = rawInfo.IsCompletion;
            int correctAnswers = rawInfo.CorrectAnswers;
            int incorrectAnswers = rawInfo.IncorrectAnswers;

            //Set up our update for the student/lesson act collection.  We
            //will also examine the raw data to see if there is anything we
            //can infer
            var mainUpdate = Update
                .Set("LastTurnTime", now)
                .Set("LessonID", lessonID)
                .Set("UserID", userID)
                .Inc("TurnCount", 1)
                .Push("Turns", doc);

            if (isAttempt) {
                mainUpdate = mainUpdate
                    .Inc("Attempts", 1)
                    .Set("CorrectAnswers", correctAnswers)
                    .Set("IncorrectAnswers", incorrectAnswers);
            }
            else {
                mainUpdate = mainUpdate
                    .Inc("CorrectAnswers", correctAnswers)
                    .Inc("IncorrectAnswers", incorrectAnswers);
            }

            if (isCompletion) {
                mainUpdate = mainUpdate.Inc("Completions", 1);
            }

            //Need to actually save the raw data
            DoUpsert(STUDENT_ACT_COLLECTION, studentLessonID, mainUpdate);

            //Upsert stats on student and lesson - which has the intended
            //side-effect of insuring that they exist
            DoUpsert(STUDENT_COLLECTION, userID, Update
                .Set("LastTurnTime", now)
                .Inc("TurnCount", 1)
                .SetOnInsert("AutoCreated", true));

            //Try and upsert stats on the class - but we don't always get a class ID
            if (!String.IsNullOrWhiteSpace(classID)) {
                DoUpsert(CLASS_COLLECTION, classID, Update
                    .Set("Location", locationID)
                    .AddToSet("Lessons", lessonID)
                    .AddToSet("Students", userID)
                    .SetOnInsert("MeetingTime", "")
                    .SetOnInsert("AutoCreated", true));
            }

            //Note that we make sure to give a default value to lists if we're
            //inserting and don't have any values for them
            var lessonUpdate = Update
                .Set("LastTurnTime", now)
                .AddToSet("Students", userID)
                .Inc("TurnCount", 1)
                .SetOnInsert("AutoCreated", true)
                .SetOnInsert("ShortName", lessonID);

            if (isAttempt) {
                lessonUpdate = lessonUpdate
                    .AddToSet("StudentsAttempted", userID)
                    .Push("AttemptTimes", now);
            }
            else {
                lessonUpdate = lessonUpdate
                    .SetOnInsert("StudentsAttempted", new BsonArray())
                    .SetOnInsert("AttemptTimes", new BsonArray());
            }

            if (isCompletion) {
                lessonUpdate = lessonUpdate.AddToSet("StudentsCompleted", userID);
            }
            else {
                lessonUpdate = lessonUpdate.SetOnInsert("StudentsCompleted", new BsonArray());
            }

            //We keep track of URL's we've seen for lessons
            if (String.IsNullOrEmpty(lessonURLSeen)) {
                lessonUpdate = lessonUpdate.SetOnInsert("URLs", new BsonArray());
            }
            else {
                lessonUpdate = lessonUpdate.AddToSet("URLs", lessonURLSeen);
            }

            DoUpsert(LESSON_COLLECTION, lessonID, lessonUpdate);
        }

        //Given the BSON document, return what we can current figure out from these turns
        protected object RawContents(BsonDocument doc) {
            dynamic ret = new ExpandoObject();

            ret.IsAttempt = (doc.GetValue("TurnID", -1).AsInt32 == 0);
            ret.IsCompletion = false;
            ret.CorrectAnswers = 0;
            ret.IncorrectAnswers = 0;

            //Check event in the input section
            var input = Util.ExtractDoc(doc, "Input");
            if (input.IsBsonDocument) {
                BsonValue evt;
                string eventVal = "";
                if (input.TryGetValue("Event", out evt)) {
                    if (evt.IsString)
                        eventVal = evt.AsString.Trim().ToLower();
                }

                if (eventVal == "correct") {
                    ret.CorrectAnswers = 1;
                }
                else if (eventVal.StartsWith("incorrect")) {
                    ret.IncorrectAnswers = 1;
                }
            }

            //Check transitions for data
            var transitions = Util.ExtractArray(doc, "Transitions");
            foreach (var trans in transitions) {
                if (!trans.IsBsonDocument) {
                    continue;
                }
                var transDoc = trans.AsBsonDocument;

                //Analyze actions to see if they have completed the lesson
                var actions = Util.ExtractArray(transDoc, "Actions");
                foreach (var action in actions) {
                    if (!action.IsBsonDocument) {
                        continue;
                    }
                    var actionDoc = action.AsBsonDocument;
                    string agent = actionDoc.GetValue("Agent", "").AsString.Trim().ToLower();
                    string act = actionDoc.GetValue("Act", "").AsString.Trim().ToLower();

                    if (agent == "system" && act == "end") {
                        ret.IsCompletion = true; //AH-HA!
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Given a "full" lesson ID, attempt to extract a "real" lesson ID.
        /// Note that this is based upon Lesson ID's being transmitted as URL's.
        /// </summary>
        /// <param name="fullLessonID"></param>
        /// <returns></returns>
        protected string ExtractLessonID(string fullLessonID) {
            if (String.IsNullOrWhiteSpace(fullLessonID)) {
                return fullLessonID;
            }

            try {
                //We parse and check the full lesson ID as a URI -if it doesn't
                //meet our criteria then we assume that we can't parse it 
                var formal = new Uri(fullLessonID);

                if (!formal.Scheme.StartsWith("http"))
                    return fullLessonID;

                if (!formal.AbsolutePath.ToLower().Contains("/lesson"))
                    return fullLessonID;

                //We always assume that the Lesson ID isn't the first or last
                //element of the path (thus the skip in the for loop below) -
                //so if there aren't at least 3 ele's this can't be correct.
                string[] pathComponents = formal.AbsolutePath.Split('/');
                if (pathComponents == null || pathComponents.Length < 3)
                    return fullLessonID;

                for (int i = 1; i < pathComponents.Length - 1; ++i) {
                    string one = pathComponents[i];
                    if (one.ToLower().StartsWith("lesson")) {
                        return one;
                    }
                }
            }
            catch (Exception) {
                //Nothing we can do... just continue so that we return the original
            }

            //Wasn't able to find and return a parsed Lesson ID
            return fullLessonID;
        }

        /// <summary>
        /// Return all lessons in DB
        /// </summary>
        /// <returns></returns>
        public List<Model.Lesson> FindLessons() {
            return FindAll<Model.Lesson>(LESSON_COLLECTION);
        }

        /// <summary>
        /// Return a dictionary of Lesson ID => Lesson Short Names - note that
        /// lessons represent human-created content, so this should be a fairly
        /// low-effort method
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, String> FindLessonNames() {
            var ret = new Dictionary<String, String>();

            var collect = mongoDatabase.GetCollection(LESSON_COLLECTION);

            var results = collect.FindAllAs<BsonDocument>()
                .SetFields(Fields.Include("LessonID").Include("ShortName"));

            foreach (var one in results) {
                if (!one.Contains("LessonID")) {
                    continue;
                }
                
                string lid = one["LessonID"].AsString;
                
                BsonValue name;
                if (!one.TryGetValue("ShortName", out name)) {
                    name = lid;
                }

                if (!name.IsString || String.IsNullOrWhiteSpace(name.AsString)) {
                    name = lid;
                }
                
                ret[lid] = name.AsString;
            }

            return ret;
        }

        /// <summary>
        /// Return a dictionary of Lesson ID => (CorrectCount, IncorrectCount)
        /// for all lessons across all students enroller
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, Tuple<int, int>> FindLessonAnswerTots() {
            var collect = mongoDatabase.GetCollection(STUDENT_ACT_COLLECTION);
            var results = collect.FindAllAs<BsonDocument>()
                .SetFields(Fields
                    .Include("LessonID")
                    .Include("CorrectAnswers")
                    .Include("IncorrectAnswers"));

            var ret = new Dictionary<String, Tuple<int, int>>();

            foreach (var one in results) {
                if (!one.Contains("LessonID")) {
                    continue;
                }

                string lid = one["LessonID"].AsString;

                Tuple<int, int> currVals;
                if (!ret.TryGetValue(lid, out currVals)) {
                    currVals = new Tuple<int, int>(0, 0);
                    ret[lid] = currVals;
                }
                
                BsonValue num;
                
                int correct = 0;
                if (one.TryGetValue("CorrectAnswers", out num)) {
                    if (num.IsNumeric) {
                        correct = num.AsInt32;
                    }
                }

                int incorrect = 0;
                if (one.TryGetValue("IncorrectAnswers", out num)) {
                    if (num.IsNumeric) {
                        incorrect = num.AsInt32;
                    }
                }

                ret[lid] = TupleAdd(currVals, correct, incorrect);
            }

            return ret;
        }
        private Tuple<int, int> TupleAdd(Tuple<int, int> t, int i1, int i2) {
            return new Tuple<int, int>(t.Item1 + i1, t.Item2 + i2);
        }

        /// <summary>
        /// Return a single lesson by ID (or null if not found)
        /// </summary>
        /// <param name="lessonID">ID of lessong to locate</param>
        /// <returns>Instance of Model.Lesson or null if not found</returns>
        public Model.Lesson FindLesson(string lessonID) {
            return FindOne<Model.Lesson>(LESSON_COLLECTION, lessonID);
        }

        public void SaveLesson(Model.Lesson lesson) {
            if (lesson == null || String.IsNullOrEmpty(lesson.Id)) {
                throw new CSALDatabaseException("Invalid save request: Missing lesson or lesson ID");
            }
            lesson.Id = lesson.Id.ToLowerInvariant();
            SaveOne<Model.Lesson>(LESSON_COLLECTION, lesson);
        }

        /// <summary>
        /// Return all students in DB
        /// </summary>
        /// <returns></returns>
        public List<Model.Student> FindStudents() {
            return FindAll<Model.Student>(STUDENT_COLLECTION);
        }

        /// <summary>
        /// Return a list of students that are at the given location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public List<Model.Student> FindStudentsByLocation(string location) {
            var studentKeys = new HashSet<string>();

            //Note that we don't use a projection because that is done
            //client-side - since we don't get any server-side speedup
            //we just use the whole instance
            var classesFound = 
                from cls in mongoDatabase.GetCollection(CLASS_COLLECTION).AsQueryable<Model.Class>()
                where cls.Location == location
                select cls;

            foreach (var clazz in classesFound) {
                if (clazz.Students != null) {
                    foreach (string student in clazz.Students) {
                        if (!String.IsNullOrWhiteSpace(student)) {
                            studentKeys.Add(student);
                        }
                    }
                }
            }

            if (studentKeys.Count < 1)
                return new List<Model.Student>();

            var studentsFound =
                from std in mongoDatabase.GetCollection(STUDENT_COLLECTION).AsQueryable<Model.Student>()
                where studentKeys.Contains(std.Id)
                select std;

            //Order after as enum since MongoDB driver doesn't really support orderby
            return studentsFound.AsEnumerable().OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Return a single student by ID (or null if not found)
        /// </summary>
        /// <param name="userID">ID of student to locate</param>
        /// <returns>An instance of Model.Student or null if not found</returns>
        public Model.Student FindStudent(string userID) {
            return FindOne<Model.Student>(STUDENT_COLLECTION, userID);
        }

        public void SaveStudent(Model.Student student) {
            if (student == null || String.IsNullOrEmpty(student.Id)) {
                throw new CSALDatabaseException("Invalid save request: Missing student or User ID");
            }
            student.Id = student.Id.ToLowerInvariant();
            SaveOne<Model.Student>(STUDENT_COLLECTION, student);
        }

        /// <summary>
        /// Return all classes in DB
        /// </summary>
        /// <returns></returns>
        public List<Model.Class> FindClasses() {
            return FindAll<Model.Class>(CLASS_COLLECTION);
        }

        /// <summary>
        /// Return a single class by ID (or null if not found)
        /// </summary>
        /// <param name="classID">ID of class to locate</param>
        /// <returns>An instance of Model.Class or null if not found</returns>
        public Model.Class FindClass(string classID) {
            return FindOne<Model.Class>(CLASS_COLLECTION, classID);
        }

        public void SaveClass(Model.Class clazz) {
            if (clazz == null || String.IsNullOrEmpty(clazz.Id)) {
                throw new CSALDatabaseException("Invalid save request: Missing Class or Class ID");
            }
            clazz.Id = clazz.Id.ToLowerInvariant();
            SaveOne<Model.Class>(CLASS_COLLECTION, clazz);
        }

        /// <summary>
        /// Return all turns matching the given lesson and student.  Note
        /// that a null or empty string result in no filter.
        /// </summary>
        /// <param name="lessonID">Lesson to match. Null or empty string matches nothing (so all lessons)</param>
        /// <param name="userID">Student to match. Null or empty string matches nothing (so all students)</param>
        /// <returns>An unordered list of ConvLog instances representing the turns found</returns>
        /// <example>
        /// var db = new CSALDatabase("mongodb://localhost:27017/testdb");
        /// db.findTurns(null, null); //Returns ALL turns
        /// db.findTurns(null, "Alice"); //Returns turns for student Alice (across all lessons)
        /// db.findTurns("CheckbookBalancing", null); //Returns ALL turns for lesson CheckbookBalance (all students)
        /// db.findTurns("CheckbookBalancing", "Bob"); //Returns ALL turns for student Bob in lesson CheckbookBalance
        /// </example>
        public List<Model.StudentLessonActs> FindTurns(string lessonID, string userID) {
            var found = new List<Model.StudentLessonActs>();

            foreach (var one in FindTurnsRaw(lessonID, userID)) {
                found.Add(BsonSerializer.Deserialize<Model.StudentLessonActs>(one));
            }

            found.Sort();
            return found;
        }

        /// <summary>
        /// Return all StudentLessonActs instances that match any of the students given
        /// </summary>
        /// <param name="studentIDs">Non-null enumerable list of user (student) ID's</param>
        /// <returns>List of StudentLessonActs instances</returns>
        public List<Model.StudentLessonActs> FindTurnsForStudents(IEnumerable<string> studentIDs) {
            var students = new HashSet<string>(studentIDs);
            if (students.Count < 1)
                return new List<Model.StudentLessonActs>();

            var turnsFound =
                from act in mongoDatabase.GetCollection(STUDENT_ACT_COLLECTION).AsQueryable<Model.StudentLessonActs>()
                where students.Contains(act.UserID)
                select act;

            //Order after as enum since MongoDB driver doesn't really support orderby
            return turnsFound.AsEnumerable().OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Really only for dev view - return a tuple for each StudentLessonActs
        /// instance defined as (LessonID, UserID, TurnCount)
        /// </summary>
        /// <returns>List of tuples</returns>
        public List<Tuple<string, string, int>> FindTurnSummary() {
            var collect = mongoDatabase.GetCollection(STUDENT_ACT_COLLECTION);
            var results = collect.FindAllAs<BsonDocument>()
                .SetFields(Fields
                    .Include("LessonID")
                    .Include("UserID")
                    .Include("TurnCount"));

            var ret = new List<Tuple<string, string, int>>();

            foreach(var one in results) {
                BsonValue lessonID;
                BsonValue userID;
                BsonValue turnCount;

                if (!one.TryGetValue("LessonID", out lessonID) || !lessonID.IsString)
                    lessonID = new BsonString("???");

                if (!one.TryGetValue("UserID", out userID) || !userID.IsString)
                    userID = new BsonString("???");

                if (!one.TryGetValue("TurnCount", out turnCount) || !turnCount.IsNumeric)
                    turnCount = new BsonInt32(0);

                ret.Add(new Tuple<string, string, int>(
                    lessonID.AsString, 
                    userID.AsString, 
                    turnCount.AsInt32));
            }

            ret.Sort();
            return ret;
        }

        /// <summary>
        /// Exactly like FindTurns, but returns the raw BsonDocument
        /// representation of the data.  Since we are very liberal in what we
        /// accept in SaveRawStudentActLesson, we might have turn data that
        /// causes exceptions when interpreted with our Model
        /// </summary>
        /// <param name="lessonID">Lesson to match. Null or empty string matches nothing (so all lessons)</param>
        /// <param name="userID">Student to match. Null or empty string matches nothing (so all students)</param>
        /// <returns>An unordered list of BsonDocument instances representing the turns found</returns>
        public List<BsonDocument> FindTurnsRaw(string lessonID, string userID) {
            //Simple if they want everything
            if (String.IsNullOrEmpty(lessonID) && String.IsNullOrEmpty(userID)) {
                return FindAll<BsonDocument>(STUDENT_ACT_COLLECTION);
            }

            var clauses = new List<IMongoQuery>();
            if (!String.IsNullOrEmpty(lessonID)) {
                clauses.Add(Query.EQ("LessonID", lessonID.ToLowerInvariant()));
            }
            if (!String.IsNullOrEmpty(userID)) {
                clauses.Add(Query.EQ("UserID", userID.ToLowerInvariant()));
            }

            var collect = mongoDatabase.GetCollection(STUDENT_ACT_COLLECTION);
            var found = new List<BsonDocument>();

            foreach (var one in collect.FindAs<BsonDocument>(Query.And(clauses))) {
                found.Add(one);
            }

            return found;
        }

        /// <summary>
        /// Upsert for the given doc ID in the given collection using the
        /// specified update.  Note that if ObjectId's are being used for
        /// the key, we expect it to already be transformed to a string
        /// </summary>
        /// <param name="collectionName">Name of MongoDB collection to target (will be created if missing)</param>
        /// <param name="docId">ID (key) of the document to upsert (will be created if missing)</param>
        /// <param name="update">Update actions to perform on specified document</param>
        protected void DoUpsert(string collectionName, string docId, IMongoUpdate update) {
            var collect = mongoDatabase.GetCollection(collectionName);
            collect.Update(Query.EQ("_id", docId), update, UpdateFlags.Upsert);
        }

        /// <summary>
        /// Generic method for finding all documents in a given collection and
        /// returning them using the specified document type
        /// </summary>
        /// <typeparam name="TDocType">Document type to use - see FindAllAs in the Mongo driver for details</typeparam>
        /// <param name="collectionName">Name of the collection to target</param>
        /// <returns>List of TDocType instances (or an empty list if nothing found)</returns>
        protected List<TDocType> FindAll<TDocType>(string collectionName) {
            var collect = mongoDatabase.GetCollection(collectionName);
            var found = new List<TDocType>();

            foreach (var one in collect.FindAllAs<TDocType>()) {
                found.Add(one);
            }

            found.Sort();
            return found;
        }

        /// <summary>
        /// Generic method for finding single document by key (_id) in a given collection
        /// and returning it as the specified doc type
        /// </summary>
        /// <typeparam name="TDocType">Document type to use - see FindOneAs in the Mongo driver for details</typeparam>
        /// <param name="collectionName">Name of collection to target</param>
        /// <param name="docId">Key (_id) of the document - note that this is a string and NOT an ObjectId</param>
        /// <returns>A TDocType instance, or null if nothing is found</returns>
        protected TDocType FindOne<TDocType>(string collectionName, string docId) {
            var collect = mongoDatabase.GetCollection(collectionName);
            if (!String.IsNullOrWhiteSpace(docId))
                docId = docId.ToLowerInvariant();
            return collect.FindOneAs<TDocType>(Query.EQ("_id", docId));
        }

        /// <summary>
        /// Generic method for saving a single entity object to the database
        /// </summary>
        /// <typeparam name="TDocType"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="doc"></param>
        protected void SaveOne<TDocType>(string collectionName, TDocType doc) {
            var collect = mongoDatabase.GetCollection(collectionName);
            collect.Save<TDocType>(doc);
        }
    }
}
