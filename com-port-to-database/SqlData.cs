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
        public static Attributes.PortConfig[] QueryPortsConfig()
        {
            try
            {
                using (OdbcConnection connection = new OdbcConnection(connectionString))
                {
                    // Create and ODBC connection
                    connection.Open();

                    // Get number of rows in the port_config table
                    string queryString = @"SELECT COUNT([port_name])
                                               FROM[Com_Port].[dbo].[port_config];";

                    OdbcCommand command = new OdbcCommand(queryString, connection);

                    // Execute the data reader and access the port_config table.
                    OdbcDataReader reader = command.ExecuteReader(System.Data.CommandBehavior.SingleRow);

                    reader.Read();
                    // Save the result of SQL-query to the variable 
                    byte portCounter = reader.GetByte(0);

                    // Always call Close and Dispose when done reading
                    reader.Close();
                    command.Dispose();

                    // Read the COM-port configuration data
                    queryString = @"SELECT[port_name]
                                              ,[baud_rate]
                                              ,[data_bits]
                                              ,[stop_bits]
                                              ,[parity]
                                              ,[handshake]
                                              ,[timeout]
                                           FROM [Com_Port].[dbo].[port_config];";

                    command = new OdbcCommand(queryString, connection);

                    // Execute the data reader and access the port_config table data
                    reader = command.ExecuteReader();

                    string ports = null;

                    // Com port structure initialization
                    Attributes.PortConfig[] config = new Attributes.PortConfig[portCounter];

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
                        config[portCounter].portData = new List<Attributes.PortData> { };
                        ports += "'" + config[portCounter].portName + "',";
                        portCounter++;
                    }

                    // Always call Close and Dispose when done reading
                    reader.Close();
                    command.Dispose();

                    //  Read the data to send through the serial port
                    queryString = @"SELECT[id]
                                          ,[port_name]
                                          ,[send_data]
                                        FROM [Com_Port].[dbo].[port_data] 
                                        WHERE port_name IN(" + ports.Remove(ports.Length - 1) + ")" +
                                        "GROUP BY port_name, send_data, id ORDER BY id;";

                    command = new OdbcCommand(queryString, connection);

                    // Execute the data reader and access the data to send
                    reader = command.ExecuteReader();

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

                    // Always call Close and Dispose when done reading
                    reader.Close();
                    command.Dispose();

                    /* The connection is automatically closed at
                       the end of the Using block. */
                    return config;
                }

            }
            catch (OdbcException e)
            {
                Service.log.Error(e);
                return null;
            }
            catch (Exception e)
            {
                Service.log.Error("The exception occurred while reading the COM-port database configuration : " + e);
                return null;
            }
        }

        // The static method writes the serial port response to SQL database
        public static void Write(string res, string id)
        {
            try
            {
                string queryString = "UPDATE [Com_Port].[dbo].[port_data] " +
                "SET response_date_time = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'," +
                "response_data = '" + res + "' " +
                "WHERE id = '" + id + "';";

                using (OdbcConnection connection = new OdbcConnection(connectionString))
                {
                    OdbcCommand command = new OdbcCommand(queryString, connection);

                    connection.Open();

                    // Execute the data reader and write the comp port data to port_data table
                    OdbcDataReader reader = command.ExecuteReader();

                    // Always call Close and Dispose when done reading
                    reader.Close();
                    command.Dispose();
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
            /* The connection is automatically closed at
                      the end of the Using block. */
        }
    }
}
