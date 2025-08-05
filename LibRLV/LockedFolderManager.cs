using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace LibRLV
{
    public class LockedFolderManager
    {
        private readonly IRLVCallbacks _callbacks;
        private readonly RLVRestrictionManager _restrictionManager;

        private readonly Dictionary<Guid, LockedFolder> _lockedFolders = new Dictionary<Guid, LockedFolder>();
        private readonly object _lockedFoldersLock = new object();

        internal LockedFolderManager(IRLVCallbacks callbacks, RLVRestrictionManager restrictionManager)
        {
            _callbacks = callbacks;
            _restrictionManager = restrictionManager;
        }

        private static List<InventoryTree> GetFoldersForItems(ImmutableDictionary<Guid, InventoryTree> rootMap, List<InventoryTree.InventoryItem> items)
        {
            // TODO: What is this - remove?
            var result = new Dictionary<Guid, InventoryTree>();

            foreach (var item in items)
            {
                if (!item.FolderId.HasValue || !rootMap.TryGetValue(item.FolderId.Value, out var folder))
                {
                    continue;
                }

                result[folder.Id] = folder;
            }

            return result.Values.ToList();
        }

        public IReadOnlyDictionary<Guid, LockedFolderPublic> GetLockedFolders()
        {
            lock (_lockedFoldersLock)
            {
                return _lockedFolders
                    .Select(n => new LockedFolderPublic(n.Value))
                    .ToImmutableDictionary(k => k.Id, v => v);
            }
        }

        public bool TryGetLockedFolder(Guid folderId, out LockedFolderPublic lockedFolder)
        {
            lock (_lockedFoldersLock)
            {
                if (_lockedFolders.TryGetValue(folderId, out var lockedFolderPrivate))
                {
                    lockedFolder = new LockedFolderPublic(lockedFolderPrivate);
                    return true;
                }

                lockedFolder = default;
                return false;
            }
        }

        private void AddLockedFolder(InventoryTree folder, RLVRestriction restriction)
        {
            lock (_lockedFoldersLock)
            {
                if (!_lockedFolders.TryGetValue(folder.Id, out var existingLockedFolder))
                {
                    existingLockedFolder = new LockedFolder(folder);
                    _lockedFolders[folder.Id] = existingLockedFolder;
                }

                if (restriction.Behavior == RLVRestrictionType.DetachAllThis || restriction.Behavior == RLVRestrictionType.DetachThis)
                {
                    existingLockedFolder.DetachRestrictions.Add(restriction);
                }
                else if (restriction.Behavior == RLVRestrictionType.AttachAllThis || restriction.Behavior == RLVRestrictionType.AttachThis)
                {
                    existingLockedFolder.AttachRestrictions.Add(restriction);
                }
                else if (restriction.Behavior == RLVRestrictionType.DetachAllThisExcept || restriction.Behavior == RLVRestrictionType.DetachThisExcept)
                {
                    existingLockedFolder.DetachExceptions.Add(restriction);
                }
                else if (restriction.Behavior == RLVRestrictionType.AttachAllThisExcept || restriction.Behavior == RLVRestrictionType.AttachThisExcept)
                {
                    existingLockedFolder.AttachExceptions.Add(restriction);
                }

                if (restriction.Behavior == RLVRestrictionType.DetachAllThis ||
                    restriction.Behavior == RLVRestrictionType.AttachAllThis ||
                    restriction.Behavior == RLVRestrictionType.AttachAllThisExcept ||
                    restriction.Behavior == RLVRestrictionType.DetachAllThisExcept)
                {
                    foreach (var child in folder.Children)
                    {
                        AddLockedFolder(child, restriction);
                    }
                }
            }
        }

        internal async Task RebuildLockedFolders()
        {
            // AttachThis/DetachThis - Only search within the #RLV root
            //  Attachment:
            //      Find attachment object
            //      Get folder object exists in (assuming it exists in the #RLV folder) and add it to the locked folder list
            //
            //  Shared Folder:
            //      Just add the shared folder to the locked folder list
            //
            //  Attachment point / Wearable Type:
            //      Find and all all the folders for all of the attachments in the specified attachment point or of the wearable type.
            //      Add those folders to the locked folder list

            if (!await _callbacks.TryGetRlvInventoryTree(out var sharedFolder))
            {
                return;
            }

            lock (_lockedFoldersLock)
            {
                _lockedFolders.Clear();

                var inventoryMap = new InventoryMap(sharedFolder);

                var detachThisRestrictions = _restrictionManager.GetRestrictions(RLVRestrictionType.DetachThis);
                var detachAllThisRestrictions = _restrictionManager.GetRestrictions(RLVRestrictionType.DetachAllThis);
                var attachThisRestrictions = _restrictionManager.GetRestrictions(RLVRestrictionType.AttachThis);
                var attachAllThisRestrictions = _restrictionManager.GetRestrictions(RLVRestrictionType.AttachAllThis);
                var detachThisExceptions = _restrictionManager.GetRestrictions(RLVRestrictionType.DetachThisExcept);
                var detachAllThisExceptions = _restrictionManager.GetRestrictions(RLVRestrictionType.DetachAllThisExcept);
                var attachThisExceptions = _restrictionManager.GetRestrictions(RLVRestrictionType.AttachThisExcept);
                var attachAllThisExceptions = _restrictionManager.GetRestrictions(RLVRestrictionType.AttachAllThisExcept);

                foreach (var restriction in detachThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, inventoryMap.Folders);
                }
                foreach (var restriction in detachAllThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, inventoryMap.Folders);
                }
                foreach (var restriction in attachThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, inventoryMap.Folders);
                }
                foreach (var restriction in attachAllThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, inventoryMap.Folders);
                }
                foreach (var exception in detachThisExceptions)
                {
                    ProcessFolderException(exception, sharedFolder);
                }
                foreach (var exception in detachAllThisExceptions)
                {
                    ProcessFolderException(exception, sharedFolder);
                }
                foreach (var exception in attachThisExceptions)
                {
                    ProcessFolderException(exception, sharedFolder);
                }
                foreach (var exception in attachAllThisExceptions)
                {
                    ProcessFolderException(exception, sharedFolder);
                }
            }
        }

        internal async Task<bool> ProcessFolderException(RLVRestriction exception)
        {
            if (!await _callbacks.TryGetRlvInventoryTree(out var sharedFolder))
            {
                return false;
            }

            return ProcessFolderException(exception, sharedFolder);
        }

        private bool ProcessFolderException(RLVRestriction exception, InventoryTree sharedFolder)
        {
            if (exception.Args.Count == 0)
            {
                return false;
            }
            else if (exception.Args[0] is string path)
            {
                var inventoryMap = new InventoryMap(sharedFolder);

                if (!inventoryMap.TryGetFolderFromPath(path, false, out var folder))
                {
                    return false;
                }

                AddLockedFolder(folder, exception);
            }

            return true;
        }

        internal async Task<bool> ProcessFolderRestrictions(RLVRestriction restriction)
        {
            if (!await _callbacks.TryGetRlvInventoryTree(out var sharedFolder))
            {
                return false;
            }

            var inventoryMap = new InventoryMap(sharedFolder);
            return ProcessFolderRestrictions(restriction, sharedFolder, inventoryMap.Folders);
        }

        private static bool TryGetItem(Guid itemId, ImmutableDictionary<Guid, InventoryTree> sharedFolderMap, out InventoryTree.InventoryItem outItem)
        {
            foreach (var folder in sharedFolderMap.Values)
            {
                foreach (var item in folder.Items)
                {
                    if (item.Id == itemId)
                    {
                        outItem = item;
                        return true;
                    }
                }
            }

            outItem = null;
            return false;
        }

        private bool ProcessFolderRestrictions(RLVRestriction restriction, InventoryTree sharedFolder, ImmutableDictionary<Guid, InventoryTree> sharedFolderMap)
        {
            if (restriction.Args.Count == 0)
            {
                if (!TryGetItem(restriction.Sender, sharedFolderMap, out var item))
                {
                    return false;
                }

                if (!item.FolderId.HasValue || !sharedFolderMap.TryGetValue(item.FolderId.Value, out var folder))
                {
                    return false;
                }

                AddLockedFolder(folder, restriction);
            }
            else if (restriction.Args[0] is WearableType wearableType)
            {
                var wornItems = new List<InventoryTree.InventoryItem>();

                sharedFolder.GetWornItems(wearableType, wornItems);
                var foldersToLock = GetFoldersForItems(sharedFolderMap, wornItems);

                foreach (var folder in foldersToLock)
                {
                    AddLockedFolder(folder, restriction);
                }
            }
            else if (restriction.Args[0] is AttachmentPoint attachmentPoint)
            {
                var attachedItems = new List<InventoryTree.InventoryItem>();

                sharedFolder.GetAttachedItems(attachmentPoint, attachedItems);
                var foldersToLock = GetFoldersForItems(sharedFolderMap, attachedItems);

                foreach (var folder in foldersToLock)
                {
                    AddLockedFolder(folder, restriction);
                }
            }
            else if (restriction.Args[0] is string path)
            {
                var inventoryMap = new InventoryMap(sharedFolder);

                if (!inventoryMap.TryGetFolderFromPath(path, false, out var folder))
                {
                    return false;
                }

                AddLockedFolder(folder, restriction);
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
