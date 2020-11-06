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
                // Set the appropriate properties
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
                _readThread = new Thread(() => PortBegin(ref portConfig.portData));
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
        private void PortBegin(ref List<Attributes.PortData> portData)
        {
            byte i = 0;
            int len = portData.Count;
            Attributes.ReadData readData = 
            new Attributes.ReadData { id = portData[i].id, read = null };

            while (_continue && _run)
            {

                // Send the data to the serial port 
                if (!String.IsNullOrEmpty(portData[i].send))
                {
                    PortSend(portData[i].send);
                }

                PortRead(ref readData);

                // The static method writes the serial port data to SQL database
                SqlData.Write(readData.id, readData.read);
                
                if (i < len - 1) i++;
                else i = 0;

                readData.id = portData[i].id;
            }
            _readThread.Join(500);
            _serialPort.Close();
            Service.InitConfig();
        }

        // Read the data from the serial port using ReadLine method
        private void PortRead(ref Attributes.ReadData rd)
        {
            try
            {
                // Read the serial port data
                string message = _serialPort.ReadLine();
                rd.read = message;
            }
            catch (TimeoutException) { rd.read = null; }
            catch (ThreadAbortException) { _continue = false; }
            catch (IOException) { _continue = false; }
            catch (Exception) { _continue = false; }
        }

        // Send the data to the serial port using WriteLine method
        private void PortSend(string IncomingData)
         {
             try
             {
              // Send the user's text straight out the port 
              _serialPort.WriteLine(IncomingData);
            }
             catch (TimeoutException) { }  
             catch (InvalidOperationException) { _continue = false; }
             catch (ArgumentNullException) { _continue = false; }
             catch (Exception) { _continue = false; }
        }
    }
}
