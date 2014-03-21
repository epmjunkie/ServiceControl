using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using BO = EPMI.ServiceControl.BusinessObjects;
using EPMI.Core.Extensions;

namespace EPMI.ServiceControl.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker worker = new BackgroundWorker();
        public List<BO.Service> Available { get; set; }
        public Utility.Service Services { get; set; }
        public Utility.Host Hosts { get; set; }
        public Utility.Control Control { get; set; }

        BO.Profile ActiveProfile
        {
            get { return (BO.Profile)cbxService.SelectedItem; }
        }
        BO.Profile ActiveServers
        {
            get { return (BO.Profile)cbxHost.SelectedItem; }
        }

        public MainWindow()
        {
            InitializeComponent();
            Services = new Utility.Service() ;
            Hosts = new Utility.Host();
            LoadHostProfiles();
            LoadServiceProfiles();
            Control = new Utility.Control();
            Control.Log += LogHandler;
            Control.Progress += progressBar1_Update;
            worker.DoWork += ControlServices;
            worker.RunWorkerCompleted += ControlServices_Completed;
        }

        void MenuItem_Host_Add(object sender, RoutedEventArgs e)
        {
            WndAddServer addServer = new WndAddServer();
            addServer.ShowDialog();
            if (addServer.isValid)
                Hosts.Profile[ActiveServers.Name].Add(addServer.Host);
            lbxHosts.Items.Refresh();
            addServer.Reset();
            addServer.Close();
        }

        void MenuItem_Host_Edit(object sender, RoutedEventArgs e)
        {
            WndAddServer editHost = new WndAddServer();
            BO.Host host = (BO.Host)lbxHosts.SelectedItem;
            editHost.Host = host;
            editHost.ShowDialog();
            if (editHost.isValid)
            {
                Hosts.Profile[ActiveServers.Name][Hosts.Profile[ActiveServers.Name].IndexOf((BO.Host)lbxHosts.SelectedItem)] = editHost.Host;
            }
            lbxHosts.Items.Refresh();
            editHost.Reset();
            editHost.Close();
        }
        void MenuItem_Host_Remove(object sender, RoutedEventArgs e)
        {
            Hosts.Profile[ActiveServers.Name].Remove((BO.IProfileItem)lbxHosts.SelectedItem);
            lbxHosts.Items.Refresh();
        }
        void LoadHostProfiles()
        {
            Hosts.Load();
            cbxHost.ItemsSource = Hosts.Profile;
            for (int i = 0; i < Hosts.Profile.Count; i++)
                if (Hosts.Profile[i].Default)
                {
                    cbxHost.SelectedValue = Hosts.Profile[i];
                    break;
                }
        }
        void LoadServiceProfiles()
        {
            Services.Load();
            cbxService.ItemsSource = Services.Profile;
            for (int i = 0; i < Services.Profile.Count; i++)
                if (Services.Profile[i].Default)
                {
                    cbxService.SelectedValue = Services.Profile[i];
                    break;
                }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            //Control.Start(ActiveProfile, ActiveServers);
            if (!worker.IsBusy)
                worker.RunWorkerAsync(new WorkItems { Start = true, Profile = ActiveProfile, Hosts = ActiveServers });
            else
                LogHandler(this, new BO.LogEventArgs("Please wait until current task is completed."));
        }
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            //Control.Stop(ActiveProfile, ActiveServers);
            if (!worker.IsBusy)
                worker.RunWorkerAsync(new WorkItems { Start = false, Profile = ActiveProfile, Hosts = ActiveServers });
            else
                LogHandler(this, new BO.LogEventArgs("Please wait until current task is completed."));
        }
        private void ControlServices(object s, DoWorkEventArgs args)
        {
            WorkItems work = (WorkItems)args.Argument;
            if (work.Start == false)
                Control.Stop(work.Profile, work.Hosts);
            else
                Control.Start(work.Profile, work.Hosts);
        }
        private void ControlServices_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (progressBar1.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
                progressBar1.Value = 0;
            else
                progressBar1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new EventHandler<BO.ProgressBarEventArgs>(progressBar1_Update), sender, new object[] { e });
        }
        void cbxHost_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lbxHosts.ItemsSource = Hosts.Profile[ActiveServers.Name];
        }
        void cbxService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lbxServices.ItemsSource = Services.Profile[ActiveProfile.Name];
        }
        private void BtnMuSave(object sender, RoutedEventArgs e)
        {
            Hosts.Save();
            //Properties.Settings.Default.DefaultProfile = ((BO.Profile)cbxService.SelectedItem).File;
            //System.Collections.Specialized.StringCollection sc = new System.Collections.Specialized.StringCollection();
            //foreach (var item in lbxHosts.Items)
            //    sc.Add(item.ToString());
            //Properties.Settings.Default.Servers = sc;
            //Properties.Settings.Default.Save();
        }
        private void LogHandler(object sender, ServiceControl.BusinessObjects.LogEventArgs e)
        {
            if (tbxLog.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
                tbxLog.AppendText(string.Format("{0}\n", e.Text));
            else
                tbxLog.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new EventHandler<BO.LogEventArgs>(LogHandler), sender, new object[] { e });
        }

        private void tbxLog_TextInput(object sender, TextChangedEventArgs e)
        {
            tbxLog.ScrollToEnd();
        }

        private void progressBar1_Update(object sender, BO.ProgressBarEventArgs e)
        {
            if (progressBar1.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
                progressBar1.Value = e.Value;
            else
                progressBar1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new EventHandler<BO.ProgressBarEventArgs>(progressBar1_Update), sender, new object[] { e });
        }

        private void lbxHosts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxHosts.SelectedItem != null)
                miHost_Edit.IsEnabled = miHost_Remove.IsEnabled = true;
            else
                miHost_Edit.IsEnabled = miHost_Remove.IsEnabled = false;
        }
    }
}
