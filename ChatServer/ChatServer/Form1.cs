using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServer
{
    public partial class Form1 : Form
    {
        private delegate void UpdateStatus(string strMsg);
        bool connected = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                Application.Restart();
                return;
            }

            if (txtIP.Text==string.Empty)
            {
                MessageBox.Show("IP needed!");
                txtIP.Focus();
                return;
            }
            try
            {
                IPAddress iPAddress = IPAddress.Parse(txtIP.Text);
                int hostPort = (int)numericPort.Value;

                Server mainServer = new Server(iPAddress, hostPort);

                Server.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);

                mainServer.StartService();

                logList.Items.Add("Server online, waiting for users!...");
                logList.SetSelected(logList.Items.Count - 1, true);


            }
            catch (Exception ex)
            {
                logList.Items.Add("Connection error: " + ex.Message);
                logList.SetSelected(logList.Items.Count - 1, true);
                return;
            }

            connected= true;
            txtIP.Enabled= false;
            numericPort.Enabled= false;
            btnStart.ForeColor = Color.Red;
            btnStart.Text = "Exit";
        }

        public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            this.Invoke(new UpdateStatus(this.updateStatus), new object[] { e.EventMessage });
        }
        private void updateStatus(string strMsg)
        {
            logList.Items.Add(strMsg);
            logList.SetSelected(logList.Items.Count - 1, true);
        }
    }
}
