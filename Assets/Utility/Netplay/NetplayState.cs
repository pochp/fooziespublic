using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Utility.Netplay
{
    public class NetplayState
    {
        public bool IsReadyToStartMatch = false;
        public bool HasRecievedReadyFromOpponent = false;
        public bool IsHost = false;
        public string OpponentIp
        {
            get { return _opponentIp; }
            set
            {
                _opponentIp = value;
                Communication.OpponentIp = value;
            }
        }
        private string _opponentIp = string.Empty;
        public bool IsFake = false; //debugging purpose
        public NetplayState()
        {

        }
    }
}
