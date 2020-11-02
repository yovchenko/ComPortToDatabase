using System;
using System.Text;
using System.Threading;
using System.Configuration;
using System.IO.Ports;
using System.Data.Odbc;

namespace com_port_to_database
{
    public class Service
    {
        // Logger declaration 
        private static readonly log4net.ILog log = log4net.LogManager
        .GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Static fields  
        private static System.Timers.Timer aTimer;

        #region Data structures
        public struct PortConfig
        { 
            public string portName;
            public string baudRate;
            public string dataBits;
            public string stopBits;
            public string parity;
            public string handShaking;
            public string timeOut;
            public string[] send;
        }
        #endregion

        // The method runs when the service is about to start
        public void OnStart()
        {
            ResetConfig();
        }

        public static void ResetConfig()
        {
            // A timer creation 
            aTimer = new System.Timers.Timer
            {
                Interval = 5000
            };

            // The event for the timer. 
            aTimer.Elapsed += OnTimedEvent;

            // Repeated events
            aTimer.AutoReset = true;

            // Start the timer
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Set initial configuration
            InitConfig();
        }

        // The method reads configuration data from SQL-database
        public static void InitConfig()
        {
            aTimer.Enabled = false;
        
            // ODBC-connection string
            string connectionString = ConfigurationManager.ConnectionStrings["Com_Port"].ConnectionString;

            try
            {
                using (OdbcConnection connection = new OdbcConnection(connectionString))
                {
                    // Create and ODBC connection
                    connection.Open();

                        // Get number of rows in the port_config table
                        string queryString = @"SELECT COUNT([port_name])
                                               FROM[Com_Port].[dbo].[port_config]";

                        OdbcCommand command = new OdbcCommand(queryString, connection);

                        // Execute the data reader and access the port_config table.
                        OdbcDataReader reader = command.ExecuteReader(System.Data.CommandBehavior.SingleRow);

                        reader.Read();
                        // Save the result of SQL-query to the variable 
                        byte portCounter = reader.GetByte(0);

                        // Always call Close and Dispose when done reading.
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
                                           FROM[Com_Port].[dbo].[port_config]";

                        command = new OdbcCommand(queryString, connection);

                        // Execute the data reader and access the port_config table.
                        reader = command.ExecuteReader();

                        string ports = null;
                        PortConfig[] config = new PortConfig[portCounter];

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
                            ports += config[portCounter].portName + ",";
                            portCounter++;
                        }

                        // Always call Close and Dispose when done reading.
                        reader.Close();
                        command.Dispose();

                        queryString = @"SELECT[port_name]
                                          ,[send]
                                          ,[response]
                                          ,[date_time_response]
                                        FROM[Com_Port].[dbo].[port_data] 
                                        WHERE port_name IN(" + ports.Remove(ports.Length - 1) + ")";

                        command = new OdbcCommand(queryString, connection);

                        // Execute the data reader and access the port data table.
                        reader = command.ExecuteReader();


                        byte counter = 0;
                        while (reader.Read())
                        {
                            //dataTable.send[counter] = Convert.ToString(reader["send"]);
                            counter++;
                        }

                        // call Close when done reading of the dataTable.
                        reader.Close();
                        command.Dispose();

                    // The connection is automatically closed at
                    // the end of the Using block.
                }

            }
            catch (OdbcException e)
            {
                log.Error(e);
                aTimer.Enabled = true;
                return;
            }
            catch (Exception e)
            {
                log.Error("The exception occurred while reading the COM-port database configuration : " + e);
                aTimer.Enabled = true;
                return;
            }

            string[] ArrayComPortsNames = null;
            int index = -1;
            //string comPortName = null;

            try
            {
                // call the method to query the current computer for a list of valid serial port
                ArrayComPortsNames = SerialPort.GetPortNames();
            }
            catch (Exception)
            {
                aTimer.Enabled = true;
            }

            Array.Sort(ArrayComPortsNames);

            if (ArrayComPortsNames.GetUpperBound(0) != index)
            {
                do
                {
                    index += 1;
                    //if (ArrayComPortsNames[index] == config.portName)
                    //{
                    //    comPortName = config.portName;
                    //    break;
                    //}
                }
                while (index != ArrayComPortsNames.GetUpperBound(0));
            }

           // if (string.IsNullOrEmpty(comPortName) && dataTable.send.Length != 0)
           // {
           //     log.Debug("ODBC connection is established");
                //Open(config, dataTable);
           // }
           // else
           // {
           //     log.Error("The serial port with the given name was not detected");
           //     aTimer.Enabled = true;
           // }
        }

