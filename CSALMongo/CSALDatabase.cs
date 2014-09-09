using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;

//TODO: first turn on a lesson changes attempts from 0 to 1
//TODO: when see act triple of system/end/*, we know lesson is completed
//TODO: test new user ID with class/location

//TODO: how count attempts for a student on a lesson?
//TODO: calc time on lesson
//TODO: calc reading time
//TODO: correct items (and total or incorrect items)

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
            
            string lessonID = doc.GetValue("LessonID", "").AsString;
            string fullUserID = doc.GetValue("UserID", "").AsString;

            if (String.IsNullOrWhiteSpace(lessonID))
                throw new CSALDatabaseException("No lesson ID specified for Student-Lesson Act");
            if (String.IsNullOrWhiteSpace(fullUserID))
                throw new CSALDatabaseException("No lesson ID specified for Student-Lesson Act");

            string locationID = "";
            string classID = "";
            string userID = fullUserID;

            //Note that if there are at least 2 dashes, we have a "complex"
            //user ID of the form locationid-classid-userid
            string[] userFlds = fullUserID.Split('-');
            if (userFlds.Length >= 2) {
                locationID = userFlds[0].Trim();
                classID = userFlds[1].Trim();
                //Note that we could have a dash in the user name
                userID = String.Join("-", userFlds.Skip(2));
            }

            string studentLessonID = userID + ":" + lessonID;
            var now = DateTime.Now;
            
            //Need to actually save the raw data
            DoUpsert(STUDENT_ACT_COLLECTION, studentLessonID, Update
                .Set("LastTurnTime", now)
                .Set("LessonID", lessonID)
                .Set("UserID", userID)
                .Inc("TurnCount", 1)
                .Push("Turns", doc));

            //Upsert stats on student and lesson - which has the intended
            //side-effect of insuring that they exist
            DoUpsert(STUDENT_COLLECTION, userID, Update
                .Set("LastTurnTime", now)
                .Inc("TurnCount", 1));
            
            DoUpsert(LESSON_COLLECTION, lessonID, Update
                .Set("LastTurnTime", now)
                .AddToSet("Students", userID)
                .Inc("TurnCount", 1));
            
            //Try and upsert stats on the class - but we don't always get a class ID
            if (!String.IsNullOrWhiteSpace(classID)) {
                DoUpsert(CLASS_COLLECTION, classID, Update
                    .Set("Location", locationID)
                    .AddToSet("Lessons", lessonID)
                    .AddToSet("Students", userID));
            }
            
        }

        /// <summary>
        /// Return all lessons in DB
        /// </summary>
        /// <returns></returns>
        public List<Model.Lesson> FindLessons() {
            return FindAll<Model.Lesson>(LESSON_COLLECTION);
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
            //Simple if they want everything
            if (String.IsNullOrEmpty(lessonID) && String.IsNullOrEmpty(userID)) {
                return FindAll<Model.StudentLessonActs>(STUDENT_ACT_COLLECTION);
            }

            var clauses = new List<IMongoQuery>();
            if (!String.IsNullOrEmpty(lessonID)) {
                clauses.Add(Query.EQ("LessonID", lessonID));
            }
            if (!String.IsNullOrEmpty(userID)) {
                clauses.Add(Query.EQ("UserID", userID));
            }

            var collect = mongoDatabase.GetCollection(STUDENT_ACT_COLLECTION);
            var found = new List<Model.StudentLessonActs>();

            foreach (var one in collect.FindAs<Model.StudentLessonActs>(Query.And(clauses))) {
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

            foreach (var student in collect.FindAllAs<TDocType>()) {
                found.Add(student);
            }

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
