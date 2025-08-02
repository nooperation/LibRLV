using System;
using System.Collections.Generic;

namespace LibRLV.EventArguments
{
    public class RemOutfitEventArgs : EventArgs
    {
        public List<Guid> ItemIds { get; }

        public RemOutfitEventArgs(List<Guid> itemIds)
        {
            ItemIds = itemIds;
        }
    }
}
