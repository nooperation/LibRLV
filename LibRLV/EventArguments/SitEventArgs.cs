using System;
using OpenMetaverse;

namespace LibRLV.EventArguments
{
    public class SitEventArgs : EventArgs
    {
        public UUID? Target { get; }

        public SitEventArgs(UUID? target)
        {
            Target = target;
        }
    }
}
