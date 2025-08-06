using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibRLV
{
    internal sealed class LockedFolderManager
    {
        private readonly IRLVCallbacks _callbacks;
        private readonly RLVRestrictionManager _restrictionManager;

        private readonly Dictionary<Guid, LockedFolder> _lockedFolders = [];
        private readonly object _lockedFoldersLock = new();

        internal LockedFolderManager(IRLVCallbacks callbacks, RLVRestrictionManager restrictionManager)
        {
            _callbacks = callbacks;
            _restrictionManager = restrictionManager;
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

        public bool TryGetLockedFolder(Guid folderId, [NotNullWhen(true)] out LockedFolderPublic? lockedFolder)
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

        private void AddLockedFolder(InventoryFolder folder, RLVRestriction restriction)
        {
            lock (_lockedFoldersLock)
            {
                if (!_lockedFolders.TryGetValue(folder.Id, out var existingLockedFolder))
                {
                    existingLockedFolder = new LockedFolder(folder);
                    _lockedFolders[folder.Id] = existingLockedFolder;
                }

                if (restriction.Behavior is RLVRestrictionType.DetachAllThis or RLVRestrictionType.DetachThis)
                {
                    existingLockedFolder.DetachRestrictions.Add(restriction);
                }
                else if (restriction.Behavior is RLVRestrictionType.AttachAllThis or RLVRestrictionType.AttachThis)
                {
                    existingLockedFolder.AttachRestrictions.Add(restriction);
                }
                else if (restriction.Behavior is RLVRestrictionType.DetachAllThisExcept or RLVRestrictionType.DetachThisExcept)
                {
                    existingLockedFolder.DetachExceptions.Add(restriction);
                }
                else if (restriction.Behavior is RLVRestrictionType.AttachAllThisExcept or RLVRestrictionType.AttachThisExcept)
                {
                    existingLockedFolder.AttachExceptions.Add(restriction);
                }

                if (restriction.Behavior is RLVRestrictionType.DetachAllThis or
                    RLVRestrictionType.AttachAllThis or
                    RLVRestrictionType.AttachAllThisExcept or
                    RLVRestrictionType.DetachAllThisExcept)
                {
                    foreach (var child in folder.Children)
                    {
                        AddLockedFolder(child, restriction);
                    }
                }
            }
        }

        internal async Task RebuildLockedFolders(CancellationToken cancellationToken)
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

            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync(cancellationToken).ConfigureAwait(false);
            if (!hasSharedFolder || sharedFolder == null)
            {
                return;
            }

            lock (_lockedFoldersLock)
            {
                _lockedFolders.Clear();

                var inventoryMap = new InventoryMap(sharedFolder);

                var detachThisRestrictions = _restrictionManager.GetRestrictionsByType(RLVRestrictionType.DetachThis);
                var detachAllThisRestrictions = _restrictionManager.GetRestrictionsByType(RLVRestrictionType.DetachAllThis);
                var attachThisRestrictions = _restrictionManager.GetRestrictionsByType(RLVRestrictionType.AttachThis);
                var attachAllThisRestrictions = _restrictionManager.GetRestrictionsByType(RLVRestrictionType.AttachAllThis);
                var detachThisExceptions = _restrictionManager.GetRestrictionsByType(RLVRestrictionType.DetachThisExcept);
                var detachAllThisExceptions = _restrictionManager.GetRestrictionsByType(RLVRestrictionType.DetachAllThisExcept);
                var attachThisExceptions = _restrictionManager.GetRestrictionsByType(RLVRestrictionType.AttachThisExcept);
                var attachAllThisExceptions = _restrictionManager.GetRestrictionsByType(RLVRestrictionType.AttachAllThisExcept);

                foreach (var restriction in detachThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, inventoryMap);
                }
                foreach (var restriction in detachAllThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, inventoryMap);
                }
                foreach (var restriction in attachThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, inventoryMap);
                }
                foreach (var restriction in attachAllThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, inventoryMap);
                }
                foreach (var exception in detachThisExceptions)
                {
                    ProcessFolderException(exception, inventoryMap);
                }
                foreach (var exception in detachAllThisExceptions)
                {
                    ProcessFolderException(exception, inventoryMap);
                }
                foreach (var exception in attachThisExceptions)
                {
                    ProcessFolderException(exception, inventoryMap);
                }
                foreach (var exception in attachAllThisExceptions)
                {
                    ProcessFolderException(exception, inventoryMap);
                }
            }
        }

        internal async Task<bool> ProcessFolderException(RLVRestriction restriction, bool isException, CancellationToken cancellationToken)
        {
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync(cancellationToken).ConfigureAwait(false);
            if (!hasSharedFolder || sharedFolder == null)
            {
                return false;
            }

            var inventoryMap = new InventoryMap(sharedFolder);

            if (isException)
            {
                return ProcessFolderException(restriction, inventoryMap);
            }
            else
            {
                return ProcessFolderRestrictions(restriction, sharedFolder, inventoryMap);
            }
        }

        private bool ProcessFolderException(RLVRestriction exception, InventoryMap inventoryMap)
        {
            if (exception.Args.Count == 0)
            {
                return false;
            }
            else if (exception.Args[0] is string path)
            {
                if (!inventoryMap.TryGetFolderFromPath(path, false, out var folder))
                {
                    return false;
                }

                AddLockedFolder(folder, exception);
            }

            return true;
        }

        private bool ProcessFolderRestrictions(RLVRestriction restriction, InventoryFolder sharedFolder, InventoryMap inventoryMap)
        {
            if (restriction.Args.Count == 0)
            {
                if (!inventoryMap.Items.TryGetValue(restriction.Sender, out var item))
                {
                    return false;
                }

                if (!item.FolderId.HasValue || !inventoryMap.Folders.TryGetValue(item.FolderId.Value, out var folder))
                {
                    return false;
                }

                AddLockedFolder(folder, restriction);
            }
            else if (restriction.Args[0] is WearableType wearableType)
            {
                var wornItems = sharedFolder.GetWornItems(wearableType);
                var foldersToLock = wornItems
                    .Where(n => n.Folder != null)
                    .Select(n => n.Folder);

                foreach (var folder in foldersToLock)
                {
                    if (folder == null)
                    {
                        continue;
                    }

                    AddLockedFolder(folder, restriction);
                }
            }
            else if (restriction.Args[0] is AttachmentPoint attachmentPoint)
            {
                var attachedItems = sharedFolder.GetAttachedItems(attachmentPoint);
                var foldersToLock = attachedItems
                    .Where(n => n.Folder != null)
                    .Select(n => n.Folder);

                foreach (var folder in foldersToLock)
                {
                    if (folder == null)
                    {
                        continue;
                    }

                    AddLockedFolder(folder, restriction);
                }
            }
            else if (restriction.Args[0] is string path)
            {
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
