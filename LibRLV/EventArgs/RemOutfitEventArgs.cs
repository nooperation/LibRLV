using OpenMetaverse;
using System;

namespace LibRLV
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
