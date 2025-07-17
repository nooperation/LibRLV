using OpenMetaverse;
using System;

namespace LibRLV.EventArguments
{
    public class DetachEventArgs : EventArgs
    {
        public DetachEventArgs(UUID? itemID, string attachPointName)
        {
            this.ItemID = itemID;
            this.AttachPointName = attachPointName;
        }

        public UUID? ItemID { get; }
        public string AttachPointName { get; }
    }
}
