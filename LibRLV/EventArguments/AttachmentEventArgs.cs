using System;
using System.Collections.Generic;

namespace LibRLV.EventArguments
{
    public class AttachmentEventArgs : EventArgs
    {
        public class AttachmentRequest
        {
            public Guid ItemId { get; }
            public AttachmentPoint AttachmentPoint { get; }
            public bool ReplaceExistingAttachments { get; }

            public AttachmentRequest(Guid itemId, AttachmentPoint attachmentPoint, bool replaceExistingAttachments)
            {
                ItemId = itemId;
                AttachmentPoint = attachmentPoint;
                ReplaceExistingAttachments = replaceExistingAttachments;
            }

            public override bool Equals(object obj)
            {
                return obj is AttachmentRequest request &&
                       ItemId.Equals(request.ItemId) &&
                       AttachmentPoint == request.AttachmentPoint &&
                       ReplaceExistingAttachments == request.ReplaceExistingAttachments;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(ItemId, AttachmentPoint, ReplaceExistingAttachments);
            }
        }

        public AttachmentEventArgs(List<AttachmentRequest> itemsToAttach)
        {
            ItemsToAttach = itemsToAttach;
        }

        public List<AttachmentRequest> ItemsToAttach { get; }
    }
}
