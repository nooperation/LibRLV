using System.Collections.Generic;
using System.Linq;
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


        public void GetWornItems(WearableType wearableType, List<InventoryItem> outWornItems)
        {
            outWornItems.AddRange(Items.Where(n => n.WornOn == wearableType));

            foreach (var item in Children)
            {
                item.GetWornItems(wearableType, outWornItems);
            }
        }

        public void GetAttachedItems(AttachmentPoint attachmentPoint, List<InventoryItem> outAttachedItems)
        {
            outAttachedItems.AddRange(Items.Where(n => n.AttachedTo == attachmentPoint));

            foreach (var item in Children)
            {
                item.GetAttachedItems(attachmentPoint, outAttachedItems);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
