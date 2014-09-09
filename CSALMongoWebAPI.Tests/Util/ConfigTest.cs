using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSALMongoWebAPI;
using CSALMongoWebAPI.Controllers;
using CSALMongo;
using CSALMongo.Model;

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
