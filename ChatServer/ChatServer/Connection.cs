using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    internal class Connection
    {
        TcpClient tcpClient;

        private Thread thSender;
        private StreamReader srReceptor;
        private StreamWriter swSender;

        private string currentUser;
        private string strUserReply;

        public Connection(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;

            thSender = new Thread(AcceptClient);
            thSender.IsBackground = true;
            thSender.Start();

        }

        private void CloseConnection()
        {
            tcpClient.Close();
            srReceptor.Close();
            swSender.Close();
        }

        private void AcceptClient()
        {
            srReceptor= new StreamReader(tcpClient.GetStream());
            swSender=new StreamWriter(tcpClient.GetStream());

            currentUser = srReceptor.ReadLine();
            if (currentUser != "")
            {
                if (Server.htUsers.Contains(currentUser)==true)
                {
                    swSender.WriteLine("0|This user already exists!");
                    swSender.Flush();
                    CloseConnection();
                    return;
                } else if (currentUser=="Admin" || currentUser=="Administrator")
                {
                    swSender.WriteLine("0|This username cannot be used!");
                    swSender.Flush();
                    CloseConnection();
                    return;

                }
                else
                {
                    swSender.WriteLine("1");
                    swSender.Flush();

                    Server.IncludeUser(tcpClient,currentUser);
                }
            }
            else
            {
                CloseConnection();
                return;
            }

            try
            {
                while ((strUserReply=srReceptor.ReadLine())!="")
                {
                    if (strUserReply==null)
                    {
                        Server.RemoveUser(tcpClient);

                    }
                    else
                    {
                        Server.SendMsg(currentUser,strUserReply);
                    }
                }
            }
            catch 
            {
                Server.RemoveUser(tcpClient);
            }
        }


    }
}
