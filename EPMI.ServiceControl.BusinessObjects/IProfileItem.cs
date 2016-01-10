using System;

namespace ServiceControl.BusinessObjects
{
    public interface IProfileItem
    {
        string Name { get; set; }
        string Value { get; set; }
        bool IsChecked { get; set; }

        string ToString();

        bool Start();
        bool Stop();

        void OnLog(LogEventArgs e);
        void OnProgress(EventArgs e);
    }
}
