using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet;

namespace EPMI.ServiceControl.Utility
{
    public class Linux
    {
        public static bool Start(string host, string username, string password)
        {
            using (Renci.SshNet.SshClient client = new Renci.SshNet.SshClient(host, username, password))
            {
                client.Connect();
                client.RunCommand("ping google.com");
                client.Disconnect();
            }
            return false;
        }
    }
}
