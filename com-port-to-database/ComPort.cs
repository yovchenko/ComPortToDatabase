using System.Threading;
using System.IO.Ports;

namespace com_port_to_database
{
    class ComPort
    {
        // static fields  
        private static bool _continue;
        private static SerialPort _serialPort;
        private static Thread readThread;
    }
}
