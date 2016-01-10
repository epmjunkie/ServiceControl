#region References
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.ServiceModel;
using System.Text;
#endregion

namespace ServiceControl.Service
{
    public partial class ServiceControlService : ServiceBase
    {
        static ServiceHost WcfServiceHost = null;

        #region Constructor
        public ServiceControlService()
        {
            InitializeComponent();
        }
        #endregion

        #region Protected Methods
        protected override void OnStart(string[] args)
        {
            StartUp();
        }

        protected override void OnStop()
        {
            ShutDown();
        }
        #endregion

        #region Public Methods
        public void StartUp()
        {
            if (WcfServiceHost != null)
                WcfServiceHost.Close();
            WcfServiceHost = new ServiceHost(typeof(ServiceControl.WCF.Listener));
            WcfServiceHost.Open();
        }
        public void ShutDown()
        {
            if (WcfServiceHost != null)
            {
                WcfServiceHost.Close();
                WcfServiceHost = null;
            }
        }
        #endregion
    }
}
