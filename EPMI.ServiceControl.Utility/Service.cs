using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EPMI.Core.Extensions;
using BO = EPMI.ServiceControl.BusinessObjects;

namespace EPMI.ServiceControl.Utility
{
    public class Service
    {
        #region Public Events
        public delegate void LogEventHandler(object sender, BO.LogEventArgs e);
        public static event LogEventHandler Log;
        #endregion

        #region Private Constants
        const string SAVE_PATH = "{0}\\Profiles\\";
        const string SAVE_FILE = "Custom.service";
        #endregion

        #region Public Properties
        public BO.ProfileCollection Profile { get; set; }
        
        public bool IsLoaded { get; set; }
        #endregion

        public Service()
        {
            IsLoaded = false;
        }
        public Service(bool load) : this()
        {
            if (load)
            Load();
        }

        public void Load()
        {
            IsLoaded = true;
            Profile = new BO.ProfileCollection();
            string path = System.IO.Path.GetFullPath(".");
            foreach (string file in Directory.GetFiles(string.Format(SAVE_PATH, path), "*.service", SearchOption.AllDirectories))
            {
                using (TextReader sr = new StreamReader(file))
                    try
                    {
                        foreach (var profile in sr.FromXml<BO.ProfileCollection>())
                            Profile.Add(profile);
                    }
                    catch(Exception ex) {
                        OnLog(new BO.LogEventArgs(string.Format("Error loading Service Profile file: {0}\r\n{1}", file, ex.Message)));
                    }
            }
        }
        public static void Save(BO.ProfileCollection profile)
        {
            string path = System.IO.Path.GetFullPath(".");
            using (System.IO.TextWriter tw = new System.IO.StreamWriter(string.Format(SAVE_PATH, path)))
                tw.Write(profile.ToXml());
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
