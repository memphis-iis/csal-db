using System;
using System.Collections.Generic;
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
        protected CSALDatabase GetDatabase() {
            return new CSALDatabase(WebConfigurationManager.AppSettings["MongoURL"]);
        }
    }
}
