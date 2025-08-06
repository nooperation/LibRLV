using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using LibRLV.EventArguments;

namespace LibRLV
{
    public class RLVCommandProcessor
    {
        private readonly ImmutableDictionary<string, Func<RLVMessage, Task<bool>>> _rlvActionHandlers;

        public event EventHandler<SetRotEventArgs>? SetRot;
        public event EventHandler<AdjustHeightEventArgs>? AdjustHeight;
        public event EventHandler<SetCamFOVEventArgs>? SetCamFOV;
        public event EventHandler<TpToEventArgs>? TpTo;
        public event EventHandler<SitEventArgs>? Sit;
        public event EventHandler? Unsit;
        public event EventHandler? SitGround;
        public event EventHandler<RemOutfitEventArgs>? RemOutfit;
        public event EventHandler<AttachmentEventArgs>? Attach;
        public event EventHandler<DetachEventArgs>? Detach;
        public event EventHandler<SetGroupEventArgs>? SetGroup;
        public event EventHandler<SetSettingEventArgs>? SetEnv;
        public event EventHandler<SetSettingEventArgs>? SetDebug;

        // TODO: Swap manager out with an interface once it's been solidified into only useful stuff
        private readonly RLVPermissionsService _manager;
        private readonly IRLVCallbacks _callbacks;

        internal RLVCommandProcessor(RLVPermissionsService manager, IRLVCallbacks callbacks)
        {
            _manager = manager;
            _callbacks = callbacks;

            _rlvActionHandlers = new Dictionary<string, Func<RLVMessage, Task<bool>>>()
            {
                { "setrot", HandleSetRot },
                { "adjustheight", HandleAdjustHeight},
                { "setcam_fov", HandleSetCamFOV},
                { "tpto", HandleTpTo},
                { "sit", HandleSit},
                { "unsit", HandleUnsit},
                { "sitground", HandleSitGround},
                { "remoutfit", HandleRemOutfit},
                { "detachme", HandleDetachMe},
                { "remattach", HandleRemAttach},
                { "detach", HandleRemAttach},
                { "detachall", HandleDetachAll},
                { "detachthis", n => HandleDetachThis(n, false)},
                { "detachallthis", n => HandleDetachThis(n, true)},
                { "setgroup", HandleSetGroup},
                { "setdebug_", HandleSetDebug},
                { "setenv_", HandleSetEnv},

                { "attach", n => HandleAttach(n, true, false)},
                { "attachall", n => HandleAttach(n, true, true)},
                { "attachover", n => HandleAttach(n, false, false)},
                { "attachallover", n => HandleAttach(n, false, true)},
                { "attachthis", n => HandleAttachThis(n, true, false)},
                { "attachallthis", n => HandleAttachThis(n, true, true)},
                { "attachthisover", n => HandleAttachThis(n, false, false)},
                { "attachallthisover", n => HandleAttachThis(n, false, true)},

                // addoutfit* -> attach* (These are all aliases of their corresponding attach command)
                { "addoutfit", n => HandleAttach(n, true, false)},
                { "addoutfitall", n => HandleAttach(n, true, true)},
                { "addoutfitover", n => HandleAttach(n, false, false)},
                { "addoutfitallover", n => HandleAttach(n, false, true)},
                { "addoutfitthis", n => HandleAttachThis(n, true, false)},
                { "addoutfitallthis", n => HandleAttachThis(n, true, true)},
                { "addoutfitthisover", n => HandleAttachThis(n, false, false)},
                { "addoutfitallthisover", n => HandleAttachThis(n, false, true)},

                // *overorreplace -> *  (These are all aliases of their corresponding attach command)
                { "attachoverorreplace", n => HandleAttach(n, true, false)},
                { "attachalloverorreplace", n => HandleAttach(n, true, true)},
                { "attachthisoverorreplace", n => HandleAttachThis(n, true, false)},
                { "attachallthisoverorreplace", n => HandleAttachThis(n, true, true)},


            }.ToImmutableDictionary();
        }

