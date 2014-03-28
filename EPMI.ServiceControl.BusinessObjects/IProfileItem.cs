using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPMI.ServiceControl.BusinessObjects
{
    public interface IProfileItem
    {
        string Name { get; set; }
        string Value { get; set; }

        string ToString();

        bool Start();
        bool Stop();

        void OnLog(LogEventArgs e);
        void OnProgress(EventArgs e);
    }
}
