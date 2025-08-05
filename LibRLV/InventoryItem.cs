using System;

namespace LibRLV
{
    public class InventoryItem
    {
        public Guid Id { get; }
        public InventoryTree? Folder { get; }
        public Guid? FolderId { get; }
        public string Name { get; set; }
        public WearableType? WornOn { get; set; }
        public AttachmentPoint? AttachedTo { get; set; }

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

        public InventoryItem(Guid id, string name, InventoryTree folder, AttachmentPoint? attachedTo, WearableType? wornOn)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
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
