using System;
using System.Collections.Generic;
using System.Linq;

namespace LibRLV
{
    public class InventoryTree
    {

        public class InventoryItem
        {

            public Guid Id { get; set; }
            public InventoryTree Folder { get; set; }
            public Guid? FolderId { get; set; }
            public string Name { get; set; }
            public WearableType? WornOn { get; set; }
            public AttachmentPoint? AttachedTo { get; set; }

            public InventoryItem(Guid id, string name, AttachmentPoint? attachedTo, WearableType? wornOn, Guid? externalFolderId = null)
            {
                Name = name;
                AttachedTo = attachedTo;
                WornOn = wornOn;
                Id = id;
                Folder = null;
                FolderId = externalFolderId;
            }

            public override string ToString()
            {
                return Name ?? Id.ToString();
            }
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public InventoryTree Parent { get; set; }
        public List<InventoryTree> Children { get; set; }
        public List<InventoryItem> Items { get; set; }

        public InventoryTree(Guid id, string name)
        {
            Id = id;
            Name = name;
            Parent = null;
            Children = new List<InventoryTree>();
            Items = new List<InventoryItem>();
        }

        public InventoryTree AddChild(Guid id, string name)
        {
            var newChild = new InventoryTree(id, name)
            {
                Parent = this
            };

            Children.Add(newChild);
            return newChild;
        }

        public InventoryItem AddItem(Guid id, string name, AttachmentPoint? attachedTo, WearableType? wornOn)
        {
            var newItem = new InventoryItem(id, name, attachedTo, wornOn)
            {
                Folder = this,
                FolderId = Id
            };

            return newItem;
        }

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
            return Name ?? Id.ToString();
        }
    }
}
