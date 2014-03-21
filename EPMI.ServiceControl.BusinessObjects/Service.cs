using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceProcess;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;

namespace EPMI.ServiceControl.BusinessObjects
{
    [XmlRoot(ElementName = "service")]
    [DataContract(Name="service")]
    public class Service : IXmlSerializable, IProfileItem
    {
        #region Event Handlers
        public delegate void LogEventHandler(object sender, LogEventArgs e);
        public delegate void ProgressChanged(object sender, EventArgs e);
        #endregion

        [XmlIgnore]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "name", DataType = "string"), DataMember(IsRequired=true)]
        public string Description { get; set; }
        [XmlAttribute(AttributeName="value", DataType="string"), DataMember(IsRequired=true)]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "isUnix", DataType = "boolean")]
        public bool? isUnix { get; set; }
        [XmlAttribute(AttributeName="isRegEx", DataType="boolean")]
        public bool? isRegEx { get; set; }
        [XmlAttribute(AttributeName="timeOut", DataType="int")]
        public int TimeOut { get; set; }
        [XmlAttribute(AttributeName="startDelay", DataType="int")]
        public int StartDelay { get; set; }
        [XmlAttribute(AttributeName="stopDelay", DataType="int")]
        public int StopDelay { get; set; }
        [XmlIgnore]
        public string Host { get; set; }
        [XmlIgnore]
        public LogEventHandler Log;
        [XmlIgnore]
        public ProgressChanged Progress;
        [XmlIgnore]
        public ServiceControllerStatus Status { get; set; }

        bool IProfileItem.Start()
        {
            try
            {
                return WindowsStart();
            }
            catch (Exception ex) { return false; }
        }
        bool IProfileItem.Stop()
        {
            try
            {
                return WindowsStop();
            }
            catch (Exception ex) { return false; }
        }
        string ToString()
        {
            return this.Name;
        }
        bool UnixStart()
        {

            return false;  
        }
        bool WindowsStart()
        {
            using (ServiceController sc = new ServiceController(this.Value, this.Host))
            {
                OnLog(new LogEventArgs(string.Format("Starting {0} on {1}", this.Name, this.Host)));
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    OnLog(new LogEventArgs(string.Format("Service {0} already running on {1}", this.Name, this.Host)));
                    OnProgress(new EventArgs());
                    return true;
                }
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, this.TimeOut));
                sc.Refresh();
                OnLog(new LogEventArgs(string.Format("[{2}] - {0} - {1}", this.Name, sc.Status, this.Host)));
                OnProgress(new EventArgs());
                Thread.Sleep(this.StartDelay * 1000);
                return sc.Status == ServiceControllerStatus.Running;
            }
        }

        bool WindowsStop()
        {
            using (ServiceController sc = new ServiceController(this.Value, this.Host))
            {
                OnLog(new LogEventArgs(string.Format("Stopping {0} on {1}", this.Name, this.Host)));
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    OnLog(new LogEventArgs(string.Format("Service {0} already stopped on {1}", this.Name, this.Host)));
                    OnProgress(new EventArgs());
                    return true;
                }
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, this.TimeOut));
                sc.Refresh();
                OnLog(new LogEventArgs(string.Format("[{2}] - {0} - {1}", this.Name, sc.Status, this.Host)));
                OnProgress(new EventArgs());
                Thread.Sleep(this.StopDelay * 1000);
                return sc.Status == ServiceControllerStatus.Stopped;
            }
        }
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
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("name", Description);
            writer.WriteAttributeString("value", Value);
            if (isRegEx != null)
            {
                writer.WriteStartAttribute("isRegEx");
                writer.WriteValue(isRegEx);
                writer.WriteEndAttribute();
            }
            if (isUnix != null)
            {
                writer.WriteStartAttribute("isUnix");
                writer.WriteValue(isUnix);
                writer.WriteEndAttribute();
            }
            if (TimeOut != 0)
            {
                writer.WriteStartAttribute("timeOut");
                writer.WriteValue(TimeOut);
                writer.WriteEndAttribute();
            }
            if (StartDelay != 0)
            {
                writer.WriteStartAttribute("startDelay");
                writer.WriteValue(StartDelay);
                writer.WriteEndAttribute();
            }
            if (StopDelay != 0)
            {
                writer.WriteStartAttribute("stopDelay");
                writer.WriteValue(StopDelay);
                writer.WriteEndAttribute();
            }
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
                        case"isRegEx":
                            isRegEx = reader.ReadContentAsBoolean();
                            break;
                        case "isUnix":
                            isUnix = reader.ReadContentAsBoolean();
                            break;
                        case "timeOut":
                            TimeOut = reader.ReadContentAsInt();
                            break;
                        case "startDelay":
                            StartDelay = reader.ReadContentAsInt();
                            break;
                        case "stopDelay":
                            StopDelay = reader.ReadContentAsInt();
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
    }
}
