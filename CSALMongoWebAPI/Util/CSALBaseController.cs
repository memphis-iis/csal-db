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
            return new CSALDatabase(AppSettings["MongoURL"]);
        }
    }
}
