using System;

namespace LibRLV
{
    public class InventoryItem
    {
        public Guid Id { get; }
        public InventoryFolder? Folder { get; }
        public Guid? FolderId { get; }
        public string Name { get; set; }
        public WearableType? WornOn { get; set; }
        public AttachmentPoint? AttachedTo { get; set; }

        /// <summary>
        /// Creates an inventory item associated with an external folder
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <param name="name">Item Name</param>
        /// <param name="externalFolderId">ID of the external folder containing this item</param>
        /// <param name="attachedTo">Attachment point if the item is attached</param>
        /// <param name="wornOn">Wearable type if the item is worn</param>
        public InventoryItem(Guid id, string name, Guid? externalFolderId, AttachmentPoint? attachedTo, WearableType? wornOn)
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
        }

        internal InventoryItem(Guid id, string name, InventoryFolder folder, AttachmentPoint? attachedTo, WearableType? wornOn)
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
        }

        public override string ToString()
        {
            return Name ?? Id.ToString();
        }
    }
}
