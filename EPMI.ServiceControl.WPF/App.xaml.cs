using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using EPMI.Core;

namespace EPMI.ServiceControl.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            EPMI.Core.Encryption.AES.EncryptionKey = "IlikeToDoWhatIDoItsThatVoodoo";
            if (e.Args.Length > 0)
                switch (e.Args[0].ToLower())
                {
                    case "start":
                        return;
                    case "stop":
                        return;
                }
            base.OnStartup(e);
        }
    }
}
