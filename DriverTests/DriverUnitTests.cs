using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DriverTests
{
    [TestClass]
    public class DriverUnitTests
    {
        /*
         * !!!!! IMPORTANT !!!!!
         * 
         * Have an RS232 port simulator program running (e.g. com0com) and
         * Run the DeviceSimulation project first before testing.
         * 
         */
        private static DeviceDriver.DeviceDriver _deviceDriver;

        [ClassInitialize()]
        public static void CLassInitialize(TestContext context)
        {
            _deviceDriver = new DeviceDriver.DeviceDriver("COM5");
        }

        [TestMethod]
        public void TurnOnTest()
        {
            try
            {
                _deviceDriver.TurnOn();
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Data);
                Console.WriteLine(e.StackTrace);
                Assert.Fail("Error thrown.");
            }
        }

        [TestMethod]
        public void TurnOffTest()
        {
            try
            {
                _deviceDriver.TurnOff();
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Data);
                Console.WriteLine(e.StackTrace);
                Assert.Fail("Error thrown.");
            }
        }

        [TestMethod]
        public void IsMuteTest()
        {
            _deviceDriver.TurnOn();
            Assert.IsTrue(_deviceDriver.IsOn());
            _deviceDriver.TurnOff();
            Assert.IsFalse(_deviceDriver.IsOn());
        }

        [TestMethod]
        public void GetSerialNumberTest()
        {
            // This is the serial number specified in the DeviceSimulation project
            string serialNumberExpected = "ABCD1234";
            Assert.AreEqual(serialNumberExpected, _deviceDriver.GetSerialNumber());
        }
    }
}
