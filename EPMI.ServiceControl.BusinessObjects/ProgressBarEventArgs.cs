using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPMI.ServiceControl.BusinessObjects
{
    public class ProgressBarEventArgs : EventArgs
    {
        public int Value { get; set; }
        public ProgressBarEventArgs(int value)
        {
            Value = value;
        }
    }
}
