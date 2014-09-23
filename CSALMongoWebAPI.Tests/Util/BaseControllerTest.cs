using System.Collections.Specialized;

using CSALMongoUnitTest;

namespace CSALMongoWebAPI.Tests.Util {
    //Wrap and extend the CSAL database base test class - mainly we need to
    //apply our own app settings for testing
    public class BaseControllerTest : CSALDatabaseBase {
        public BaseControllerTest() {
            var settings = new NameValueCollection();
            
            //Inherit from the testing base for the CSAL db DLL
            settings.Add("MongoURL", DB_URL);
            
            //Just make up the google settings we need
            settings.Add("GoogleClientID", "Google Client ID for Testing");
            settings.Add("GoogleClientSecret", "very secret string");
            
            AppSettings = settings;
        }

        public NameValueCollection AppSettings { get; set; }
    }
}
