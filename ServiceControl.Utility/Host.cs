﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BO = ServiceControl.BusinessObjects;
using System.Xml;
using System.Xml.Serialization;
using EPMJunkie.Core;
using EPMJunkie.Core.Encryption;
using EPMJunkie.Core.Extensions;

namespace ServiceControl.Utility
{
    public class Host
    {
        #region Public Events
        public delegate void LogEventHandler(object sender, BO.LogEventArgs e);
        public static event LogEventHandler Log;
        #endregion

        #region Private Constants
        const string SAVE_PATH = "{0}\\Profiles\\";
        const string SAVE_FILE = "Default.host";
        #endregion

        #region Public Properties
        public BO.ProfileCollection Profile { get; set; }
        [XmlIgnore]
        public bool IsLoaded { get; set; }
        #endregion

        public Host()
        {
            IsLoaded = false;
        }
        public Host(bool load)
            : this()
        {
            if (load)
            Load();
        }

        public void Load()
        {
            IsLoaded = true;
            Profile = new BO.ProfileCollection();
            string path = System.IO.Path.GetFullPath(".");
            foreach (string file in Directory.GetFiles(string.Format(SAVE_PATH, path), "*.host", SearchOption.AllDirectories))
            {
                using (TextReader sr = new StreamReader(file))
                    try
                    {
                        foreach (var profile in sr.FromXml<BO.ProfileCollection>())
                            Profile.Add(profile);
                    }
                    catch(Exception ex) {
                        OnLog(new BO.LogEventArgs(string.Format("Error loading Host Profile file: {0}\r\n{1}", file, ex.Message)));
                    }
            }
        }

        public void Save()
        {
            string path = System.IO.Path.GetFullPath(".");
            using (System.IO.TextWriter tw = new System.IO.StreamWriter(string.Format(SAVE_PATH, path) + SAVE_FILE))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Profile.ToXml());
                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "  ";
                settings.NewLineChars = "\r\n";
                settings.NewLineHandling = NewLineHandling.Replace;
                using (XmlWriter writer = XmlWriter.Create(sb, settings))
                {
                    doc.Save(writer);
                }
                tw.Write(sb.ToString());
            }
        }

        public void GetServices(BO.Host host)
        {
            host.Services = new List<BO.Service>();
            try
            {
                ImpersonateUser iu = new ImpersonateUser();
                if (!(string.IsNullOrEmpty(host.Username) || string.IsNullOrEmpty(host.Password)))
                    iu.Impersonate(host.Domain, host.Username, AES.DecryptString(host.Password));
                System.ServiceProcess.ServiceController[] sc = System.ServiceProcess.ServiceController.GetServices(host.Value);
                foreach (var s in sc)
                {
                    host.Services.Add(new BO.Service
                    {
                        Name = s.DisplayName,
                        Value = s.ServiceName,
                        Status = s.Status
                    });
                }
                iu.Undo();
            }
            catch (Exception)
            {
                //OnLog(new BO.LogEventArgs(string.Format("Failed to Connect to Server: {0} [{1}]\r\n", host.Name, host.Value )));
            }
        }

        #region Event Handlers
        protected virtual void OnLog(BO.LogEventArgs e)
        {
            if (Log != null)
                Log(this, e);
        }
        #endregion
    }
}
