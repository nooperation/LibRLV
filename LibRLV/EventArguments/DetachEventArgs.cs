using System;
using System.Collections.Generic;

namespace LibRLV.EventArguments
{
    public class DetachEventArgs : EventArgs
    {
        public List<Guid> ItemIds { get; }

        public DetachEventArgs(List<Guid> itemIds)
        {
            ItemIds = itemIds;
        }
    }
}
