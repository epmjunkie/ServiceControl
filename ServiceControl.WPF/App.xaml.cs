using System.Windows;
using EPMJunkie.Core.Encryption;

namespace ServiceControl.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            AES.EncryptionKey = "IlikeToDoWhatIDoItsThatVoodoo";
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
