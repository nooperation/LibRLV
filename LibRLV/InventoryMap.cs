﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LibRLV
{
    public class InventoryMap
    {
        public ImmutableDictionary<Guid, InventoryItem> Items { get; }
        public ImmutableDictionary<Guid, InventoryTree> Folders { get; }
        public InventoryTree Root { get; }

        /// <summary>
        /// Creates a mapping of all items and folders for a given InventoryTree and exposes several
        /// methods for exploring this tree.
        /// </summary>
        /// <param name="root">Root of the tree. Generally the #RLV folder.</param>
        /// <exception cref="ArgumentNullException">root is null</exception>
        public InventoryMap(InventoryTree root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            var itemsTemp = new Dictionary<Guid, InventoryItem>();
            var foldersTemp = new Dictionary<Guid, InventoryTree>();
            CreateInventoryMap(root, foldersTemp, itemsTemp);

            Root = root;
            Items = itemsTemp.ToImmutableDictionary();
            Folders = foldersTemp.ToImmutableDictionary();
        }

        /// <summary>
        /// Attempts to find a folder under the root rlv folder #RLV by the given path.
        /// Folders are not case sensitive. Folders may containing a special prefix (~, +),
        /// which will be treated as if the folder did not have the prefix, unless the path
        /// contains the prefix as well then an exact match will be made.
        /// Example:
        ///     Existing shared folder path: #RLV/Clothing/+Hats/+Fancy
        ///     search term: "clothing/hats/fancy"
        ///     results: The object representing Clothing/+Hats/+Fancy
        /// </summary>
        /// <param name="path">Forward-slash separated folder path. Do not include "#RLV/" as part of the path. Do not start with or end with a forward slash.</param>
        /// <param name="skipPrivateFolders">If true, ignores folders starting with '.'</param>
        /// <param name="folder">The found folder, or null if not found</param>
        /// <returns>True if folder was found, false otherwise</returns>
        public bool TryGetFolderFromPath(string path, bool skipPrivateFolders, out InventoryTree folder)
        {
            if (string.IsNullOrEmpty(path))
            {
                folder = null;
                return false;
            }

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

        /// <summary>
        /// Finds all folders containing the specified itemId, all folders containing an item
        /// that is attached to the specified attachment point, or all folders containing an
        /// item that is worn as the specified wearable type. Only one search criteria may be
        /// specified.
        /// </summary>
        /// <param name="limitToOneResult">Deprecated, should always be false. Returns only the first found folder. This only exists to support the deprecated @GetPath command</param>
        /// <param name="itemId">If specified, find the folder containing this item ID</param>
        /// <param name="attachmentPoint">If specified, find all folders containing an item currently attached to this attachment point</param>
        /// <param name="wearableType">If specified, find all folders containing an item currently worn as this type</param>
        /// <returns>Collection of folders matching the search criteria</returns>
        public IEnumerable<InventoryTree> FindFoldersContaining(
            bool limitToOneResult,
            Guid? itemId,
            AttachmentPoint? attachmentPoint,
            WearableType? wearableType)
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

                    if (item.AttachedTo == attachmentPoint)
                    {
                        if (foldersContainingAttachments.Add(item.Folder.Id))
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

                    if (item.WornOn == wearableType)
                    {
                        if (foldersIdsContainingWearables.Add(item.Folder.Id))
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

        /// <summary>
        /// Attempts to create a path to the specified folder ID
        /// Example result:
        ///     ID of folder (#RLV/Clothing/Hats/Fancy) sets finalPath to "Clothing/Hats/Fancy"
        /// </summary>
        /// <param name="folderId">ID of the folder to get the path to</param>
        /// <param name="finalPath">The path to the folder if function is successful, otherwise null</param>
        /// <returns>True if the folder was found and a path was generated, otherwise false</returns>
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

        private static void CreateInventoryMap(
            InventoryTree root,
            Dictionary<Guid, InventoryTree> folders,
            Dictionary<Guid, InventoryItem> items)
        {
            if (folders.ContainsKey(root.Id))
            {
                return;
            }

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
