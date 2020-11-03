using System.Threading;
using System.IO.Ports;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace com_port_to_database
{
     class ComPort
    {
        public static bool _run;
        public bool _continue;
        private SerialPort _serialPort;
        private Thread _readThread;

        // The method opens a new serial port connection
        public void Open(Attributes.PortConfig pc)
        {
            bool _error = false;
            _run = true;
            _serialPort = new SerialPort();

            try
            {
                // set the appropriate properties
                _serialPort.PortName = pc.portName;
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
                Service.log.Error("The com port settings error: " + e);
                return;
            }

            try
            {
                _serialPort.Open();
                Service.log.Debug(Convert.ToString(pc.portName) + " is open");
            }
            catch (UnauthorizedAccessException)
            { _error = true; }
            catch (ArgumentOutOfRangeException)
            { _error = true; }
            catch (ArgumentException)
            { _error = true; }
            catch (IOException)
            { _error = true; }
            catch (InvalidOperationException)
            { _error = true; }

            if (_serialPort.IsOpen)
            {
                // Create a new thread to read and write on the serial port 
                _continue = true;
                _readThread = new Thread(() => Read(pc.portData));
                _readThread.Start();
            }
            else _error = true;

            if (_error)
            {
                _continue = false;
                Service.log.Error("Unable to open serial com port: " + pc.portName);
            }
        }

        // The method reads and writes data on the serial port 
        private void Read(List<Attributes.PortData> portData)
        {
            byte i = 0;
            int len = portData.Capacity;

            while (_continue && _run)
            {
                // Send the data to the serial port 
                Send(portData[i].send);

                if (i < len - 1) i++;
                else i = 0;

                try
                {
                    string message = _serialPort.ReadLine();

                    // The static method writes the serial port data to SQL database
                    if (message != null) SqlData.Write(message, portData[i].id);
                }
                catch (TimeoutException) { }
                catch (ThreadAbortException) { }
                catch (IOException) { }
                catch (Exception) { }

            }
            _readThread.Join(500);
            _serialPort.Close();
        }

         // The method sends data to the serial port
         private void Send(string IncomingData)
         {
             try
             {
                 var bytes = Encoding.ASCII.GetBytes(IncomingData);
                 _serialPort.Write(bytes, 0, bytes.Length);
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
