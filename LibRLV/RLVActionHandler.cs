using LibRLV.EventArguments;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static LibRLV.InventoryTree;

namespace LibRLV
{
    public class RLVActionHandler
    {
        private readonly ImmutableDictionary<string, Func<RLVMessage, bool>> RLVActionHandlers;

        public event EventHandler<SetRotEventArgs> SetRot;
        public event EventHandler<AdjustHeightEventArgs> AdjustHeight;
        public event EventHandler<SetCamFOVEventArgs> SetCamFOV;
        public event EventHandler<TpToEventArgs> TpTo;
        public event EventHandler<SitEventArgs> Sit;
        public event EventHandler Unsit;
        public event EventHandler SitGround;
        public event EventHandler<RemOutfitEventArgs> RemOutfit;
        public event EventHandler DetachMe;
        public event EventHandler<AttachmentEventArgs> Attach;
        public event EventHandler<InventoryPathEventArgs> AttachOver;
        public event EventHandler<InventoryPathEventArgs> AttachAll;
        public event EventHandler<InventoryPathEventArgs> AttachAllOverOrReplace;
        public event EventHandler<DetachEventArgs> Detach;
        public event EventHandler<InventoryPathEventArgs> DetachAll;
        public event EventHandler<AttachmentEventArgs> AttachThis;
        public event EventHandler<AttachmentEventArgs> AttachThisOver;
        public event EventHandler<AttachmentEventArgs> AttachThisOverOrReplace;
        public event EventHandler<AttachmentEventArgs> AttachAllThis;
        public event EventHandler<AttachmentEventArgs> AttachAllThisOver;
        public event EventHandler<AttachmentEventArgs> AttachAllThisOverOrReplace;
        public event EventHandler<AttachmentEventArgs> DetachThis;
        public event EventHandler<AttachmentEventArgs> DetachAllThis;
        public event EventHandler<SetGroupEventArgs> SetGroup;
        public event EventHandler<SetSettingEventArgs> SetEnv;
        public event EventHandler<SetSettingEventArgs> SetDebug;

        // TODO: Swap manager out with an interface once it's been solidified into only useful stuff
        RLVManager _manager;
        IRLVCallbacks _callbacks;

        public RLVActionHandler(RLVManager manager, IRLVCallbacks callbacks)
        {
            _manager = manager;
            _callbacks = callbacks;

            RLVActionHandlers = new Dictionary<string, Func<RLVMessage, bool>>()
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
                { "attach", n => HandleAttach(n, true, false)},
                { "attachover", n => HandleAttach(n, false, false)},
                { "attachoverorreplace", n => HandleAttach(n, true, false)},
                { "attachall", n => HandleAttach(n, true, true)},
                { "attachallover", n => HandleAttach(n, false, true)},
                { "attachalloverorreplace", n => HandleAttach(n, true, true)},
                { "detach", HandleRemAttach},
                { "remattach", HandleRemAttach},
                { "detachall", n => HandleInventoryThing(n, DetachAll)},
                { "attachthis", n => HandleAttachThis(n, true, false)},
                { "attachthisover", n => HandleAttachThis(n, false, false)},
                { "attachthisoverorreplace", n => HandleAttachThis(n, true, false)},
                { "attachallthis", n => HandleAttachThis(n, true, true)},
                { "attachallthisover", n => HandleAttachThis(n, false, true)},
                { "attachallthisoverorreplace", n => HandleAttachThis(n, true, true)},
                { "detachthis", n => HandleAttachmentThing(n, DetachThis)},
                { "detachallthis", n => HandleAttachmentThing(n, DetachAllThis)},
                { "setgroup", HandleSetGroup},
                { "setdebug_", HandleSetDebug},
                { "setenv_", HandleSetEnv},
            }.ToImmutableDictionary();
        }

