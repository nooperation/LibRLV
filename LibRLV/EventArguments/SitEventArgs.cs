using System;
using OpenMetaverse;

namespace LibRLV.EventArguments
{
    public class SitEventArgs : EventArgs
    {
        public SitEventArgs(UUID? target)
        {
            Target = target;
        }

        public UUID? Target { get; }
    }
}
