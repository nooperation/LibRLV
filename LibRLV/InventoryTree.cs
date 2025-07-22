using OpenMetaverse;
using System.Collections.Generic;

namespace LibRLV
{
    public class InventoryTree
    {
        public class InventoryItem
        {
            public UUID Id { get; set; }
            public UUID FolderId { get; set; }
            public string Name { get; set; }
            public WearableType? WornOn { get; set; }
            public AttachmentPoint? AttachedTo { get; set; }
        }

        public UUID Id { get; set; }
        public string Name { get; set; }
        public InventoryTree Parent { get; set; }
        public List<InventoryTree> Children { get; set; }
        public List<InventoryItem> Items { get; set; }
    }
}
