using System;
using System.Collections.Generic;

namespace LibRLV
{
    public class InventoryFolder
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public InventoryFolder? Parent { get; private set; }
        public IReadOnlyList<InventoryFolder> Children => _children;
        public IReadOnlyList<InventoryItem> Items => _items;

        private readonly List<InventoryFolder> _children;
        private readonly List<InventoryItem> _items;

        public InventoryFolder(Guid id, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }

            Id = id;
            Name = name;
            Parent = null;
            _children = [];
            _items = [];
        }

        /// <summary>
        /// Adds a child folder to this folder
        /// </summary>
        /// <param name="id">Folder ID</param>
        /// <param name="name">Folder Name</param>
        /// <returns>Newly added Folder</returns>
        public InventoryFolder AddChild(Guid id, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }

            var newChild = new InventoryFolder(id, name)
            {
                Parent = this
            };

            _children.Add(newChild);
            return newChild;
        }

        /// <summary>
        /// Adds an item to this folder
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <param name="name">Item Name</param>
        /// <param name="attachedTo">Item attachment point if attached</param>
        /// <param name="wornOn">Item wearable type if worn</param>
        /// <returns>Newly added item</returns>
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

        /// <summary>
        /// Gets a list of items currently worn as the specified wearable type
        /// </summary>
        /// <param name="wearableType">Finds all items worn of this type</param>
        /// <returns>Collection of items that are worn as the specified wearable type</returns>
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

        /// <summary>
        /// Gets a list of items currently attached to the specified attachment point
        /// </summary>
        /// <param name="attachmentPoint">Finds all items attached to this attachment point</param>
        /// <returns>Collection of items currently attached to the specified attachment point</returns>
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
