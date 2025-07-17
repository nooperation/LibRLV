using OpenMetaverse;
using System;

namespace LibRLV.EventArguments
{
    public class AttachmentEventArgs : EventArgs
    {
        public AttachmentEventArgs(string attachmentPointOrClothingLayer)
        {
            if (UUID.TryParse(attachmentPointOrClothingLayer, out UUID uuid))
            {
                this.ItemId = uuid;
                this.AttachmentPointOrClothingLayer = null;
            }
            else
            {
                this.ItemId = null;
                this.AttachmentPointOrClothingLayer = attachmentPointOrClothingLayer;
            }
        }

        public UUID? ItemId { get; }
        public string AttachmentPointOrClothingLayer { get; }
    }
}
