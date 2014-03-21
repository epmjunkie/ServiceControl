using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPMI.ServiceControl.BusinessObjects
{
    public class LogEventArgs : EventArgs
    {
        public string Text { get; set; }
        public LogEventArgs(string text) {
            this.Text = text;
        }
    }
}
