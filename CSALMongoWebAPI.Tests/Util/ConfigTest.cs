using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSALMongoWebAPI.Controllers;

namespace CSALMongoWebAPI.Tests.Util {
    [TestClass]
    public class ConfigTest: BaseControllerTest {
        [TestMethod]
        public void TestAppSettingChanges() {
            var controller = new TurnController();
            this.AppSettings.Add(controller.AppSettings);
            Assert.IsFalse(String.IsNullOrEmpty(this.AppSettings["MongoURL"]));
        }
    }
}
