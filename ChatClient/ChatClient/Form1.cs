using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        private string userName;
        private StreamWriter swSender;
        private StreamReader srReceptor;
        private TcpClient tcpServer;

        private delegate void UpdateLogCallBack(string strMsg);
        private delegate void CloseConnectionCallBack(string strReason);

        private Thread msgThread;
        private IPAddress ipAddress;
        private int hostPort;
        private bool connected;


        public Form1()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();

        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (!connected)
            {
                StartConnection();
            }
            else
            {
                CloseConnection("Disconnected by user");
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            SendMsg();
        }

        private void TxtMsg_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13) //Detect enter
            {
                SendMsg();
            }
        }

        private void StartConnection()
        {
            try
            {
                ipAddress = IPAddress.Parse(txtIP.Text);
                hostPort = (int)numericPort.Value;
                tcpServer = new TcpClient();
                tcpServer.Connect(ipAddress, hostPort);

                connected = true;

                userName = txtUsername.Text;
                txtIP.Enabled = false;
                numericPort.Enabled = false;
                txtUsername.Enabled = false;
                txtMsg.Enabled = true;
                btnSend.Enabled = true;
                btnStart.ForeColor = Color.Red;
                btnStart.Text = "Disconnect";

                swSender = new StreamWriter(tcpServer.GetStream());
                swSender.WriteLine(txtUsername.Text);
                swSender.Flush();

                msgThread = new Thread(new ThreadStart(GetMsgs))
                {
                    IsBackground = true,

                };
                msgThread.Start();


                lblStatus.Invoke(new Action(() =>
                {
                    lblStatus.ForeColor = Color.Green;
                    lblStatus.Text = $"Connected! IP: {ipAddress}:{hostPort}";
                }));

            }
            catch (Exception ex)
            {
                lblStatus.Invoke(new Action(() =>
                {
                    lblStatus.ForeColor = Color.Red;
                    lblStatus.Text = "Error! Couldn't connect to the server...\n" + ex.Message;
                }));
            }
        }

        private void GetMsgs()
        {
            srReceptor = new StreamReader(tcpServer.GetStream());
            string CntReply = srReceptor.ReadLine();

            if (CntReply[0] == '1')
            {
                this.Invoke(new UpdateLogCallBack(this.UpdateLog), new object[] { "Successfully connected!" });
            }
            else
            {
                string reason = "Not connected!";
                reason += CntReply.Substring(2, CntReply.Length - 2);
                this.Invoke(new CloseConnectionCallBack(this.CloseConnection), new object[] { reason });
                return;

            }
            while (connected)
            {
                this.Invoke(new UpdateLogCallBack(this.UpdateLog), new object[] { srReceptor.ReadLine() });
            }
        }

        private void UpdateLog(string strMsg)
        {
            txtLog.AppendText(strMsg + "\r\n");
        }

        private void SendMsg()
        {
            if (txtMsg.Lines.Length >= 1)
            {
                swSender.WriteLine(txtMsg.Text);
                swSender.Flush();
                txtMsg.Lines = null;
            }
            txtMsg.Text = "";
        }
        private void CloseConnection(string strReason)
        {
            txtLog.AppendText(strReason + "\r\n");
            txtIP.Enabled = true;
            numericPort.Enabled = true;
            txtUsername.Enabled = true;
            txtMsg.Enabled = false;
            btnSend.Enabled = false;
            btnStart.ForeColor = Color.Green;
            btnStart.Text = "Connect";

            connected= false;
            swSender.Close();
            srReceptor.Close();
            tcpServer.Close();
            lblStatus.Invoke(new Action(() =>
            {
                lblStatus.ForeColor = Color.Green;
                lblStatus.Text = $"Disconnected!";
            }));
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (connected)
            {
                connected = false;
                swSender.Close();
                srReceptor.Close();
                tcpServer.Close();
                lblStatus.Invoke(new Action(() =>
                {
                    lblStatus.ForeColor = Color.Green;
                    lblStatus.Text = $"Disconnected!";
                }));
            }

        }
    }
}
