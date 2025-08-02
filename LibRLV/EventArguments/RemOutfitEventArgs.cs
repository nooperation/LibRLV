using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace LibRLV.EventArguments
{
    public class RemOutfitEventArgs : EventArgs
    {
        public List<UUID> ItemIds { get; }

        public RemOutfitEventArgs(List<UUID> itemIds)
        {
            ItemIds = itemIds;
        }
    }
}
