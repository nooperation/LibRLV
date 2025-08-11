using System;

namespace LibRLV.EventArguments
{
    public class SitEventArgs : EventArgs
    {
        public Guid Target { get; }

        public SitEventArgs(Guid target)
        {
            Target = target;
        }
    }
}
