using Renci.SshNet;

namespace ServiceControl.Utility
{
    public class Linux
    {
        public static bool Start(string host, string username, string password)
        {
            using (SshClient client = new SshClient(host, username, password))
            {
                client.Connect();
                client.RunCommand("ping google.com");
                client.Disconnect();
            }
            return false;
        }
    }
}