        internal bool ProcessActionCommand(RLVMessage command)
        {
            if (RLVActionHandlers.TryGetValue(command.Behavior, out var func))
            {
                return func(command);
            }
            else if (command.Behavior.StartsWith("setdebug_"))
            {
                return RLVActionHandlers["setdebug_"](command);
            }
            else if (command.Behavior.StartsWith("setenv_"))
            {
                return RLVActionHandlers["setenv_"](command);
            }

            return false;
        }

        private bool HandleSetDebug(RLVMessage command)
        {
            var separatorIndex = command.Behavior.IndexOf('_');
            if (separatorIndex == -1)
            {
                return false;
            }

            var settingName = command.Behavior.Substring(separatorIndex + 1);
            if (settingName.Length == 0)
            {
                return false;
            }

            SetDebug?.Invoke(this, new SetSettingEventArgs(settingName, command.Option));

            return true;
        }

        private bool HandleSetEnv(RLVMessage command)
        {
            var separatorIndex = command.Behavior.IndexOf('_');
            if (separatorIndex == -1)
            {
                return false;
            }

            var settingName = command.Behavior.Substring(separatorIndex + 1);
            if (settingName.Length == 0)
            {
                return false;
            }

            SetEnv?.Invoke(this, new SetSettingEventArgs(settingName, command.Option));

            return true;
        }

        private bool HandleSetGroup(RLVMessage command)
        {
            if (UUID.TryParse(command.Option, out UUID groupId))
            {
                SetGroup?.Invoke(this, new SetGroupEventArgs(groupId));
            }
            else
            {
                SetGroup?.Invoke(this, new SetGroupEventArgs(command.Option));
            }

            return true;
        }

        private bool HandleInventoryThing(RLVMessage command, EventHandler<InventoryPathEventArgs> handler)
        {
            handler?.Invoke(this, new InventoryPathEventArgs(command.Option));
            return true;
        }

        private bool HandleAttachmentThing(RLVMessage command, EventHandler<AttachmentEventArgs> handler)
        {
            //handler?.Invoke(this, new AttachmentEventArgs(command.Option));
            return true;
        }

        private bool CanRemAttachItem(InventoryItem item, InventoryMap inventoryMap, bool enforceNostrip)
        {
            if (enforceNostrip && item.Name.ToLower().Contains("nostrip"))
            {
                return false;
            }

            if (!inventoryMap.Folders.TryGetValue(item.FolderId, out var folder))
            {
                while (folder != null)
                {
                    if (enforceNostrip && folder.Name.Contains("nostrip"))
                    {
                        return false;
                    }
                    folder = folder.Parent;
                }
            }

            if (!_manager.CanDetach(item, true))
            {
                return false;
            }

            if (item.WornOn == WearableType.Skin || item.WornOn == WearableType.Shape || item.WornOn == WearableType.Eyes || item.WornOn == WearableType.Hair)
            {
                return false;
            }

            return true;
        }

        private void CollectItemsToAttach(InventoryTree folder, bool replaceExistingAttachments, bool recursive, List<AttachmentEventArgs.AttachmentRequest> itemsToAttach)
        {
            AttachmentPoint? folderAttachmentPoint = null;
            if (RLVCommon.TryGetAttachmentPointFromItemName(folder.Name, out var attachmentPointTemp))
            {
                folderAttachmentPoint = attachmentPointTemp;
            }

            if (folder.Name.StartsWith("+"))
            {
                replaceExistingAttachments = false;
            }

            foreach (var item in folder.Items)
            {
                if (item.AttachedTo != null || item.WornOn != null)
                {
                    continue;
                }

                if (item.Name.StartsWith("."))
                {
                    continue;
                }

                if (RLVCommon.TryGetAttachmentPointFromItemName(item.Name, out var attachmentPoint))
                {
                    itemsToAttach.Add(new AttachmentEventArgs.AttachmentRequest(item.Id, attachmentPoint, replaceExistingAttachments));
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
                    if (child.Name.StartsWith("."))
                    {
                        continue;
                    }

                    CollectItemsToAttach(child, replaceExistingAttachments, recursive, itemsToAttach);
                }
            }
        }

