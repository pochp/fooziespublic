using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Utility.Netplay.Communications
{
    class GuestFirstContactToHost : Communication
    {
        public string GuestIp;

        public GuestFirstContactToHost()
        {
            GuestIp = string.Empty;
        }
    }
}
