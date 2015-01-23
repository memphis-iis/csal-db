using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Configuration;

using CSALMongo;

namespace CSALMongoWebAPI.Util {
    /// <summary>
    /// Common functionality for CSAL Database Web API Controllers
    /// </summary>
    public class CSALBaseController : ApiController {
        protected NameValueCollection appSettings = null;

        protected static bool CONV_CHECK = false;
        protected static object convLock = new object();

        protected static bool INDEX_CHECK = false;
        protected static object indexLock = new object();
        

        /// <summary>
        /// Provide access to the web.config app settings.  Note this wrapper
        /// allows test code to inject its own app settings
        /// </summary>
        public NameValueCollection AppSettings {
            set {
                appSettings = value;
            }
            get {
                //DEFAULT - use the web app
                if (appSettings == null) {
                    appSettings = WebConfigurationManager.AppSettings;
                }
                return appSettings;
            }
        }

        [NonAction]
        public CSALDatabase DBConn() {
            //Note that both our static checks need to do a check-lock-check
            //to be safe

            var conn = new CSALDatabase(AppSettings["MongoURL"]);

            //Check that conventions are set (one-time)
            if (!CONV_CHECK) {
                lock (convLock) {
                    if (!CONV_CHECK) {
                        //note that we set the check *after* to make sure
                        //everyone blocks until it's done
                        conn.HandleConventions();
                        CONV_CHECK = true;
                    }
                }
            }

            //Check that indexes have been handled (one-time)
            if (!INDEX_CHECK) {
                lock (indexLock) {
                    //note that we set INDEX_CHECK *before* doing an insure so
                    //other callers don't block while indexes are getting created
                    if (!INDEX_CHECK) {
                        INDEX_CHECK = true;
                        conn.InsureIndexes();
                    }
                }
            }

            //Finally done
            return conn;
        }
    }
}
