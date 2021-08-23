using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using SimulatedInternet;

namespace Simulated_Internet_Example
{
    public partial class Form1 : Form
    {
        List<NetworkAppExample> examples = new List<NetworkAppExample>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LiveSetUp();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SimulationSetUp();
        }

        public void LiveSetUp()
        {
            NetworkAppExample example = new NetworkAppExample(null);
            example.SetDestinationIP("127.0.0.1");
            example.StartNetwork();
        }

        public void SimulationSetUp()
        {
            #region SETUP FOR SIMULATION

            ///////////////////////////////////////////////////////////////////////////////////
            ///    THIS WILL AUTOMATICALLY BE DONE FOR THE CLIENTS IF THE HOST IP IS SET    ///
            ///////////////////////////////////////////////////////////////////////////////////
            
            InternetManager manager = new InternetManager();

            examples.Add(new NetworkAppExample(manager, NetworkAppExample.NetworkType.Simulated));
            examples.Add(new NetworkAppExample(manager, NetworkAppExample.NetworkType.Simulated));

            examples.First().SimulationID = "one";
            examples.Last().SimulationID = "two";

            manager.Details = new List<ConnectionDetail>();
            manager.Identities = new List<DetailPackage>();

            string IpOne = manager.RegisterIP();
            string IpTwo = manager.RegisterIP();

            manager.Identities.Add(new DetailPackage
            {
                IP = IpOne,
                Member = examples.First(),
                locationX = 0,
                locationY = 0
            });

            manager.Identities.Add(new DetailPackage
            {
                IP = IpTwo,
                Member = examples.Last(),
                locationX = 50,
                locationY = 50
            });

            manager.Details.Add(new ConnectionDetail
            {
                Member = examples.First(),
                ConnectedMember = examples.Last(),
                IP = IpOne,
                DestIP = IpTwo,
                Ping = manager.FakePing(manager.Identities.First(), manager.Identities.Last())
            });

            ConnectionDetail d = manager.LookUpConnection(IpOne, IpTwo);

            examples.First().SetDestinationIP(IpTwo);
            examples.Last().SetDestinationIP(IpOne);

            manager.StartNetworkSimulation();            
           
            examples.First().StartNetwork();
           
            examples.Last().StartNetwork();

            #endregion
        }
    }
}
