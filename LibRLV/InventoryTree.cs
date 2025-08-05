using System;
using System.Collections.Generic;

namespace LibRLV
{
    public class InventoryTree
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public InventoryTree Parent { get; private set; }
        public IReadOnlyList<InventoryTree> Children => _children;
        public IReadOnlyList<InventoryItem> Items => _items;

        private readonly List<InventoryTree> _children;
        private readonly List<InventoryItem> _items;

        public InventoryTree(Guid id, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }

            Id = id;
            Name = name;
            Parent = null;
            _children = new List<InventoryTree>();
            _items = new List<InventoryItem>();
        }

        public InventoryTree AddChild(Guid id, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }

            var newChild = new InventoryTree(id, name)
            {
                Parent = this
            };

            _children.Add(newChild);
            return newChild;
        }

        public InventoryItem AddItem(Guid id, string name, AttachmentPoint? attachedTo, WearableType? wornOn)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }

            var newItem = new InventoryItem(id, name, this, attachedTo, wornOn);

            _items.Add(newItem);
            return newItem;
        }

        public IEnumerable<InventoryItem> GetWornItems(WearableType wearableType)
        {
            foreach (var item in Items)
            {
                if (item.WornOn == wearableType)
                {
                    yield return item;
                }
            }

            foreach (var child in Children)
            {
                foreach (var childItem in child.GetWornItems(wearableType))
                {
                    yield return childItem;
                }
            }
        }

        public IEnumerable<InventoryItem> GetAttachedItems(AttachmentPoint attachmentPoint)
        {
            foreach (var item in Items)
            {
                if (item.AttachedTo == attachmentPoint)
                {
                    yield return item;
                }
            }

            foreach (var child in Children)
            {
                foreach (var childItem in child.GetAttachedItems(attachmentPoint))
                {
                    yield return childItem;
                }
            }
        }

        public override string ToString()
        {
            return $"{Name ?? Id.ToString()} (Children: {_children.Count}, Items: {_items.Count})";
        }
    }
}
