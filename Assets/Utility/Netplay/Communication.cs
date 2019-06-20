using Assets.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Assets.Utility.Netplay
{
    public class Communication
    {
        public static string OpponentIp = "192.168.0.1";
        public static int Port = 11000;
        public enum CommunicationType { InputHistory, ReadyToBeginSync, ClientConnectingToHost, Unset, Synchronization}

        public CommunicationType Communication_Type;
        public string Message;

        public static void InitializeCommuncationSettings(bool isHost)
        {
            Port = GameManager.Instance.Configuration.Port;
            if(!isHost)
            {
                OpponentIp = GameManager.Instance.Configuration.IpAddress_Debug;
            }
        }

        public Communication()
        {
            Communication_Type = CommunicationType.Unset;
            Message = String.Empty;
        }

        public void Send()
        {
            var thisAsMessage = JsonUtility.ToJson(this);
            Send(thisAsMessage);
        }

        private static void Send(string message)
        {
            Boolean exception_thrown = false;
            Socket sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress send_to_address = IPAddress.Parse(OpponentIp);
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, Port);


            // the socket object must have an array of bytes to send.
            // this loads the string entered by the user into an array of bytes.
            byte[] send_buffer = Encoding.ASCII.GetBytes(message);
            try
            {
                sending_socket.SendTo(send_buffer, sending_end_point);
            }
            catch (Exception send_exception)
            {
                exception_thrown = true;
                Console.WriteLine(" Exception {0}", send_exception.Message);
            }
            if (exception_thrown == false)
            {
                Console.WriteLine("Message has been sent to the broadcast address");
            }
            else
            {
                exception_thrown = false;
                Console.WriteLine("The exception indicates the message was not sent.");
            }
        }

        public static Communication CreateGuestFirstContact(string myCharacter)
        {
            var msg = new OnlineGame.ConnectToHostMessage() { MyCharacter = myCharacter, MyIP = GetOwnIp() };
            Communication com = new Communication()
            {
                Communication_Type = CommunicationType.ClientConnectingToHost,
                Message = JsonUtility.ToJson(msg)
            };
            return com;
        }

        private static string GetOwnIp()
        {
            return "192.168.1.20";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
