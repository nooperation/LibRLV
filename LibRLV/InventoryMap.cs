using System.Collections.Generic;
using System.Collections.Immutable;
using OpenMetaverse;

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
            var iter = Root;
            while (true)
            {
                InventoryTree candidate = null;
                string candidateNameSelected = null;
                string candidatePathRemaining = null;
                var candidateHasPrefix = false;

                foreach (var child in iter.Children)
                {
                    if (child.Name.StartsWith(".") && skipPrivateFolders)
                    {
                        continue;
                    }

                    var fixedChildName = child.Name;
                    var hasPrefix = false;
                    if (!path.StartsWith(child.Name) && (child.Name.StartsWith(".") || child.Name.StartsWith("~") || child.Name.StartsWith("+")))
                    {
                        fixedChildName = fixedChildName.Substring(1);
                        hasPrefix = true;
                    }

                    if (path.StartsWith(fixedChildName))
                    {
                        // This whole candidate system should probably be redone as a recursive search to find the best possible exact path, but this
                        //   should be good enough for now
                        //
                        // We currently pick the best candidate based on:
                        //  1. The longest candidate that exists at the start of the path and ends with a '/' or matches the remaining path exactly.
                        //     For example, a folder containing "Clothing/Hats" and "Clothing" with a subfolder of "Hats", we would prefer the longest
                        //     match of "Clothing/Hats" first even though in the path they both represent is "#RLV/Clothing/Hats"
                        //
                        //  2. Exact matches are preferred over matches that have the prefix removed, for example if we are searching for a "Clothing"
                        //     folder in a folder that contains "Clothing" and "+Clothing", we prefer the one without the prefix first
                        //
                        //  3. The first exact match is preferred. If there are multiple "Clothing" folders, just pick the first one that appears

                        if (candidate == null ||
                            fixedChildName.Length > candidateNameSelected.Length ||
                            (fixedChildName.Length == candidateNameSelected.Length && !hasPrefix && candidateHasPrefix))
                        {
                            if (path.Length == fixedChildName.Length)
                            {
                                candidatePathRemaining = "";
                                candidate = child;
                                candidateNameSelected = fixedChildName;
                                candidateHasPrefix = hasPrefix;
                                break;
                            }

                            if (path.Length > fixedChildName.Length && path[fixedChildName.Length] == '/')
                            {
                                candidatePathRemaining = path.Substring(fixedChildName.Length + 1);
                                candidate = child;
                                candidateNameSelected = fixedChildName;
                                candidateHasPrefix = hasPrefix;
                            }
                        }
                    }
                }

                if (candidate == null)
                {
                    folder = null;
                    return false;
                }

                path = candidatePathRemaining;
                if (path.Length == 0)
                {
                    folder = candidate;
                    return true;
                }

                iter = candidate;
            }
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
