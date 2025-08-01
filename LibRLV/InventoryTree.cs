using System.Collections.Generic;
using OpenMetaverse;

namespace LibRLV
{
    public class InventoryTree
    {
        public class InventoryItem
        {
            public UUID Id { get; set; }
            public InventoryTree Folder { get; set; }
            public UUID FolderId { get; set; }
            public string Name { get; set; }
            public WearableType? WornOn { get; set; }
            public AttachmentPoint? AttachedTo { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        public UUID Id { get; set; }
        public string Name { get; set; }
        public InventoryTree Parent { get; set; }
        public List<InventoryTree> Children { get; set; }
        public List<InventoryItem> Items { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