        // the method runs when the service is about to stop
        public void OnStop()
        {
            aTimer.Enabled = false;
            //Close(false);
        }

        /*

        // the method opens a new serial port connection
        public static void Open(PortConfig pc, DataTable dt)
        {
            aTimer.Enabled = false;
            _serialPort = new SerialPort();

            try
            {
                // set the appropriate properties
                _serialPort.PortName = Convert.ToString(pc.portName);
                _serialPort.BaudRate = Convert.ToInt32(pc.baudRate);
                _serialPort.DataBits = Convert.ToInt16(pc.dataBits);
                _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), pc.stopBits);
                _serialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), pc.handShaking);
                _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), pc.parity);

                // Set the read/write timeouts
                _serialPort.ReadTimeout = Convert.ToInt32(pc.timeOut);
                _serialPort.WriteTimeout = Convert.ToInt32(pc.timeOut);
            }
            catch (Exception e)
            {
                aTimer.Enabled = true;
                log.Error(e);
            }

            try
            {
                _serialPort.Open();
                log.Debug(Convert.ToString(pc.portName) + " is open");
            }
            catch (Exception e)
            {
                aTimer.Enabled = true;
                log.Error(e);
            }

            if (_serialPort.IsOpen)
            {
                // create a new thread to read from the serial port 
                _continue = true;
                readThread = new Thread(() => Read(dt));
                readThread.Start();
            }
            else
            {
                aTimer.Enabled = true;
            }

        }

        // the method reads data from the serial port 
        public static void Read(DataTable dataTable)
        {
            aTimer.Enabled = false;
            byte i = 0;
            int len = dataTable.send.Length;

            while (_continue)
            {
                // send data to the serial port 
                Send(dataTable.send[i]);

                if (i < len - 1) i++;
                else i = 0;
                try
                {
                    string message = _serialPort.ReadLine();
                    // write the serial port data to the database
                    if (message != null) Write(message);
                }
                catch (System.TimeoutException)
                {

                }

                catch (System.OperationCanceledException)
                {
                    Close(true);
                }
                catch (Exception)
                {
                    Close(true);
                }

            }
        }

        // the method sends data to the serial port
        public static void Send(string IncomingData)
        {
            try
            {
                var bytes = Encoding.ASCII.GetBytes(IncomingData);
                _serialPort.Write(bytes, 0, bytes.Length);
            }
            catch (System.TimeoutException)
            {

            }
            catch (Exception)
            {
                Close(true);
            }
        }

        // the method writes the serial port data to the database
        public static void Write(string res)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["comPort"].ConnectionString;

            try
            {
                string queryString = "UPDATE [ComPort].[dbo].[portData] " +
                "SET dateTimeResponse = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'," +
                "response = '" + res + "'" +
                "WHERE send = CASE " +
                    "WHEN CHARINDEX(CHAR(65), '" + res + "') > 0 AND " +
                    "CHARINDEX(CHAR(65), '" + res + "') < 7 " +
                    "THEN CHAR(65) " +
                    "WHEN CHARINDEX(CHAR(66), '" + res + "') > 0 AND " +
                    "CHARINDEX(CHAR(66), '" + res + "') < 7 " +
                    "THEN CHAR(66) " +
                    "WHEN CHARINDEX(CHAR(67), '" + res + "') > 0 AND " +
                    "CHARINDEX(CHAR(67), '" + res + "') < 7 " +
                    "THEN CHAR(67) " +
                    "ELSE CHAR(32) " +
                    "END;";

                using (OdbcConnection connection = new OdbcConnection(connectionString))
                {
                    OdbcCommand command = new OdbcCommand(queryString, connection);

                    connection.Open();

                    try
                    {
                        // execute the DataReader and access the port data table.
                        OdbcDataReader reader = command.ExecuteReader();

                        reader.Close();
                        command.Dispose();
                    }
                    catch (Exception)
                    {
                        Close(true);
                        command.Dispose();
                    }

                }

            }
            catch (OdbcException)
            {
                Close(true);
            }
            catch (Exception)
            {
                Close(true);
            }

        }

        // the method closes the port connection
        public static void Close(bool tReset)
        {
            if (_continue == true)
            {
                _continue = false;
                readThread.Join(500);
                _serialPort.Close();
                if (tReset) aTimer.Enabled = true;
            }
        }
         */
    }

}
