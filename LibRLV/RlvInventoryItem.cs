﻿using System;

namespace LibRLV
{
    public class RlvInventoryItem
    {
        public Guid Id { get; }
        public RlvSharedFolder? Folder { get; }
        public Guid? FolderId { get; }
        public string Name { get; set; }
        public RlvWearableType? WornOn { get; set; }
        public RlvAttachmentPoint? AttachedTo { get; set; }
        public Guid? AttachedPrimId { get; set; }

        /// <summary>
        /// Creates an inventory item associated with an external folder
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <param name="name">Item Name</param>
        /// <param name="externalFolderId">ID of the external folder containing this item</param>
        /// <param name="attachedTo">Attachment point if the item is attached</param>
        /// <param name="attachedPrimId">ID of the attached prim</param>
        /// <param name="wornOn">Wearable type if the item is worn</param>
        public RlvInventoryItem(Guid id, string name, Guid? externalFolderId, RlvAttachmentPoint? attachedTo, Guid? attachedPrimId, RlvWearableType? wornOn)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }

            Name = name;
            AttachedTo = attachedTo;
            WornOn = wornOn;
            Id = id;
            Folder = null;
            FolderId = externalFolderId;
            AttachedPrimId = attachedPrimId;
        }

        internal RlvInventoryItem(Guid id, string name, RlvSharedFolder folder, RlvAttachmentPoint? attachedTo, Guid? attachedPrimId, RlvWearableType? wornOn)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }
            if (folder == null)
            {
                throw new ArgumentException("Folder cannot be null", nameof(folder));
            }

            Name = name;
            AttachedTo = attachedTo;
            WornOn = wornOn;
            Id = id;
            Folder = folder ?? throw new ArgumentException("Folder cannot be null", nameof(folder));
            FolderId = folder.Id;
            AttachedPrimId = attachedPrimId;
        }

        public override string ToString()
        {
            return Name ?? Id.ToString();
        }
    }
}
