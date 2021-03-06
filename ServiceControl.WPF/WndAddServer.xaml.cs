﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using BO = ServiceControl.BusinessObjects;
using Ssh = Renci.SshNet;
using System.Xml;
using System.Text.RegularExpressions;
using EPMJunkie.Core.Encryption;

namespace ServiceControl.WPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class WndAddServer : Window
    {
        string Password { get; set; }
        bool? IsChecked { get; set; }
        public bool isValid
        {
            get
            {
                if (string.IsNullOrEmpty(tbxHost.Text) || string.IsNullOrEmpty(tbxName.Text))
                    return false;
                return true;
            }
        }
        public BO.Host Host
        {
            get
            {
                return new BO.Host
                {
                    Name = tbxName.Text,
                    Value = tbxHost.Text,
                    IsSSH = IsSSH.IsChecked ?? false,
                    Username = tbxUsername.Text,
                    Password = (string.IsNullOrEmpty(tbxPassword.Password) ? Password : AES.EncryptString(tbxPassword.Password)),
                    Domain = tbxDomain.Text,
                    Path = tbxUnixPath.Text,
                    IsChecked = IsChecked ?? true
                };
            }
            set
            {
                tbxName.Text = value.Name;
                tbxHost.Text = value.Value;
                IsSSH.IsChecked = value.IsSSH;
                tbxUsername.Text = value.Username;
                Password = value.Password;
                tbxDomain.Text = value.Domain;
                tbxUnixPath.Text = value.Path;
                IsChecked = value.IsChecked;
            }
        }

        public WndAddServer()
        {
            InitializeComponent();
            tbxName.Focus();
        }
        public void Reset()
        {
            this.Host = new BO.Host();
            tbxPassword.Password = Password = string.Empty;
        }

        void tbxHost_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Save();
            else if (e.Key == Key.Escape)
                Cancel();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        void Save()
        {
            this.Close();
        }
        void Cancel()
        {
            Reset();
            this.Close();
        }

        private void tbxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                tbxHost.Focus();
            else if (e.Key == Key.Escape)
                Cancel();
        }

        private void btnUnixDetect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var KeyboardInteractive = new Ssh.KeyboardInteractiveAuthenticationMethod(Host.Username);
                var Password = new Ssh.PasswordAuthenticationMethod(Host.Username, AES.DecryptString(Host.Password));
                var encryptedPassword = Host.Password;
                KeyboardInteractive.AuthenticationPrompt += delegate(object sender1, Ssh.Common.AuthenticationPromptEventArgs e1)
                {
                    foreach (var prompt in e1.Prompts)
                        if (prompt.Request.ToLower().Contains("password"))
                            prompt.Response = AES.DecryptString(encryptedPassword);
                };
                var conn = new Ssh.ConnectionInfo(Host.Value,
                    Host.Username,
                    Password,
               KeyboardInteractive);
                using (Ssh.SshClient client = new Ssh.SshClient(conn))
                {
                    client.Connect();
                    var termdic = new Dictionary<Ssh.Common.TerminalModes, uint>();
                    termdic.Add(Ssh.Common.TerminalModes.ECHO, 0);

                    using (var shell = client.CreateShellStream("gogrid", 80, 24, 800, 600, 1024, termdic))
                    {
                        using (var output = new StreamReader(shell))
                        using (var input = new StreamWriter(shell))
                        {
                            input.AutoFlush = true;
                            while (shell.Length == 0)
                            {
                                Thread.Sleep(500);
                            }
                            //shell.WriteLine("stty raw -echo"); // disable echo
                            while (shell.Length != 0) { shell.Read(); }
                            shell.Write("([ -d ~/oraInventory/ContentsXML/ ] && [ -e ~/oraInventory/ContentsXML/inventory.xml ])  && echo epmi1 || echo epmi0\n");
                            while (shell.Length == 0)
                            {
                                Thread.Sleep(500);
                            }
                            var resp = shell.ReadLine();
                            while (shell.Length != 0) { shell.Read(); }
                            if (System.Text.RegularExpressions.Regex.IsMatch(resp, "epmi1$"))
                            {
                                shell.Write("cat ~/oraInventory/ContentsXML/inventory.xml\n");
                                while (shell.Length == 0)
                                {
                                    Thread.Sleep(500);
                                }
                                resp = Read(output, true);
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(resp);
                                var nodes = doc.SelectNodes("INVENTORY/HOME_LIST/HOME");
                                for (int i = 0; i < nodes.Count; i++)
                                    if (Regex.IsMatch(nodes[i].Attributes["NAME"].Value, @"EpmSystem_\S+"))
                                    {
                                        tbxUnixPath.Text = nodes[i].Attributes["LOC"].Value;
                                        break;
                                    }
                                MessageBox.Show("Success");
                            }
                        }
                    }
                    client.Disconnect();
                }
            }
            catch (Ssh.Common.SshAuthenticationException)
            {
                MessageBox.Show("Failed to authenticate to server. Check username and password.");
            }
            catch (Exception)
            {
                MessageBox.Show("Unknown error.");
            }
        }
        private static string Read(StreamReader reader)
        {
            return Read(reader, false);
        }
        private static string Read(StreamReader reader, bool singleline)
        {
            var response = reader.ReadToEnd();
            string[] lines = response.Split('\n');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lines.Length - 1; i++)
                if (singleline)
                    sb.Append(lines[i].Trim());
                else
                    sb.Append(lines[i]);
            return sb.ToString();
        }
    }
}
