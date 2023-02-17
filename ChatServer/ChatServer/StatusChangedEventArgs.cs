using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class StatusChangedEventArgs : EventArgs
    {
        private string EventMsg;
        public string EventMessage
        {
            get { return EventMsg; }
            set { EventMsg = value; }
        }

        public StatusChangedEventArgs(string strEventMsg) 
        {
            EventMsg= strEventMsg;
        }

    }
}
