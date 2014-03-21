using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ServiceModel;
using EPMI.Core.Extensions;
using System.Runtime.Serialization;
using System.Xml;

namespace EPMI.ServiceControl.BusinessObjects
{
    [XmlRoot(ElementName="profiles", IsNullable=true)]
    [DataContract(Name="profiles")]
    public class ProfileCollection : Collection<Profile>, IXmlSerializable
    {
        public Profile Default
        {
            get
            {
                for (int i = 0; i < Items.Count; i++)
                    if (Items[i].Default == true)
                        return Items[i];
                return null;
            }
        }
        public Profile this[string profile]
        {
            get
            {
                for (int i = 0; i < Items.Count; i++)
                    if (Items[i].Name == profile)
                        return Items[i];
                return null;
            }
        }
        public void WriteXml(XmlWriter writer)
        {
            var profileSerializer = new XmlSerializer(typeof(Profile));
            foreach (var item in Items)
            {
                profileSerializer.Serialize(writer, item);
            }
        }
        public void ReadXml(XmlReader reader)
        {
            XmlDocument doc = new XmlDocument();
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "profiles")
            {
                XmlNode profiles = doc.ReadNode(reader);
                foreach (XmlNode node in profiles.SelectNodes("profile"))
                {
                    Items.Add(node.OuterXml.FromXml<Profile>());
                }
            }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
    }
}
