using System.Collections.Specialized;

using CSALMongoUnitTest;

namespace CSALMongoWebAPI.Tests.Util {
    //Wrap and extend the CSAL database base test class - mainly we need to
    //apply our own app settings for testing
    public class BaseControllerTest : CSALDatabaseBase {
        public BaseControllerTest() {
            var settings = new NameValueCollection();
            settings.Add("MongoURL", DB_URL);
            AppSettings = settings;
        }

        public NameValueCollection AppSettings { get; set; }
    }
}
