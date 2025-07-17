using OpenMetaverse;
using System;

namespace LibRLV.EventArguments
{
    public class RemOutfitEventArgs : EventArgs
    {
        public RemOutfitEventArgs(WearableType part)
        {
            this.Part = part;
        }

        public WearableType Part { get; }
    }
}
