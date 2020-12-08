using Microsoft.VisualStudio.TestTools.UnitTesting;
using com_port_to_database;
using System;

namespace com_port_to_database_unit_tests
{
    [TestClass]
    public class ComPortSpec : ServiceSpec
    {
        // Create a static instance of configuration for serial ports
        private static Attributes.PortConfig[] config = new Attributes.PortConfig[1];
        private static byte len = 0; // The initial length 
        private static int status = 0xFF; // The initial status

        [TestMethod]
        public void ComPortTestSqlData()
        {
            // This class inherits methods of ServiceSpec class 
            this.ConfigTestDefaultValue();

            /* The static method returns
                    serial ports configuration length.  */
            len = SqlData.QueryPortsConfig(ref config);

            // Check that the length is equal to 1 
            Assert.AreEqual(len, 1);

            // Check the serail port name 
            Assert.AreEqual(config[len - 1].portName, "COM1");
        }
        [TestMethod]
        public void ComPortTestOpen()
        {
            ComPortTestSqlData();

            // Create an instance of ComPort class 
            ComPort cp = new ComPort(config[len - 1]);

            // Check the initialization is true 
            Assert.IsTrue(ComPort._run);

            // Check the status - the port already open 
            Assert.AreEqual(cp.Open(), 0x0B);
        }
    }
}
