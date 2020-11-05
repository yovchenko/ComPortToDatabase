using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Configuration;

namespace com_port_to_database
{
    class SqlData
    {
        // ODBC-connection string
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["Com_Port"].ConnectionString;

        //The static method reads the serial port configuration from SQL database
        public static byte QueryPortsConfig(ref Attributes.PortConfig[] config)
        {
            try
            {
                using (OdbcConnection connection = new OdbcConnection(connectionString))
                {
                    // Create and ODBC connection
                    connection.Open();

                    // Get number of rows with the serial port settings
                    string queryString = @"SELECT COUNT(*) OVER () AS TotalRecords 
                                           FROM[Com_Port].[dbo].[port_config] AS pc
                                           JOIN port_data AS pd ON pc.port_name = pd.port_name
                                           GROUP BY pc.port_name;";

                    byte portCounter = 0;

                    using (OdbcCommand command = new OdbcCommand(queryString, connection))
                    {
                        // Execute the data reader and access the port_config table
                        using (OdbcDataReader reader = command.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                        {
                            reader.Read();

                            portCounter = reader.GetByte(0);
                        }
                    }

                    // Read the COM-port configuration data
                    queryString = @"SELECT DISTINCT pc.[port_name]
	                                        ,pc.[baud_rate]
	                                        ,pc.[data_bits]
	                                        ,pc.[stop_bits]
	                                        ,pc.[parity]
	                                        ,pc.[handshake]
	                                        ,pc.[timeout]
                                    FROM [Com_Port].[dbo].[port_config] AS pc 
                                    JOIN port_data AS pd ON pc.port_name = pd.port_name;";

                    string ports = null;

                    using (OdbcCommand command = new OdbcCommand(queryString, connection))
                    {
                        // Execute the data reader and access the port_config table data
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows || portCounter == 0) return 0;

                            // Reassign a new value to the variable
                            portCounter = 0;

                            while (reader.Read())
                            {
                                config[portCounter].portName = Convert.ToString(reader["port_name"]);
                                config[portCounter].baudRate = Convert.ToString(reader["baud_rate"]);
                                config[portCounter].dataBits = Convert.ToString(reader["data_bits"]);
                                config[portCounter].stopBits = Convert.ToString(reader["stop_bits"]);
                                config[portCounter].parity = Convert.ToString(reader["parity"]);
                                config[portCounter].handShaking = Convert.ToString(reader["handshake"]);
                                config[portCounter].timeOut = Convert.ToString(reader["timeout"]);
                                if (config[portCounter].portData == null) config[portCounter].portData = new List<Attributes.PortData> { };
                                else config[portCounter].portData.Clear();
                                ports += "'" + config[portCounter].portName + "',";
                                portCounter++;
                            }
                        }
                    }

                    //  Read the data to send through the serial port
                    queryString = @"SELECT[id]
                                          ,[port_name]
                                          ,[send_data]
                                        FROM [Com_Port].[dbo].[port_data] 
                                        WHERE port_name IN(" + ports.Remove(ports.Length - 1) + ")" +
                                        "GROUP BY port_name, send_data, id ORDER BY id;";


                    using (OdbcCommand command = new OdbcCommand(queryString, connection))
                    {
                        // Execute the data reader and access the data to send
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows || String.IsNullOrEmpty(ports)) return 0;

                            // Add the data to send to the structure 
                            while (reader.Read())
                            {
                                for (int i = 0; i < config.Length; i++)
                                {
                                    if (config[i].portName == Convert.ToString(reader["port_name"]))
                                    {
                                        Attributes.PortData portData = new Attributes.PortData();
                                        portData.id = Convert.ToString(reader["id"]);
                                        portData.send = Convert.ToString(reader["send_data"]);
                                        config[i].portData.Add(portData);
                                    }
                                }
                            }
                        }
                    }
                    /* The connection is automatically closed at
                       the end of the Using block. */
                    return portCounter;
                }

            }
            catch (OdbcException e)
            {
                Service.log.Error(e);
                return 0;
            }
            catch (Exception e)
            {
                Service.log.Error("The exception occurred while reading the COM-port database configuration : " + e);
                return 0;
            }
        }

        // The static method writes the serial port response to SQL database
        public static void Write(string id, string message)
        {
            try
            {
                using (OdbcConnection connection = new OdbcConnection(connectionString))
                {
                    // Create and ODBC connection
                    connection.Open();

                    string queryString = "UPDATE [Com_Port].[dbo].[port_data] " +
                                         "SET response_date_time = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'," +
                                         "response_data = '" + message + "' " +
                                         "WHERE id = '" + id + "';";

                    using (OdbcCommand command = new OdbcCommand(queryString, connection))
                    {
                        // Execute the data reader and write the serial port data to SQL database table
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {

                        }
                    }
                    /* The connection is automatically closed at
                    the end of the Using block. */
                }
            }
            catch (OdbcException e)
            {
                Service.log.Error(e);
            }
            catch (Exception e)
            {
                Service.log.Error("The exception occurred while writing the COM-port response to SQL database : " + e);
            }
        }
    }
}
