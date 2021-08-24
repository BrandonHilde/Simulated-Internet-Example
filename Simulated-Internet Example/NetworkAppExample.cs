using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using SimulatedInternet;

namespace Simulated_Internet_Example
{
    public class NetworkAppExample:NetworkMember
    {
        public enum NetworkType { Live = 22222, Simulated = 1337 }; // port target can be set by value
        public enum NetworkState { Listening, Hibernating}

        public NetworkType Network = NetworkType.Simulated;
        public NetworkState State = NetworkState.Hibernating;

        public InternetManager SimulatedInternet { get; set; }

        private Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        private string IpConnection = "";

        public NetworkAppExample(InternetManager SimulatedInternet, NetworkType NetType = NetworkType.Live)
        {
            this.SimulatedInternet = SimulatedInternet;
            Network = NetType;
        }

        /// <summary>
        /// Sets the connected member. This may need to be changed to a list of members in your code.
        /// </summary>
        /// <param name="IP"></param>
        public void SetDestinationIP(string IP)
        {
            IpConnection = IP;
        }

        public void StartNetwork()
        {
            StartListener();

            Thread th = new Thread(NetworkBrain);
            th.IsBackground = true;
            th.Start();
        }

        /// <summary>
        /// This is an example of a network applications main thread. Put all network opperations in here
        /// </summary>
        private void NetworkBrain()
        {
            int count = 0;

            while(State == NetworkState.Listening)
            {
                Thread.Sleep(1000);

                SendData(Encoding.UTF8.GetBytes("Testing Count: " + count++), IpConnection, this);
            }
        }

        /// <summary>
        /// This class Recieves all the incoming data whether from the live or simulated internet
        /// </summary>
        /// <param name="data"></param>
        /// <param name="SourceIP"></param>
        public override void DataListener(byte[] data, string SourceIP)
        {
            // using Console.WriteLine(); in a multi-threaded application is not advisiable but its fine in this example
            Console.WriteLine(SourceIP + " Sent: " + Encoding.UTF8.GetString(data));
        }

        /// <summary>
        /// This allows messages to be automatically routed to either the simulated internet or the live internet
        /// </summary>
        /// <param name="data"></param>
        /// <param name="DestinationIP"></param>
        /// <param name="Self"></param>
        public void SendData(byte[] data, string DestinationIP, NetworkMember Self)
        {
            if (Network == NetworkType.Simulated)
            {
                SimulatedInternet.SendBytesToIP(
                                        this,
                                        data,
                                        DestinationIP,
                                        (ushort)Network);
            }
            else
            {
                InternetSend(data, DestinationIP);
            }
        }

        /// <summary>
        /// Sends to the live internet
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        private void InternetSend(byte[] data, string ip)
        {
            IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse(ip), (int)Network);

            sendSocket.SendTo(data, EndPoint);
        }

        /// <summary>
        /// Starts the network listening and handles recieve code for the live network.
        /// </summary>
        private void StartListener()
        {
            State = NetworkState.Listening;

            if (Network == NetworkType.Live)
            {
                new Thread(() =>
                {
                    UdpClient listener = new UdpClient((int)Network);
                    IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, (int)Network);

                    try
                    {
                        while (State == NetworkState.Listening)
                        {
                            byte[] bytes = listener.Receive(ref EndPoint);

                            DataListener(bytes, EndPoint.ToString());
                        }
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine(e);
                    }
                    finally
                    {
                        listener.Close();
                        State = NetworkState.Hibernating;
                    }
                }).Start();
            }
        }
    }
}
