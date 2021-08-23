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
        public enum NetworkType { Live = 22222, Simulated = 1337 };
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

        private void NetworkBrain()
        {
            int count = 0;

            while(State == NetworkState.Listening)
            {
                Thread.Sleep(1000);

                SendData(Encoding.UTF8.GetBytes("Testing Count: " + count++), IpConnection, this);
            }
        }

        public override void DataListener(byte[] data, string SourceIP)
        {
            Console.WriteLine(SourceIP + " Sent: " + Encoding.UTF8.GetString(data));
        }

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

        private void InternetSend(byte[] data, string ip)
        {
            IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse(ip), (int)Network);

            sendSocket.SendTo(data, EndPoint);
        }

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
