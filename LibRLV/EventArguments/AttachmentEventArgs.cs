using OpenMetaverse;
using System;
using System.Collections.Generic;

namespace LibRLV.EventArguments
{
    public class AttachmentEventArgs : EventArgs
    {
        public class AttachmentRequest
        {
            public UUID ItemId { get; set; }
            public AttachmentPoint AttachmentPoint { get; set; }
            public bool ReplaceExistingAttachments { get; set; }

            public AttachmentRequest(UUID itemId, AttachmentPoint attachmentPoint, bool replaceExistingAttachments)
            {
                this.ItemId = itemId;
                this.AttachmentPoint = attachmentPoint;
                this.ReplaceExistingAttachments = replaceExistingAttachments;
            }

            public override bool Equals(object obj)
            {
                return obj is AttachmentRequest request &&
                       this.ItemId.Equals(request.ItemId) &&
                       this.AttachmentPoint == request.AttachmentPoint &&
                       this.ReplaceExistingAttachments == request.ReplaceExistingAttachments;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.ItemId, this.AttachmentPoint, this.ReplaceExistingAttachments);
            }
        }

        public AttachmentEventArgs(List<AttachmentRequest> itemsToAttach)
        {
            this.ItemsToAttach = itemsToAttach;
        }

        public List<AttachmentRequest> ItemsToAttach { get; set; }
    }
}
