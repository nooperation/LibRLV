using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace LibRLV.EventArguments
{
    public class RemOutfitEventArgs : EventArgs
    {
        public RemOutfitEventArgs(List<UUID> itemIds)
        {
            ItemIds = itemIds;
        }

        public List<UUID> ItemIds { get; set; }
    }
}
