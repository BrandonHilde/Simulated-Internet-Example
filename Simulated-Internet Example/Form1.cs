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
            
            InternetManager manager = new InternetManager();

            examples.Add(new NetworkAppExample(manager, NetworkAppExample.NetworkType.Simulated));
            examples.Add(new NetworkAppExample(manager, NetworkAppExample.NetworkType.Simulated));

            // each app in your simulated network needs a unique id

            examples.First().SimulationID = "one"; // because there are two members we can use .First() and .Last() to tell them apart
            examples.Last().SimulationID = "two";  // if we have many members use a for loop and set SimulationID = x.toString(); as x++ increases the value

            // Many of theses steps will be automatically done by the simulated internet if the host is set up
            // In this example I will be doing everything manually
            manager.Details = new List<ConnectionDetail>();
            manager.Identities = new List<DetailPackage>();

            // RegisterIP(); creates a fake ip. You can tell its fake because it uses 256-999 range for numbers
            // Example of real IP: 127.0.0.1, 255.255.255.255, etc.  
            // Example of fake IP: 848.453.944.762 
            string IpOne = manager.RegisterIP();
            string IpTwo = manager.RegisterIP();

            ///////////////////////////////////////////////////////////////////////////////////
            ///    THIS WILL AUTOMATICALLY BE DONE FOR THE CLIENTS IF THE HOST IP IS SET    ///
            ///////////////////////////////////////////////////////////////////////////////////
            

            // here we are adding the member app and the ip address associated.
            // we also add a location so that ping can be calulated by way of distance
            // this allows us to place a member on a map and have the ping be relativily consistant amongst many different members
            // this also prevents impossible ping relations from existing in your simulation
            // regions of low and high ping should naturally emerge 

            manager.Identities.Add(new DetailPackage
            {
                IP = IpOne,
                Member = examples.First(),
                locationX = 0, // map locationX
                locationY = 0  // map locationY
            });

            manager.Identities.Add(new DetailPackage
            {
                IP = IpTwo,
                Member = examples.Last(),
                locationX = 50,
                locationY = 50
            });

            // ConnectionDetail is used to calculate the relationship between two connected members for proper simulation
            // it is also used to route messages in the simulated internet
            manager.Details.Add(new ConnectionDetail
            {
                Member = examples.First(),
                ConnectedMember = examples.Last(),
                IP = IpOne,
                DestIP = IpTwo,
                Ping = manager.FakePing(manager.Identities.First(), manager.Identities.Last())
            });

            // you may need to change .SetDestinationIP(string); to be a list of connected members depending on your network application
            examples.First().SetDestinationIP(IpTwo);
            examples.Last().SetDestinationIP(IpOne);

            // start up the engine of the simulated internet

            manager.StartNetworkSimulation();            
           
            examples.First().StartNetwork();
           
            examples.Last().StartNetwork();

            #endregion
        }
    }
}
