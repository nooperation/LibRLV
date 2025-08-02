using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace LibRLV.EventArguments
{
    public class DetachEventArgs : EventArgs
    {
        public List<UUID> ItemIds { get; }

        public DetachEventArgs(List<UUID> itemIds)
        {
            ItemIds = itemIds;
        }
    }
}
