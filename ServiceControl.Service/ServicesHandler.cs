using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BO = ServiceControl.BusinessObjects;
using ServiceControl.Utility;

namespace ServiceControl.Service
{
    internal class ServicesHandler
    {
        Utility.Service Service = new Utility.Service(true);
        Utility.Host Host = new Utility.Host(true);

        internal void Start()
        {
            if (!Service.IsLoaded)
                Service.Load();
            Start(Service.Profile.Default);
        }
        internal void Start(string serviceProfile)
        {
            if (!Service.IsLoaded)
                Service.Load();
            Start(Service.Profile[serviceProfile]);
        }
        internal void Start(BO.Profile serviceProfile)
        {
            if (!Host.IsLoaded)
                Host.Load();
            Start(serviceProfile, Host.Profile.Default);
        }
        internal void Start(BO.Profile services, string hostProfile)
        {
            if (!Host.IsLoaded)
                Host.Load();
            Start(services, Host.Profile[hostProfile]);
        }
        internal void Start(BO.Profile services, BO.Profile hosts)
        {
            Control control = new Control();
            control.Start(services, hosts);
        }
    }
}
