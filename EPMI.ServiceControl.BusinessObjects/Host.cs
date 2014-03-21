using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;
using System.IO;

namespace EPMI.ServiceControl.BusinessObjects
{
    [Serializable]
    [XmlRoot(ElementName = "host")]
    [DataContract(Name = "host")]
    public class Host : IXmlSerializable, IProfileItem
    {
        #region Event Handlers
        public delegate void LogEventHandler(object sender, LogEventArgs e);
        public delegate void ProgressChanged(object sender, EventArgs e);
        #endregion

        #region Public Properties
        [XmlAttribute(AttributeName="name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName="value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "username")]
        public string Username { get; set; }
        [XmlAttribute(AttributeName = "password")]
        public string Password { get; set; }
        [XmlAttribute(AttributeName="ssh")]
        public bool IsSSH { get; set; }
        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; }
        [XmlAttribute(AttributeName = "command")]
        public string Command { get; set; }
        [XmlIgnore]
        public List<Service> Services { get; set; }
        [XmlIgnore]
        public LogEventHandler Log;
        [XmlIgnore]
        public ProgressChanged Progress;
        #endregion

        #region Private Methods
        bool IProfileItem.Start()
        {
            using (Renci.SshNet.SshClient client = new Renci.SshNet.SshClient(Value, Username, EPMI.Core.Encryption.AES.DecryptString(Password)))
            {
                client.Connect();
                var cmd = client.RunCommand(string.Format("{0}start.sh", Path));
                OnLog(new LogEventArgs(string.Format("Attempting to start all serivices on {0}", Name)));
                client.Disconnect();
            }
            return true;
        }
        bool IProfileItem.Stop()
        {
            using (Renci.SshNet.SshClient client = new Renci.SshNet.SshClient(Value, Username, EPMI.Core.Encryption.AES.DecryptString(Password)))
            {
                client.Connect();
                var cmd = client.RunCommand(string.Format("{0}stop.sh", Path));
                OnLog(new LogEventArgs(string.Format("Attempting to stop all serivices on {0}", Name)));
                client.Disconnect();
            }
            return true;
        }
        #endregion

        #region Public Methods
        public void OnLog(LogEventArgs e)
        {
            if (Log != null)
                Log(this, e);
        }
        public void OnProgress(EventArgs e)
        {
            if (Progress != null)
                Progress(this, e);
        }
        #endregion

        #region Serilization
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("value", Value);
            if (!(string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))
            {
                writer.WriteStartAttribute("ssh");
                writer.WriteValue(IsSSH);
                writer.WriteEndAttribute();
            }
            if (!(string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password)))
            {
                writer.WriteAttributeString("username", Username);
                writer.WriteAttributeString("password", Password);
            }
            if (!string.IsNullOrEmpty(Path))
                writer.WriteAttributeString("path", Path);
            if (!string.IsNullOrEmpty(Command))
                writer.WriteAttributeString("command", Command);
        }
        public void ReadXml(XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                reader.MoveToFirstAttribute();
                do
                {
                    switch (reader.Name)
                    {
                        case "value":
                            Value = reader.Value;
                            break;
                        case "name":
                            Name = reader.Value;
                            break;
                        case "ssh":
                            IsSSH = reader.ReadContentAsBoolean();
                            break;
                        case "username":
                            Username = reader.Value;
                            break;
                        case "password":
                            Password = reader.Value;
                            break;
                        case "path":
                            Path = reader.Value;
                            break;
                        case "command":
                            Command = reader.Value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(string.Format("Unknown attribute {0} with value of {1}", reader.Name, reader.Value));
                    }

                } while (reader.MoveToNextAttribute());
            }
        }
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        #endregion
    }
}
