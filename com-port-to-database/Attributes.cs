using System.Collections.Generic;

namespace com_port_to_database
{
    class Attributes
    {
        #region Data structures
        public struct PortData
        {
            public string id;
            public string send;
        }
        public struct PortConfig
        {
            public string portName;
            public string baudRate;
            public string dataBits;
            public string stopBits;
            public string parity;
            public string handShaking;
            public string timeOut;
            public List<PortData> portData;
        }
        #endregion
    }
}
