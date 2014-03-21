using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceProcess;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;
using System.Linq.Expressions;

namespace EPMI.ServiceControl.BusinessObjects
{
    [XmlRoot(ElementName="profile", IsNullable=true)]
    [DataContract(Name = "profile")]
    public class Profile : Collection<IProfileItem>, IXmlSerializable
    {
        public string Name { get; set; }
        public bool Default { get; set; }
        public string File { get; set; }

        public override string ToString()
       {
            return this.Name;
        }

        public Profile LoadXML(string file)
        {
            System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
            xml.Load(file);
            System.Xml.XmlNode profile = xml.SelectSingleNode("services");
            this.Name = profile.Attributes["profile"].Value;
            bool _default = false;
            bool.TryParse(profile.Attributes["default"].Value, out _default);
            this.Default = _default;
            System.Xml.XmlNodeList nodes = profile.SelectNodes("service");
            foreach (System.Xml.XmlNode node in nodes)
                if (node.Attributes["value"] != null)
                {
                    Items.Add(new Service
                    {
                        Description = node.Attributes["name"] != null ? node.Attributes["name"].Value : node.Attributes["value"].Value,
                        Value = node.Attributes["value"].Value,
                        TimeOut = node.Attributes["timeout"] != null ? int.Parse(node.Attributes["timeout"].Value) : 0,
                        StartDelay = node.Attributes["start"] != null ? int.Parse(node.Attributes["start"].Value) : 0,
                        StopDelay = node.Attributes["shutdown"] != null ? int.Parse(node.Attributes["shutdown"].Value) : 0
                    });
                }
                else
                {
                    // node has no value....
                }
            return this;
        }
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("name", Name);
            writer.WriteStartAttribute("default");
            writer.WriteValue(Default);
            writer.WriteEndAttribute();
            XmlSerializer serviceSerializer;
            foreach (var item in Items)
            {
                if (item.GetType() == typeof(Host))
                    serviceSerializer = new XmlSerializer(typeof(Host));
                else
                    serviceSerializer = new XmlSerializer(typeof(Service));
                serviceSerializer.Serialize(writer, item);
            }
        }
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToAttribute("name");
            Name = reader.Value;
            reader.MoveToAttribute("default");
            Default = reader.ReadContentAsBoolean();
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "service")
                {
                    var serviceSerializer = new XmlSerializer(typeof(Service));
                    Items.Add((Service)serviceSerializer.Deserialize(reader));
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "host")
                {
                    var hostSerializer = new XmlSerializer(typeof(Host));
                    Items.Add((Host)hostSerializer.Deserialize(reader));
                }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
    }
}
