using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSALMongoWebAPI;
using CSALMongoWebAPI.Controllers;

//TODO: this is all horrible broken

namespace CSALMongoWebAPI.Tests.Controllers {
    [TestClass]
    public class ClassesControllerTest {
        [TestMethod]
        public void Get() {
            // Arrange
            ClassesController controller = new ClassesController();

            // Act
            IEnumerable<string> result = controller.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("value1", result.ElementAt(0));
            Assert.AreEqual("value2", result.ElementAt(1));
        }

        [TestMethod]
        public void GetById() {
            // Arrange
            ClassesController controller = new ClassesController();

            // Act
            string result = controller.Get(5);

            // Assert
            Assert.AreEqual("value", result);
        }

        [TestMethod]
        public void PostById() {
            // Arrange
            ClassesController controller = new ClassesController();

            // Act
            controller.Post(5, "value");

            // Assert
        }
    }
}
