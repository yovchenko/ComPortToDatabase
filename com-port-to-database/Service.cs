using System;
using System.IO.Ports;
using System.ComponentModel;
using System.Threading.Tasks;

namespace com_port_to_database
{
    public class Service
    {
        // Logger declaration 
        public static readonly log4net.ILog log = log4net.LogManager
        .GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Windows 7, 8, and 10 limits the number of COM ports to 256.
        private static ComPort[] comPortArr = new ComPort[255];

        //
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

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Set initial configuration
            InitConfig();
        }

        // The method reads configuration data from SQL-database
        public static void InitConfig()
        {
            aTimer.Enabled = false;

            Attributes.PortConfig[] config = SqlData.QueryPortsConfig();
            if (config == null)
            {
                aTimer.Enabled = true;
                return;
            }
   
            string[] ArrayComPortsNames = null;
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

            // Compare SQL database data with the list of valid serial port
            if (ArrayComPortsNames.GetUpperBound(0) != index)
            {
                do
                {
                    index += 1;
                    for (int i = 0; i < config.Length; i++)
                    {
                        if (ArrayComPortsNames[index] == config[i].portName)
                        {
                            if (comPortArr[i] != null)
                            {
                                comPortArr[i].Close();
                                try
                                {
                                    // Create a task that will complete after a time delay
                                    Task.Delay(500).ContinueWith(t =>
                                    {
                                        // Set a new configuration to the given serial port.
                                        comPortArr[i].Open(config[i]);
                                    });

                                }
                                catch(ArgumentOutOfRangeException e) { log.Error(e); }
                                catch(TaskCanceledException e) { log.Error(e); }
                                catch(ObjectDisposedException e) { log.Error(e); }
                            } else
                            {
                                // A serial port initialization
                                comPortArr[i] = new ComPort();
                                comPortArr[i].Open(config[i]);
                            }
                            break;
                        }
                    }
                }
                while (index != ArrayComPortsNames.GetUpperBound(0));
            }

            if(ComPort._run)
            {
                log.Debug("ODBC connection is established!");
            } else
            {
                log.Error("The serial port with the given name was not detected!");
                aTimer.Enabled = true;
            }
        }

  

        // The method is called when the service is about to stop
        public void OnStop()
        {
            ComPort._run = false;
            aTimer.Enabled = false;
        }
    }

}