        internal async Task<bool> ProcessActionCommand(RLVMessage command)
        {
            if (_rlvActionHandlers.TryGetValue(command.Behavior, out var func))
            {
                return await func(command);
            }
            else if (command.Behavior.StartsWith("setdebug_", StringComparison.OrdinalIgnoreCase))
            {
                return await _rlvActionHandlers["setdebug_"](command);
            }
            else if (command.Behavior.StartsWith("setenv_", StringComparison.OrdinalIgnoreCase))
            {
                return await _rlvActionHandlers["setenv_"](command);
            }

            return false;
        }

        private Task<bool> HandleSetDebug(RLVMessage command)
        {
            var separatorIndex = command.Behavior.IndexOf('_');
            if (separatorIndex == -1)
            {
                return Task.FromResult(false);
            }

            var settingName = command.Behavior.Substring(separatorIndex + 1);
            if (settingName.Length == 0)
            {
                return Task.FromResult(false);
            }

            var handler = SetDebug;
            handler?.Invoke(this, new SetSettingEventArgs(settingName, command.Option));

            return Task.FromResult(true);
        }

        private Task<bool> HandleSetEnv(RLVMessage command)
        {
            var separatorIndex = command.Behavior.IndexOf('_');
            if (separatorIndex == -1)
            {
                return Task.FromResult(false);
            }

            var settingName = command.Behavior.Substring(separatorIndex + 1);
            if (settingName.Length == 0)
            {
                return Task.FromResult(false);
            }

            var handler = SetEnv;
            handler?.Invoke(this, new SetSettingEventArgs(settingName, command.Option));

            return Task.FromResult(true);
        }

        private Task<bool> HandleSetGroup(RLVMessage command)
        {
            var argParts = command.Option.Split([';'], StringSplitOptions.RemoveEmptyEntries);
            if (argParts.Length == 0)
            {
                return Task.FromResult(false);
            }

            var groupRole = string.Empty;
            if (argParts.Length > 1)
            {
                groupRole = argParts[1];
            }

            if (Guid.TryParse(argParts[0], out var groupId))
            {
                var handler = SetGroup;
                handler?.Invoke(this, new SetGroupEventArgs(groupId, groupRole));
            }
            else
            {
                var handler = SetGroup;
                handler?.Invoke(this, new SetGroupEventArgs(argParts[0], groupRole));
            }

            return Task.FromResult(true);
        }

