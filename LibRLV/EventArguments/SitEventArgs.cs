using OpenMetaverse;
using System;

namespace LibRLV.EventArguments
{
    public class SitEventArgs : EventArgs
    {
        public SitEventArgs(UUID? target)
        {
            this.Target = target;
        }

        public UUID? Target { get; }
    }
}
