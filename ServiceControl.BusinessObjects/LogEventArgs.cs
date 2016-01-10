using System;

namespace ServiceControl.BusinessObjects
{
    public class LogEventArgs : EventArgs
    {
        public string Text { get; set; }
        public LogEventArgs(string text) {
            this.Text = text;
        }
    }
}
