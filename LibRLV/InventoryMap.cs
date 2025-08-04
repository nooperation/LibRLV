using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LibRLV
{
    public class InventoryMap
    {
        public ImmutableDictionary<Guid, InventoryTree.InventoryItem> Items { get; }
        public ImmutableDictionary<Guid, InventoryTree> Folders { get; }
        public InventoryTree Root { get; }

        public InventoryMap(InventoryTree root)
        {
            var itemsTemp = new Dictionary<Guid, InventoryTree.InventoryItem>();
            var foldersTemp = new Dictionary<Guid, InventoryTree>();
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
                var candidateNameLengthSelected = 0;
                var candidatePathRemaining = string.Empty;
                var candidateHasPrefix = false;

                foreach (var child in iter.Children)
                {
                    if (child.Name.Length == 0)
                    {
                        continue;
                    }

                    if (skipPrivateFolders && child.Name[0] == '.')
                    {
                        continue;
                    }

                    var fixedChildName = child.Name;
                    var hasPrefix = false;

                    // Only fix the child name if we don't already have an exact match with path
                    if ((child.Name[0] == '.' || child.Name[0] == '~' || child.Name[0] == '+') &&
                        !path.StartsWith(child.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        fixedChildName = fixedChildName.Substring(1);
                        hasPrefix = true;
                    }

                    if (path.StartsWith(fixedChildName, StringComparison.OrdinalIgnoreCase))
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
                            fixedChildName.Length > candidateNameLengthSelected ||
                            (fixedChildName.Length == candidateNameLengthSelected && !hasPrefix && candidateHasPrefix))
                        {
                            if (path.Length == fixedChildName.Length)
                            {
                                candidatePathRemaining = "";
                                candidate = child;
                                candidateNameLengthSelected = fixedChildName.Length;
                                candidateHasPrefix = hasPrefix;
                                break;
                            }

                            if (path.Length > fixedChildName.Length && path[fixedChildName.Length] == '/')
                            {
                                candidatePathRemaining = path.Substring(fixedChildName.Length + 1);
                                candidate = child;
                                candidateNameLengthSelected = fixedChildName.Length;
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

        public IEnumerable<InventoryTree> FindFoldersContaining(bool limitToOneResult, Guid? itemId, AttachmentPoint? attachmentPoint, WearableType? wearableType)
        {
            var folders = new List<InventoryTree>();

            if (itemId.HasValue)
            {
                if (!Items.TryGetValue(itemId.Value, out var item))
                {
                    return Enumerable.Empty<InventoryTree>();
                }

                if (!item.FolderId.HasValue || !Folders.TryGetValue(item.FolderId.Value, out var folder))
                {
                    return Enumerable.Empty<InventoryTree>();
                }

                folders.Add(folder);
            }
            else if (attachmentPoint.HasValue)
            {
                var foldersContainingAttachments = new HashSet<Guid>();
                foreach (var item in Items.Values)
                {
                    if (item.Folder == null)
                    {
                        // External folders are unknown to RLV
                        continue;
                    }

                    if (item.AttachedTo == attachmentPoint && item.FolderId.HasValue)
                    {
                        if (foldersContainingAttachments.Add(item.FolderId.Value))
                        {
                            folders.Add(item.Folder);

                            if (limitToOneResult)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else if (wearableType.HasValue)
            {
                var foldersIdsContainingWearables = new HashSet<Guid>();
                foreach (var item in Items.Values)
                {
                    if (item.Folder == null)
                    {
                        // External folders are unknown to RLV
                        continue;
                    }

                    if (item.WornOn == wearableType && item.FolderId.HasValue)
                    {
                        if (foldersIdsContainingWearables.Add(item.FolderId.Value))
                        {
                            folders.Add(item.Folder);

                            if (limitToOneResult)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return folders;
        }

        public bool TryBuildPathToFolder(Guid folderId, out string finalPath)
        {
            var path = new Stack<string>();

            if (!Folders.TryGetValue(folderId, out var folder))
            {
                finalPath = null;
                return false;
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

            finalPath = string.Join("/", path);
            return true;
        }

        private static void CreateInventoryMap(InventoryTree root, Dictionary<Guid, InventoryTree> folders, Dictionary<Guid, InventoryTree.InventoryItem> items)
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