        // @attach:[folder]=force
        private bool HandleAttach(RLVMessage command, bool replaceExistingAttachments, bool recursive)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return false;
            }
            var inventoryMap = new InventoryMap(sharedFolder);

            if (!inventoryMap.TryGetFolderFromPath(command.Option, true, out var folder))
            {
                Attach?.Invoke(this, new AttachmentEventArgs(new List<AttachmentEventArgs.AttachmentRequest>()));
                return false;
            }

            var itemsToAttach = new List<AttachmentEventArgs.AttachmentRequest>();
            CollectItemsToAttach(folder, replaceExistingAttachments, recursive, itemsToAttach);

            Attach?.Invoke(this, new AttachmentEventArgs(itemsToAttach));
            return true;
        }

        // TODO: This is copy-pasted from RLVGetHandler::HandleGetPath - This logic needs to be shared somewhere
        private class FolderPaths
        {
            public InventoryTree Folder { get; set; }
            public string SharedPath { get; set; }
        }
        private List<FolderPaths> GetPath_Internal(bool limitToOneResult, UUID? itemId, AttachmentPoint? attachmentPoint, WearableType? wearableType)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return new List<FolderPaths>();
            }

            var inventoryMap = new InventoryMap(sharedFolder);
            var folders = new List<FolderPaths>();

            if (itemId != null)
            {
                if (!inventoryMap.Items.TryGetValue(itemId.Value, out var item))
                {
                    return new List<FolderPaths>();
                }

                if (!inventoryMap.Folders.TryGetValue(item.FolderId, out var folder))
                {
                    return new List<FolderPaths>();
                }

                folders.Add(new FolderPaths()
                {
                    Folder = folder,
                    SharedPath = ""
                });
            }
            else if (attachmentPoint != null)
            {
                var folderIds = inventoryMap.Items.Values
                    .Where(n => n.AttachedTo == attachmentPoint)
                    .Select(n => n.FolderId)
                    .Distinct()
                    .ToList();

                var foundFolders = inventoryMap
                    .Folders
                    .Where(n => folderIds.Contains(n.Key))
                    .Select(n => n.Value);

                if (limitToOneResult)
                {
                    var foundFolder = foundFolders.FirstOrDefault();
                    if (foundFolder != null)
                    {
                        folders.Add(new FolderPaths()
                        {
                            Folder = foundFolder,
                            SharedPath = ""
                        });
                    }
                }
                else
                {
                    folders.AddRange(foundFolders.Select(n => new FolderPaths()
                    {
                        Folder = n,
                        SharedPath = ""
                    }));
                }
            }
            else if (wearableType != null)
            {
                var folderIds = inventoryMap.Items.Values
                    .Where(n => n.WornOn == wearableType)
                    .Select(n => n.FolderId)
                    .Distinct()
                    .ToList();

                var foundFolders = inventoryMap
                    .Folders
                    .Where(n => folderIds.Contains(n.Key))
                    .Select(n => n.Value);

                if (limitToOneResult)
                {
                    var foundFolder = foundFolders.FirstOrDefault();
                    if (foundFolder != null)
                    {
                        folders.Add(new FolderPaths()
                        {
                            Folder = foundFolder,
                            SharedPath = ""
                        });
                    }
                }
                else
                {
                    folders.AddRange(foundFolders.Select(n => new FolderPaths()
                    {
                        Folder = n,
                        SharedPath = ""
                    }));
                }
            }

            foreach (var item in folders)
            {
                item.SharedPath = inventoryMap.BuildPathToFolder(item.Folder.Id);
            }

            return folders;
        }

        private bool HandleAttachThis(RLVMessage command, bool replaceExistingAttachments, bool recursive)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return false;
            }
            var inventoryMap = new InventoryMap(sharedFolder);
            var folderPaths = new List<InventoryTree>();

            if (RLVCommon.RLVWearableTypeMap.TryGetValue(command.Option, out var wearableType))
            {
                var parts = GetPath_Internal(false, null, null, wearableType);
                folderPaths.AddRange(parts.Select(n => n.Folder));
            }
            if (RLVCommon.RLVAttachmentPointMap.TryGetValue(command.Option, out var attachmentPoint))
            {
                var parts = GetPath_Internal(false, null, attachmentPoint, null);
                folderPaths.AddRange(parts.Select(n => n.Folder));
            }
            else if (inventoryMap.TryGetFolderFromPath(command.Option, true, out var folder))
            {
                folderPaths.Add(folder);
            }
            else if (command.Option.Length == 0)
            {
                var parts = GetPath_Internal(false, command.Sender, null, null);
                folderPaths.AddRange(parts.Select(n => n.Folder));
            }

            var itemsToAttach = new List<AttachmentEventArgs.AttachmentRequest>();

            foreach (var item in folderPaths)
            {
                CollectItemsToAttach(item, replaceExistingAttachments, recursive, itemsToAttach);
            }

            Attach?.Invoke(this, new AttachmentEventArgs(itemsToAttach));
            return true;
        }

        // @remattach[:<folder|attachpt|uuid>]=force
        // TODO: Add support for Attachment groups (RLVa)
        private bool HandleRemAttach(RLVMessage command)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return false;
            }
            var inventoryMap = new InventoryMap(sharedFolder);

            UUID? uuid = null;
            AttachmentPoint? attachmentPoint = null;
            UUID? folderId = null;

            if (UUID.TryParse(command.Option, out var uuidTemp))
            {
                uuid = uuidTemp;
            }
            else if (RLVCommon.RLVAttachmentPointMap.TryGetValue(command.Option, out var attachmentPointTemp))
            {
                attachmentPoint = attachmentPointTemp;
            }
            else if (inventoryMap.TryGetFolderFromPath(command.Option, true, out var folderTemp))
            {
                folderId = folderTemp.Id;
            }
            else if (command.Option.Length > 0)
            {
                return false;
            }

            var itemsToDetach = inventoryMap.Items
                .Where(n =>
                    n.Value.AttachedTo != null &&
                    (uuid == null || n.Value.Id == uuid) &&
                    (attachmentPoint == null || n.Value.AttachedTo == attachmentPoint) &&
                    (folderId == null || n.Value.FolderId == folderId) &&
                    CanRemAttachItem(n.Value, inventoryMap, true)
                )
                .Select(n => n.Value)
                .ToList();

            var itemIdsToDetach = itemsToDetach
                .Select(n => n.Id)
                .Distinct()
                .ToList();

            Detach?.Invoke(this, new DetachEventArgs(itemIdsToDetach));
            return true;
        }

        // @detachme=force
        private bool HandleDetachMe(RLVMessage command)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return false;
            }
            var inventoryMap = new InventoryMap(sharedFolder);

            var itemIdsToDetach = new List<UUID>();
            if (inventoryMap.Items.TryGetValue(command.Sender, out var sender))
            {
                if (CanRemAttachItem(sender, inventoryMap, false))
                {
                    itemIdsToDetach.Add(sender.Id);
                }
            }

            Detach?.Invoke(this, new DetachEventArgs(itemIdsToDetach));
            return true;
        }

        // @remoutfit[:<folder|layer>]=force
        // TODO: Add support for Attachment groups (RLVa)
        private bool HandleRemOutfit(RLVMessage command)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return false;
            }
            var inventoryMap = new InventoryMap(sharedFolder);

            UUID? folderId = null;
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

            var itemsToDetach = inventoryMap.Items
                .Where(n =>
                    n.Value.WornOn != null &&
                    (folderId == null || n.Value.FolderId == folderId) &&
                    (wearableType == null || n.Value.WornOn == wearableType) &&
                    CanRemAttachItem(n.Value, inventoryMap, true)
                )
                .Select(n => n.Value)
                .ToList();

            var itemIdsToDetach = itemsToDetach
                .Select(n => n.Id)
                .Distinct()
                .ToList();

            RemOutfit?.Invoke(this, new RemOutfitEventArgs(itemIdsToDetach));
            return true;
        }

        private bool HandleUnsit(RLVMessage command)
        {
            if (!_manager.CanUnsit())
            {
                return false;
            }

            Unsit?.Invoke(this, new EventArgs());
            return true;
        }

        private bool HandleSitGround(RLVMessage command)
        {
            if (!_manager.CanSit())
            {
                return false;
            }

            SitGround?.Invoke(this, new EventArgs());
            return true;
        }

        private bool HandleSetRot(RLVMessage command)
        {
            if (!float.TryParse(command.Option, out float angleInRadians))
            {
                return false;
            }

            SetRot?.Invoke(this, new SetRotEventArgs(angleInRadians));
            return true;
        }

        private bool HandleAdjustHeight(RLVMessage command)
        {
            var args = command.Option.Split(';');
            if (args.Length < 1)
            {
                return false;
            }

            if (!float.TryParse(args[0], out float distance))
            {
                return false;
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

            AdjustHeight?.Invoke(this, new AdjustHeightEventArgs(distance, factor, deltaInMeters));
            return true;
        }

        private bool HandleSetCamFOV(RLVMessage command)
        {
            if (_manager.IsCamLocked())
            {
                return false;
            }

            if (!float.TryParse(command.Option, out float fov))
            {
                return false;
            }

            SetCamFOV?.Invoke(this, new SetCamFOVEventArgs(fov));
            return true;
        }

        private bool HandleSit(RLVMessage command)
        {
            if (command.Option != string.Empty && !UUID.TryParse(command.Option, out UUID sitTarget))
            {
                return false;
            }

            if (!_manager.CanSit())
            {
                return false;
            }

            if (!_callbacks.TryGetObjectExists(sitTarget, out var isCurrentlySitting).Result)
            {
                return false;
            }

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

            Sit?.Invoke(this, new SitEventArgs(sitTarget));
            return true;
        }

        private bool HandleTpTo(RLVMessage command)
        {
            // @tpto is inhibited by @tploc=n, by @unsit too.
            if (!_manager.CanTpLoc())
            {
                return false;
            }
            if (!_manager.CanUnsit())
            {
                return false;
            }

            var commandArgs = command.Option.Split(';');
            var locationArgs = commandArgs[0].Split('/');

            if (locationArgs.Length < 3 || locationArgs.Length > 4)
            {
                return false;
            }

            float? lookat = null;
            if (commandArgs.Length > 1)
            {
                if (!float.TryParse(commandArgs[1], out float val))
                {
                    return false;
                }

                lookat = val;
            }

            if (locationArgs.Length == 3)
            {
                if (!float.TryParse(locationArgs[0], out var x))
                {
                    return false;
                }
                if (!float.TryParse(locationArgs[1], out var y))
                {
                    return false;
                }
                if (!float.TryParse(locationArgs[2], out var z))
                {
                    return false;
                }

                TpTo?.Invoke(this, new TpToEventArgs(x, y, z, null, lookat));
                return true;
            }
            else if (locationArgs.Length == 4)
            {
                var regionName = locationArgs[0];

                if (!float.TryParse(locationArgs[1], out var x))
                {
                    return false;
                }
                if (!float.TryParse(locationArgs[2], out var y))
                {
                    return false;
                }
                if (!float.TryParse(locationArgs[3], out var z))
                {
                    return false;
                }

                TpTo?.Invoke(this, new TpToEventArgs(x, y, z, regionName, lookat));
                return true;
            }

            return false;
        }
    }
}
