using System.Threading;
using System.IO.Ports;
using System;
using System.Collections.Generic;
using System.IO;

namespace com_port_to_database
{
     class ComPort
    {
        public static bool _run;
        public bool _continue;
        private SerialPort _serialPort;
        private Thread _readThread;
        private Attributes.PortConfig portConfig;

        public ComPort(Attributes.PortConfig portConfig)
        {
            this.portConfig = portConfig;
        }

        // The method opens a new serial port connection
        public void Open()
        {
            _serialPort = new SerialPort();

            // At least one thread is going to be created
            _run = true;

            try
            {
                // set the appropriate properties
                _serialPort.PortName = portConfig.portName;
                _serialPort.BaudRate = Convert.ToInt32(portConfig.baudRate);
                _serialPort.DataBits = Convert.ToInt16(portConfig.dataBits);
                _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), portConfig.stopBits);
                _serialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), portConfig.handShaking);
                _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), portConfig.parity);

                // Set the read/write timeouts
                _serialPort.ReadTimeout = Convert.ToInt32(portConfig.timeOut);
                _serialPort.WriteTimeout = Convert.ToInt32(portConfig.timeOut);
            }
            catch (Exception e)
            {
                Service.log.Error("The com port settings error: " + e);
                return;
            }

            try
            {
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException e)
            { Service.log.Error(e); }
            catch (ArgumentOutOfRangeException e)
            { Service.log.Error(e); }
            catch (ArgumentException e)
            { Service.log.Error(e); }
            catch (IOException e)
            { Service.log.Error(e); }
            catch (InvalidOperationException e)
            { Service.log.Error(e); }

            if (_serialPort.IsOpen)
            {
                Service.log.Debug(Convert.ToString(portConfig.portName) + " is open");
                // Create a new thread to read and write on the serial port 
                _continue = true;
                _readThread = new Thread(() => Read(portConfig.portData));
                try
                {
                    _readThread.Start();
                }
                catch(ThreadStateException e) { Service.log.Error(e); }
                catch (OutOfMemoryException e) { Service.log.Error(e); }
            } else
            {
                Service.InitConfig();
            }
        }

        // The method reads and writes data on the serial port 
        private void Read(List<Attributes.PortData> portData)
        {
            byte i = 0;
            int len = portData.Count;

            // The flag response-timeout
            bool flag = true;

            while (_continue && _run)
            {

                // Send the data to the serial port 
                if (!String.IsNullOrEmpty(portData[i].send) && flag)
                {
                    Send(portData[i].send);
                    flag = false;
                }

                string id = portData[i].id;

                if (i < len - 1) i++;
                else i = 0;

                try
                {

                    string message = _serialPort.ReadLine();

                    // The static method writes the serial port data to SQL database
                    if (!String.IsNullOrEmpty(message))
                    {
                        SqlData.Write(message, id);
                        flag = true;
                    }

                }
                catch (TimeoutException) { flag = true; }
                catch (ThreadAbortException) { _continue = false; }
                catch (IOException) { _continue = false; }
                catch (Exception) { _continue = false;  }

            }
            _readThread.Join(500);
            _serialPort.Close();
            Service.InitConfig();
        }

         // The method sends data to the serial port
         private void Send(string IncomingData)
         {
             try
             {
              // Send the user's text straight out the port 
              _serialPort.WriteLine(IncomingData);
            }
             catch (TimeoutException) { }  
             catch (InvalidOperationException) { }
             catch (ArgumentOutOfRangeException) { }
             catch (ArgumentException) { }
             catch (Exception) { }
        }

        // The method closes the port connection
        public void Close()
        {
            _continue = false;
        }
    }
}
