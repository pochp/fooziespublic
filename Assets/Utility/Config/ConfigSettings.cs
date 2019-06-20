using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Utility.Config
{
    [Serializable()]
    public class ConfigSettings
    {
        //Online settings
        public int Port;
        public string IpAddress_Debug; //to be used for guest connecting to host ONLY
        public int RollbackMax;
        public int ExtraInputDelay;

        public ConfigSettings()
        {
            Port = 11000;
            IpAddress_Debug = "192.168.0.1";
            RollbackMax = 20;
            ExtraInputDelay = 0;
        }
    }
}