        private bool CanRemAttachItem(InventoryItem item, bool enforceNostrip)
        {
            if (item.WornOn == null && item.AttachedTo == null)
            {
                return false;
            }

            if (item.Name.StartsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (enforceNostrip && item.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains("nostrip"))
            {
                return false;
            }

            if (enforceNostrip && item.Folder != null && item.Folder.Name.Contains("nostrip"))
            {
                return false;
            }

            if (!_manager.CanDetach(item, true))
            {
                return false;
            }

            if (item.WornOn is WearableType.Skin or WearableType.Shape or WearableType.Eyes or WearableType.Hair)
            {
                return false;
            }

            return true;
        }

        private static void CollectItemsToAttach(InventoryTree folder, bool replaceExistingAttachments, bool recursive, List<AttachmentEventArgs.AttachmentRequest> itemsToAttach)
        {
            if (folder.Name.Length > 0)
            {
                if (folder.Name.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                if (folder.Name.StartsWith("+", StringComparison.OrdinalIgnoreCase))
                {
                    replaceExistingAttachments = false;
                }
            }

            AttachmentPoint? folderAttachmentPoint = null;
            if (RLVCommon.TryGetAttachmentPointFromItemName(folder.Name, out var attachmentPointTemp))
            {
                folderAttachmentPoint = attachmentPointTemp;
            }

            foreach (var item in folder.Items)
            {
                if (item.AttachedTo != null || item.WornOn != null)
                {
                    continue;
                }

                if (item.Name.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (RLVCommon.TryGetAttachmentPointFromItemName(item.Name, out var attachmentPoint))
                {
                    itemsToAttach.Add(new AttachmentEventArgs.AttachmentRequest(item.Id, attachmentPoint.Value, replaceExistingAttachments));
                }
                else if (folderAttachmentPoint != null)
                {
                    itemsToAttach.Add(new AttachmentEventArgs.AttachmentRequest(item.Id, folderAttachmentPoint.Value, replaceExistingAttachments));
                }
                else
                {
                    itemsToAttach.Add(new AttachmentEventArgs.AttachmentRequest(item.Id, AttachmentPoint.Default, replaceExistingAttachments));
                }
            }

            if (recursive)
            {
                foreach (var child in folder.Children)
                {
                    if (child.Name.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    CollectItemsToAttach(child, replaceExistingAttachments, recursive, itemsToAttach);
                }
            }
        }

        // @attach:[folder]=force
        private async Task<bool> HandleAttach(RLVMessage command, bool replaceExistingAttachments, bool recursive)
        {
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync();
            if (!hasSharedFolder || sharedFolder == null)
            {
                return false;
            }
            var inventoryMap = new InventoryMap(sharedFolder);

            if (!inventoryMap.TryGetFolderFromPath(command.Option, true, out var folder))
            {
                var handler = Attach;
                handler?.Invoke(this, new AttachmentEventArgs([]));

                return false;
            }
            else
            {
                var itemsToAttach = new List<AttachmentEventArgs.AttachmentRequest>();
                CollectItemsToAttach(folder, replaceExistingAttachments, recursive, itemsToAttach);

                var handler = Attach;
                handler?.Invoke(this, new AttachmentEventArgs(itemsToAttach));

                return true;
            }
        }

        private async Task<bool> HandleAttachThis(RLVMessage command, bool replaceExistingAttachments, bool recursive)
        {
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync();
            if (!hasSharedFolder || sharedFolder == null)
            {
                return false;
            }

            var inventoryMap = new InventoryMap(sharedFolder);
            var folderPaths = new List<InventoryTree>();

            if (RLVCommon.RLVWearableTypeMap.TryGetValue(command.Option, out var wearableType))
            {
                var parts = inventoryMap.FindFoldersContaining(false, null, null, wearableType);
                folderPaths.AddRange(parts);
            }
            if (RLVCommon.RLVAttachmentPointMap.TryGetValue(command.Option, out var attachmentPoint))
            {
                var parts = inventoryMap.FindFoldersContaining(false, null, attachmentPoint, null);
                folderPaths.AddRange(parts);
            }
            else if (inventoryMap.TryGetFolderFromPath(command.Option, true, out var folder))
            {
                folderPaths.Add(folder);
            }
            else if (command.Option.Length == 0)
            {
                var parts = inventoryMap.FindFoldersContaining(false, command.Sender, null, null);
                folderPaths.AddRange(parts);
            }

            var itemsToAttach = new List<AttachmentEventArgs.AttachmentRequest>();

            foreach (var item in folderPaths)
            {
                CollectItemsToAttach(item, replaceExistingAttachments, recursive, itemsToAttach);
            }

            var handler = Attach;
            handler?.Invoke(this, new AttachmentEventArgs(itemsToAttach));

            return true;
        }

        private void CollectItemsToDetach(InventoryTree folder, InventoryMap inventoryMap, bool recursive, List<Guid> itemsToDetach)
        {
            if (folder.Name.StartsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (var item in folder.Items)
            {
                if (!CanRemAttachItem(item, true))
                {
                    continue;
                }

                itemsToDetach.Add(item.Id);
            }

            if (recursive)
            {
                foreach (var child in folder.Children)
                {
                    if (child.Name.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    CollectItemsToDetach(child, inventoryMap, recursive, itemsToDetach);
                }
            }
        }

        // @remattach[:<folder|attachpt|uuid>]=force
        // TODO: Add support for Attachment groups (RLVa)
        private async Task<bool> HandleRemAttach(RLVMessage command)
        {
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync();
            if (!hasSharedFolder || sharedFolder == null)
            {
                return false;
            }

            var (hasCurrentOutfit, currentOutfit) = await _callbacks.TryGetCurrentOutfitAsync();
            if (!hasCurrentOutfit || currentOutfit == null)
            {
                return false;
            }

            var inventoryMap = new InventoryMap(sharedFolder);

            var itemIdsToDetach = new List<Guid>();

            if (Guid.TryParse(command.Option, out var uuid))
            {
                var item = currentOutfit.FirstOrDefault(n => n.Id == uuid);
                if (item != null)
                {
                    if (CanRemAttachItem(item, true))
                    {
                        itemIdsToDetach.Add(uuid);
                    }
                }
            }
            else if (inventoryMap.TryGetFolderFromPath(command.Option, true, out var folder))
            {
                CollectItemsToDetach(folder, inventoryMap, false, itemIdsToDetach);
            }
            else if (RLVCommon.RLVAttachmentPointMap.TryGetValue(command.Option, out var attachmentPoint))
            {
                itemIdsToDetach = currentOutfit
                    .Where(n =>
                        n.AttachedTo == attachmentPoint &&
                        CanRemAttachItem(n, true)
                    )
                    .Select(n => n.Id)
                    .Distinct()
                    .ToList();
            }
            else if (command.Option.Length == 0)
            {
                // Everything attachable will be detached (excludes clothing/wearable types)
                itemIdsToDetach = currentOutfit
                    .Where(n =>
                        n.AttachedTo != null && CanRemAttachItem(n, true)
                    )
                    .Select(n => n.Id)
                    .Distinct()
                    .ToList();
            }
            else
            {
                return false;
            }

            var handler = Detach;
            handler?.Invoke(this, new DetachEventArgs(itemIdsToDetach));

            return true;
        }

        private async Task<bool> HandleDetachAll(RLVMessage command)
        {
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync();
            if (!hasSharedFolder || sharedFolder == null)
            {
                return false;
            }
            var inventoryMap = new InventoryMap(sharedFolder);

            if (!inventoryMap.TryGetFolderFromPath(command.Option, true, out var folder))
            {
                return false;
            }

            var itemIdsToDetach = new List<Guid>();
            CollectItemsToDetach(folder, inventoryMap, true, itemIdsToDetach);

            var handler = Detach;
            handler?.Invoke(this, new DetachEventArgs(itemIdsToDetach));

            return true;
        }

        private async Task<bool> HandleDetachThis(RLVMessage command, bool recursive)
        {
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync();
            if (!hasSharedFolder || sharedFolder == null)
            {
                return false;
            }
            var inventoryMap = new InventoryMap(sharedFolder);
            var folderPaths = new List<InventoryTree>();

            if (Guid.TryParse(command.Option, out var uuid))
            {
                if (inventoryMap.Items.TryGetValue(uuid, out var item))
                {
                    if (item.FolderId.HasValue && inventoryMap.Folders.TryGetValue(item.FolderId.Value, out var folder))
                    {
                        folderPaths.Add(folder);
                    }
                }
            }
            else if (RLVCommon.RLVWearableTypeMap.TryGetValue(command.Option, out var wearableType))
            {
                var parts = inventoryMap.FindFoldersContaining(false, null, null, wearableType);
                folderPaths.AddRange(parts);
            }
            else if (RLVCommon.RLVAttachmentPointMap.TryGetValue(command.Option, out var attachmentPoint))
            {
                var parts = inventoryMap.FindFoldersContaining(false, null, attachmentPoint, null);
                folderPaths.AddRange(parts);
            }
            else if (inventoryMap.TryGetFolderFromPath(command.Option, true, out var folder))
            {
                folderPaths.Add(folder);
            }
            else if (command.Option.Length == 0)
            {
                var parts = inventoryMap.FindFoldersContaining(false, command.Sender, null, null);
                folderPaths.AddRange(parts);
            }

            var itemIdsToDetach = new List<Guid>();
            foreach (var item in folderPaths)
            {
                CollectItemsToDetach(item, inventoryMap, recursive, itemIdsToDetach);
            }

            var handler = Detach;
            handler?.Invoke(this, new DetachEventArgs(itemIdsToDetach));

            return true;
        }

        // @detachme=force
        private async Task<bool> HandleDetachMe(RLVMessage command)
        {
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync();
            if (!hasSharedFolder || sharedFolder == null)
            {
                return false;
            }
            var inventoryMap = new InventoryMap(sharedFolder);

            var itemIdsToDetach = new List<Guid>();
            if (inventoryMap.Items.TryGetValue(command.Sender, out var sender))
            {
                if (CanRemAttachItem(sender, false))
                {
                    itemIdsToDetach.Add(sender.Id);
                }
            }

            var handler = Detach;
            handler?.Invoke(this, new DetachEventArgs(itemIdsToDetach));

            return true;
        }

        // @remoutfit[:<folder|layer>]=force
        // TODO: Add support for Attachment groups (RLVa)
        private async Task<bool> HandleRemOutfit(RLVMessage command)
        {
            var (hasCurrentOutfit, currentOutfit) = await _callbacks.TryGetCurrentOutfitAsync();
            if (!hasCurrentOutfit || currentOutfit == null)
            {
                return false;
            }
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync();
            if (!hasSharedFolder || sharedFolder == null)
            {
                return false;
            }

            var inventoryMap = new InventoryMap(sharedFolder);

            Guid? folderId = null;
            WearableType? wearableType = null;

            if (RLVCommon.RLVWearableTypeMap.TryGetValue(command.Option, out var wearableTypeTemp))
            {
                wearableType = wearableTypeTemp;
            }
            else if (inventoryMap.TryGetFolderFromPath(command.Option, true, out var folder))
            {
                folderId = folder.Id;
            }
            else if (command.Option.Length != 0)
            {
                return false;
            }

            var itemsToDetach = currentOutfit
                .Where(n =>
                    n.WornOn != null &&
                    (folderId == null || n.FolderId == folderId) &&
                    (wearableType == null || n.WornOn == wearableType) &&
                    CanRemAttachItem(n, true)
                )
                .ToList();

            var itemIdsToDetach = itemsToDetach
                .Select(n => n.Id)
                .Distinct()
                .ToList();

            var handler = RemOutfit;
            handler?.Invoke(this, new RemOutfitEventArgs(itemIdsToDetach));

            return true;
        }

        private Task<bool> HandleUnsit(RLVMessage command)
        {
            if (!_manager.CanUnsit())
            {
                return Task.FromResult(false);
            }

            var handler = Unsit;
            handler?.Invoke(this, new EventArgs());

            return Task.FromResult(true);
        }

        private Task<bool> HandleSitGround(RLVMessage command)
        {
            if (!_manager.CanSit())
            {
                return Task.FromResult(false);
            }

            var handler = SitGround;
            handler?.Invoke(this, new EventArgs());

            return Task.FromResult(true);
        }

        private Task<bool> HandleSetRot(RLVMessage command)
        {
            if (!float.TryParse(command.Option, out var angleInRadians))
            {
                return Task.FromResult(false);
            }

            var handler = SetRot;
            handler?.Invoke(this, new SetRotEventArgs(angleInRadians));

            return Task.FromResult(true);
        }

        private Task<bool> HandleAdjustHeight(RLVMessage command)
        {
            var args = command.Option.Split([';'], StringSplitOptions.RemoveEmptyEntries);
            if (args.Length < 1)
            {
                return Task.FromResult(false);
            }

            if (!float.TryParse(args[0], out var distance))
            {
                return Task.FromResult(false);
            }

            var factor = 1.0f;
            var deltaInMeters = 0.0f;

            if (args.Length > 1 && !float.TryParse(args[1], out factor))
            {
                factor = 1;
            }

            if (args.Length > 2 && !float.TryParse(args[2], out deltaInMeters))
            {
                deltaInMeters = 0;
            }

            var handler = AdjustHeight;
            handler?.Invoke(this, new AdjustHeightEventArgs(distance, factor, deltaInMeters));

            return Task.FromResult(true);
        }

        private Task<bool> HandleSetCamFOV(RLVMessage command)
        {
            var cameraRestrictions = _manager.GetCameraRestrictions();
            if (cameraRestrictions.IsLocked)
            {
                return Task.FromResult(false);
            }

            if (!float.TryParse(command.Option, out var fov))
            {
                return Task.FromResult(false);
            }

            var handler = SetCamFOV;
            handler?.Invoke(this, new SetCamFOVEventArgs(fov));

            return Task.FromResult(true);
        }

        private async Task<bool> HandleSit(RLVMessage command)
        {
            if (command.Option != string.Empty && !Guid.TryParse(command.Option, out var sitTarget))
            {
                return false;
            }

            if (!_manager.CanSit())
            {
                return false;
            }

            var objectExists = await _callbacks.ObjectExistsAsync(sitTarget);
            if (!objectExists)
            {
                return false;
            }

            var isCurrentlySitting = await _callbacks.IsSittingAsync();
            if (isCurrentlySitting)
            {
                if (!_manager.CanUnsit())
                {
                    return false;
                }

                if (!_manager.CanStandTp())
                {
                    return false;
                }
            }

            var handler = Sit;
            handler?.Invoke(this, new SitEventArgs(sitTarget));

            return true;
        }

        private Task<bool> HandleTpTo(RLVMessage command)
        {
            // @tpto is inhibited by @tploc=n, by @unsit too.
            if (!_manager.CanTpLoc())
            {
                return Task.FromResult(false);
            }
            if (!_manager.CanUnsit())
            {
                return Task.FromResult(false);
            }

            var commandArgs = command.Option.Split([';'], StringSplitOptions.RemoveEmptyEntries);
            var locationArgs = commandArgs[0].Split('/');

            if (locationArgs.Length is < 3 or > 4)
            {
                return Task.FromResult(false);
            }

            float? lookat = null;
            if (commandArgs.Length > 1)
            {
                if (!float.TryParse(commandArgs[1], out var val))
                {
                    return Task.FromResult(false);
                }

                lookat = val;
            }

            if (locationArgs.Length == 3)
            {
                if (!float.TryParse(locationArgs[0], out var x))
                {
                    return Task.FromResult(false);
                }
                if (!float.TryParse(locationArgs[1], out var y))
                {
                    return Task.FromResult(false);
                }
                if (!float.TryParse(locationArgs[2], out var z))
                {
                    return Task.FromResult(false);
                }

                var handler = TpTo;
                handler?.Invoke(this, new TpToEventArgs(x, y, z, null, lookat));

                return Task.FromResult(true);
            }
            else if (locationArgs.Length == 4)
            {
                var regionName = locationArgs[0];

                if (!float.TryParse(locationArgs[1], out var x))
                {
                    return Task.FromResult(false);
                }
                if (!float.TryParse(locationArgs[2], out var y))
                {
                    return Task.FromResult(false);
                }
                if (!float.TryParse(locationArgs[3], out var z))
                {
                    return Task.FromResult(false);
                }

                var handler = TpTo;
                handler?.Invoke(this, new TpToEventArgs(x, y, z, regionName, lookat));

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
