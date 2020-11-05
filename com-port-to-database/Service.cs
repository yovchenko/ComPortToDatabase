using System;
using System.IO.Ports;
using System.ComponentModel;

namespace com_port_to_database
{
    public class Service
    {
        // Logger declaration 
        public static readonly log4net.ILog log = log4net.LogManager
        .GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Windows 7, 8, and 10 limits the number of COM ports to 256.
        private static ComPort[] comPortArr = new ComPort[255];

        // Create a static instance of configuration for serial ports
        private static Attributes.PortConfig[] config = new Attributes.PortConfig[255];

        private static System.Timers.Timer aTimer;

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

        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            // Set initial configuration
            InitConfig();
        }

        // The method reads configuration data from SQL-database
        public static void InitConfig()
        {
            aTimer.Enabled = false;

            // The static method 
            byte len = SqlData.QueryPortsConfig(ref config);

            if (len == 0)
            {
                aTimer.Enabled = true;
                return;
            }

            string[] ArrayComPortsNames;
            int index = -1;

            try
            {
                // Call the method to query the current computer for a list of valid serial port
                ArrayComPortsNames = SerialPort.GetPortNames();
            }
            catch (Win32Exception e)
            {
                log.Error(e);
                aTimer.Enabled = true;
                return;
            }

            Array.Sort(ArrayComPortsNames);

            byte activePorts = 0;

            // Compare SQL database data with the list of valid serial port
            if (ArrayComPortsNames.GetUpperBound(0) != index)
            {
                do
                {
                    index += 1;
                    for (int i = 0; i < len; i++)
                    {
                        if (ArrayComPortsNames[index] == config[i].portName)
                        {
                            if (comPortArr[i] != null)
                            {
                                // The thread is already running
                                if (comPortArr[i]._continue)
                                {
                                    activePorts++;
                                    break;
                                }
                            }
                            // A new serial port initialization
                            comPortArr[i] = new ComPort(config[i]);
                            comPortArr[i].Open();
                            activePorts++;
                            break;
                        }
                    }
                }
                while (index != ArrayComPortsNames.GetUpperBound(0));
            }

            if (activePorts == len)
            {
                log.Debug("ODBC connection is established. All serial ports are found!");
            }
            else
            {
                log.Error("The serial port with the given name was not detected!");
                aTimer.Enabled = true;
            }
        }



        // The method is called when the service is about to stop
        public void OnStop()
        {
            // Stop all currently running threads
            ComPort._run = false;
            aTimer.Enabled = false;
        }
    }

}
