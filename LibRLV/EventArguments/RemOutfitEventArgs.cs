using OpenMetaverse;
using System;
using System.Collections.Generic;

namespace LibRLV.EventArguments
{
    public class RemOutfitEventArgs : EventArgs
    {
        public RemOutfitEventArgs(List<UUID> itemIds)
        {
            this.ItemIds = itemIds;
        }

        public List<UUID> ItemIds { get; set; }
    }
}
