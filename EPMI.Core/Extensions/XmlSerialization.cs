using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace EPMI.Core.Extensions
{
    public static class XmlSerializationExtension
    {
        public static string ToXml(this object data)
        {
            XmlSerializer s = new XmlSerializer(data.GetType());
            using (MemoryStream ms = new MemoryStream())
            using (var writer = new XmlTextWriter(ms, new UTF8Encoding()))
            {
                s.Serialize(writer, data);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
        public static T FromXml<T>(this string data)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(data))
            {
                object obj = s.Deserialize(reader);
                return (T)obj;
            }
        }
        public static T FromXml<T>(this TextReader data)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (TextReader reader = data)
            {
                object obj = s.Deserialize(reader);
                return (T)obj;
            }
        }
    }
}
