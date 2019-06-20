using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Assets.Utility.Netplay
{
    /// <summary>
    /// This is mostly for listening
    /// The idea is that it runs in a separate thread, so messages just get stacked until they are read
    /// 
    /// multithreading taken from : https://answers.unity.com/questions/357033/unity3d-and-c-coroutines-vs-threading.html
    /// </summary>
    class NetworkManager : Singleton<NetworkManager>
    {
        private bool _stopListening = true;
        private object m_Handle = new object();

        private UdpClient _listener;
        private int _listenPort = 11000;
        private IPEndPoint _groupEP;
        private List<Communication> _unreadCommunications;
        private List<Communication> _lastReadCommunications;
        private object _unreadCommunicationsHandle = new object();
        private object _lastReadCommunicationsHandle = new object();

        /// <summary>
        /// WARNING : do not use operations that modify these lists. Overwrite them with another list, so that the lock system works properly and prevents errors
        /// </summary>
        public List<Communication> UnreadCommunications
        {
            get
            {
                List<Communication> tmp;
                lock (_unreadCommunicationsHandle)
                {
                    tmp = _unreadCommunications;
                }
                return tmp;
            }
            set
            {
                lock (_unreadCommunicationsHandle)
                {
                    _unreadCommunications = value;
                }
            }
        }
        public List<Communication> LastReadCommunications
        {
            get
            {
                List<Communication> tmp;
                lock (_lastReadCommunicationsHandle)
                {
                    tmp = _lastReadCommunications;
                }
                return tmp;
            }
            set
            {
                lock (_lastReadCommunicationsHandle)
                {
                    _lastReadCommunications = value;
                }
            }
        }


        public bool IsDone
        {
            get
            {
                bool tmp;
                //lock (m_Handle)
                {
                    tmp = _stopListening;
                }
                return tmp;
            }
            set
            {
                //lock (m_Handle)
                {
                    _stopListening = value;
                }
            }
        }

        private NetworkManager()
        {
            UnreadCommunications = new List<Communication>();
            LastReadCommunications = new List<Communication>();
        }

        public void StartListening()
        {
            _listenPort = GameManager.Instance.Configuration.Port;
            UnreadCommunications = new List<Communication>();
            LastReadCommunications = new List<Communication>();
            _listener = new UdpClient(_listenPort);
            _listener.Client.SendTimeout = 300;
            _listener.Client.ReceiveTimeout = 300;
            _groupEP = new IPEndPoint(IPAddress.Any, _listenPort);
            
            IsDone = false;
        }

        public void StopListening()
        {
            _listener.Close();
            IsDone = true;
        }

        // Update is called once per frame
        void Update()
        {
            if(!IsDone)
            {
                Listen();
            }
        }


        public List<Communication> GetUnreadCommunications()
        {
            //get unread messages
            //set messages to be removed from unread messages
            //we need to add them to the set, in case this gets called before they have had time to be removed
            //return
            var unreadCommunications = UnreadCommunications;
            var lastReadCommunications = LastReadCommunications;
            lastReadCommunications.AddRange(unreadCommunications);
            var communicationsToRemove = lastReadCommunications.Distinct().ToList();
            LastReadCommunications = communicationsToRemove;
            return lastReadCommunications;
        }

        private void Listen()
        {
            string received_data;
            byte[] receive_byte_array;
            try
            {
                Console.WriteLine("Waiting for broadcast");
                // this is the line of code that receives the broadcase message.
                // It calls the receive function from the object listener (class UdpClient)
                // It passes to listener the end point groupEP.
                // It puts the data from the broadcast message into the byte array
                // named received_byte_array.
                // I don't know why this uses the class UdpClient and IPEndPoint like this.
                // Contrast this with the talker code. It does not pass by reference.
                // Note that this is a synchronous or blocking call.
                receive_byte_array = _listener.Receive(ref _groupEP);
                Console.WriteLine("Received a broadcast from {0}", _groupEP.ToString());
                received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                Console.WriteLine("data follows \n{0}\n\n", received_data);
                Communication recievedCom = JsonUtility.FromJson(received_data, typeof(Communication)) as Communication;

                //add communication to unread coms while making sure to respect the lock
                var unreadComs = new List<Communication>();
                unreadComs.AddRange(this.UnreadCommunications);
                unreadComs.Add(recievedCom);
                var lastReadComs = new List<Communication>();
                lastReadComs.AddRange(this.LastReadCommunications);
                unreadComs.RemoveAll(o => lastReadComs.Contains(o));
                this.UnreadCommunications = unreadComs;
                this.LastReadCommunications = new List<Communication>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
