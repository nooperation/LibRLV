using OpenMetaverse;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LibRLV
{
    public class InventoryMap
    {
        public ImmutableDictionary<UUID, InventoryTree.InventoryItem> Items { get; }
        public ImmutableDictionary<UUID, InventoryTree> Folders { get; }
        public InventoryTree Root { get; }

        public InventoryMap(InventoryTree root)
        {
            var itemsTemp = new Dictionary<UUID, InventoryTree.InventoryItem>();
            var foldersTemp = new Dictionary<UUID, InventoryTree>();
            CreateInventoryMap(root, foldersTemp, itemsTemp);

            Root = root;
            Items = itemsTemp.ToImmutableDictionary();
            Folders = foldersTemp.ToImmutableDictionary();
        }

        public bool TryGetFolderFromPath(string path, bool skipPrivateFolders, out InventoryTree folder)
        {
            var parts = path.Split('/');

            var iter = Root;
            foreach (var part in parts)
            {
                if (skipPrivateFolders && part.StartsWith("."))
                {
                    folder = null;
                    return false;
                }

                iter = iter.Children.FirstOrDefault(n => n.Name == part);
                if (iter == null)
                {
                    folder = null;
                    return false;
                }
            }

            folder = iter;
            return true;
        }

        public string BuildPathToFolder(UUID folderId)
        {
            var path = new Stack<string>();

            if (!Folders.TryGetValue(folderId, out var folder))
            {
                return null;
            }

            var iter = folder;
            while (iter != null)
            {
                // Don't include the root (#RLV) folder itself in the path
                if (iter.Parent == null)
                {
                    break;
                }

                path.Push(iter.Name);
                iter = iter.Parent;
            }

            return string.Join("/", path);
        }

        private static void CreateInventoryMap(InventoryTree root, Dictionary<UUID, InventoryTree> folders, Dictionary<UUID, InventoryTree.InventoryItem> items)
        {
            folders[root.Id] = root;
            foreach (var item in root.Items)
            {
                items[item.Id] = item;
            }

            foreach (var child in root.Children)
            {
                CreateInventoryMap(child, folders, items);
            }
        }
    }
}
