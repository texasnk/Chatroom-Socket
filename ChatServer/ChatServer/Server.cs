using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
    internal class Server
    {
        public static Hashtable htUsers = new Hashtable(30);
        public static Hashtable htConnections = new Hashtable(30);

        private IPAddress ipAddress;
        private int hostPort;
        private TcpClient tcpClient;

        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;

        public Server(IPAddress address, int port)
        {
            ipAddress= address;
            hostPort= port;


        }

        private Thread thListener;
        private TcpListener tcClient;

        bool serverWoking = false;

        public static void IncludeUser(TcpClient tcpUser, string strUsername)
        { 
            Server.htUsers.Add(strUsername, tcpUser);
            Server.htConnections.Add(tcpUser, strUsername);

            MsgAdmin(htConnections[tcpUser] + " connected!");
        }

        public static void RemoveUser(TcpClient tcpUser)
        {
            if (htConnections[tcpUser] != null)
            {
                MsgAdmin(htConnections[tcpUser] + " disconnected!");
                Server.htUsers.Remove(Server.htConnections[tcpUser]);
                Server.htConnections.Remove(tcpUser);
            }
        }

        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChangedEventHandler statusHandler = StatusChanged;
            if (statusHandler != null)
            {
                statusHandler(null, e); 
            }
        }

        public static void MsgAdmin(string msg)
        {
            StreamWriter swSender;
            e = new StatusChangedEventArgs("Admin: " + msg);
            OnStatusChanged(e);

            TcpClient[] tcpClients = new TcpClient[Server.htUsers.Count];
            Server.htUsers.Values.CopyTo(tcpClients, 0);

            for (int i = 0; i < tcpClients.Length; i++)
            {
                try
                {
                    if (msg.Trim() == "" || htUsers[i]==null)
                    {
                        continue;
                    }

                    swSender = new StreamWriter(tcpClients[i].GetStream());
                    swSender.WriteLine("Admin: " + msg);
                    swSender.Flush();
                    swSender = null;
                }
                catch
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        public static void SendMsg(string Source, string msg)
        {
            StreamWriter swSender;

            e = new StatusChangedEventArgs(Source + ": " + msg);
            OnStatusChanged(e);

            TcpClient[] tcpClients = new TcpClient[Server.htUsers.Count];
            Server.htUsers.Values.CopyTo(tcpClients, 0);

            for (int i = 0; i < tcpClients.Length; i++)
            {
                try
                {
                    if (msg.Trim() == "" || htUsers[i] == null)
                    {
                        continue;
                    }

                    swSender = new StreamWriter(tcpClients[i].GetStream());
                    swSender.WriteLine(Source + " Admin: " + msg);
                    swSender.Flush();
                    swSender = null;
                }
                catch
                {
                    RemoveUser(tcpClients[i]);
                }
            }

        }

        public void StartService()
        {
            try
            {
                IPAddress localIp = ipAddress;
                int localPort = hostPort;

                tcClient = new TcpListener(localIp, localPort);

                tcClient.Start();

                serverWoking= true;

                thListener = new Thread(KeepService);
                thListener.IsBackground = true;
                thListener.Start();

            }
            catch (Exception ex)
            {

            }

        }

        public void KeepService()
        {
            while (serverWoking) {
                tcpClient = tcClient.AcceptTcpClient();
                Connection newConnection = new Connection(tcpClient);
            
            }
        }

    }
}
