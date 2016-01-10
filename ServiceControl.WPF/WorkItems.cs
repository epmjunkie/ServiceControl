using BO = ServiceControl.BusinessObjects;

namespace ServiceControl.WPF
{
    public class WorkItems
    {
        public bool Start { get; set; }
        public BO.Profile Profile { get; set; }
        public BO.Profile Hosts { get; set; }
    }
}
