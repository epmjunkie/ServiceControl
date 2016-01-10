using System;

namespace ServiceControl.BusinessObjects
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
