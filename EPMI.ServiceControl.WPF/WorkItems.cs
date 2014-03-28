using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BO = EPMI.ServiceControl.BusinessObjects;

namespace EPMI.ServiceControl.WPF
{
    public class WorkItems
    {
        public bool Start { get; set; }
        public BO.Profile Profile { get; set; }
        public BO.Profile Hosts { get; set; }
    }
}
