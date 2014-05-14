using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BO = EPMI.ServiceControl.BusinessObjects;

namespace EPMI.ServiceControl.Utility
{
    public class Control
    {
        #region Public Events
        public delegate void LogEventHandler(object sender, BO.LogEventArgs e);
        public event LogEventHandler Log;
        public delegate void ProgressBarEventHandler(object sender, BO.ProgressBarEventArgs e);
        public event ProgressBarEventHandler Progress;
        #endregion

        #region Private Variables
        long _total = 0;
        long _items = 0;
        #endregion

        #region Public Methods
        public void Start(BO.Profile profile, BO.Profile hosts)
        {
            Queue<BO.IProfileItem> queue = new Queue<BO.IProfileItem>();
            GetHosts(hosts, queue);
                GetServices(profile, hosts, queue);
            _total = _items = queue.Count;
            while (queue.Count > 0)
            {
                _items--;
                queue.Dequeue().Start();
            }
        }
        public void Stop(BO.Profile profile, BO.Profile hosts)
        {
            Queue<BO.IProfileItem> queue = new Queue<BO.IProfileItem>();
            GetHosts(hosts, queue);
            GetServices(profile, hosts, queue);
            _total = _items = queue.Count;
            Stack<BO.IProfileItem> stack = new Stack<BO.IProfileItem>();
            while (queue.Count > 0)
                stack.Push(queue.Dequeue()); // reverse queue
            while (stack.Count > 0)
            {
                _items--;
                stack.Pop().Stop();
            }
        }
        #endregion

        #region Private Static Methods
        static void GetServers(BO.Profile hosts)
        {
            foreach (var host in hosts.Where(c => ((BO.Host)c).IsSSH == false))
                Host.GetServices((BO.Host)host);
        }
        #endregion

        #region Private Methods
        Queue<BO.IProfileItem> GetHosts(BO.Profile hosts, Queue<BO.IProfileItem> queue)
        {
            foreach (var host in hosts.Where(c => ((BO.Host)c).IsSSH == true))
            {
                var temp = (BO.Host)host;
                temp.Log = LogHandler;
                    temp.Progress = ProgressChanged;
                queue.Enqueue(temp);
            }
            return queue;
        }
        Queue<BO.IProfileItem> GetServices(BO.Profile profile, BO.Profile hosts, Queue<BO.IProfileItem> queue)
        {
            foreach (BO.Service service in profile)
            {
                GetServers(hosts);
                foreach (BO.Host host in hosts)
                    if (!host.IsSSH)
                        foreach (var item in host.Services)
                            if (IsMatch(service, item))
                            {
                                BO.Service s = new BO.Service
                                {
                                    Host = host.Name,
                                    Name = item.Name,
                                    Value = item.Value,
                                    Description = service.Description,
                                    Status = item.Status,
                                    TimeOut = service.TimeOut,
                                    StartDelay = service.StartDelay
                                };
                                s.Log += LogHandler;
                                s.Progress += ProgressChanged;
                                queue.Enqueue(s);
                            }

            }
            return queue;
        }
        bool IsMatch(BO.Service service, BO.Service item)
        {
            if (!service.isRegEx ?? false) // Is not regex
            {
                if (service.Value == item.Name) // Determine if XML value matches Service Description
                    return true;
                else if (service.Value == item.Value) // Determine if XML value matches Service Name
                    return true;
            }
            else
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(item.Name, service.Value)) // Determine if XML pattern matches Service Description
                    return true;
                else if (System.Text.RegularExpressions.Regex.IsMatch(item.Value, service.Value)) // Determine if XML pattern matches Service Name
                    return true;
            }
            return false;
        }
        #endregion

        #region Event Handlers
        void LogHandler(object sender, BO.LogEventArgs e)
        {
            OnLog(e);
        }
        protected virtual void OnLog(BO.LogEventArgs e)
        {
            if (Log != null)
                Log(this, e);
        }
        void ProgressChanged(object sender, EventArgs e)
        {
            int value = (int)((((double)_total - (double)_items) / (double)_total) * (double)100);
            OnProgress(new BO.ProgressBarEventArgs(value));
        }
        protected virtual void OnProgress(BO.ProgressBarEventArgs e)
        {
            if (Progress != null)
                Progress(this, e);
        }
        #endregion
    }
}
