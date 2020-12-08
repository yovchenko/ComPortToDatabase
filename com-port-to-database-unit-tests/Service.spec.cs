using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Odbc;
using System.Configuration;
using com_port_to_database;

namespace com_port_to_database_unit_tests
{
    [TestClass]
    public class ServiceSpec : Service
    {
        private static int status = 0xFF; // the initial status

        private static string connectionString;
        [TestInitialize()]
        public void Startup()
        {
            // Run the method before each test in the assembly
            this.OnStart();
            // ODBC-connection string
            connectionString = ConfigurationManager
                .ConnectionStrings["Com_Port"].ConnectionString;
            // Remove all rows in SQL tables with the serial port config    
            this.ClearTables();
        }
        [TestMethod]
        public void ConfigTestEmptyTables()
        {
            // Read status of the serial port configuration 
            status = Service.InitConfig();
            /* Check status - no valid configuration.
                    All MS SQL tables are empty.  */
            Assert.AreEqual(status, 0x0A);
        }
        [TestMethod]
        public void ConfigTestDefault()
        {
            using (OdbcConnection connection = new OdbcConnection(connectionString))
            {
                // Create and ODBC connection
                connection.Open();

                // Insert default serial port settings into port_config table 
                string queryString = @"INSERT INTO [dbo].[port_config] 
                (port_name, baud_rate, data_bits, stop_bits, parity, handshake, timeout) 
                VALUES ('COM1', 9600, 8, 'One', 'None', 'None', 500);";
                using (OdbcCommand command = new OdbcCommand(queryString, connection))
                {
                    // Execute the data reader
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                    }
                }

                // Insert default value into port_data table 
                queryString = @"INSERT INTO [dbo].[port_data] 
                (port_name) VALUES ('COM1');";

                using (OdbcCommand command = new OdbcCommand(queryString, connection))
                {
                    // Execute the data reader
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                    }
                }
                /* The connection is automatically closed at
                the end of the Using block. */
            }

            // Read status of the serial port configuration 
            status = Service.InitConfig();

            // Check status not - initial value
            Assert.AreNotEqual(status, 0xFF);
            // Check status not - no valid configuration
            Assert.AreNotEqual(status, 0x0A);
        }

        private void ClearTables()
        {
            using (OdbcConnection connection = new OdbcConnection(connectionString))
            {
                // Create and ODBC connection
                connection.Open();

                // Remove all rows in port_config table
                string queryString = "DELETE FROM [dbo].port_config;";
                using (OdbcCommand command = new OdbcCommand(queryString, connection))
                {
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                    }
                }
                // Remove all rows in port_data table
                queryString = "DELETE FROM [dbo].port_data;";
                using (OdbcCommand command = new OdbcCommand(queryString, connection))
                {
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                    }
                }
                /* The connection is automatically closed at
                the end of the Using block. */
            }
        }
    }
}
