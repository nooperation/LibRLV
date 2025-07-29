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
        public event EventHandler<InventoryPathEventArgs> Attach;
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
                { "attach", n => HandleInventoryThing(n, Attach)},
                { "attachover", n => HandleInventoryThing(n, AttachOver)},
                { "attachoverorreplace", n => HandleInventoryThing(n, AttachOver)},
                { "attachall", n => HandleInventoryThing(n, AttachAll)},
                { "attachallover", n => HandleInventoryThing(n, AttachAll)},
                { "attachalloverorreplace", n => HandleInventoryThing(n, AttachAllOverOrReplace)},
                { "detach", HandleRemAttach},
                { "remattach", HandleRemAttach},
                { "detachall", n => HandleInventoryThing(n, DetachAll)},
                { "attachthis", n => HandleAttachmentThing(n, AttachThis)},
                { "attachthisover", n => HandleAttachmentThing(n, AttachThisOver)},
                { "attachthisoverorreplace", n => HandleAttachmentThing(n, AttachThisOverOrReplace)},
                { "attachallthis", n => HandleAttachmentThing(n, AttachAllThis)},
                { "attachallthisover", n => HandleAttachmentThing(n, AttachAllThisOver)},
                { "attachallthisoverorreplace", n => HandleAttachmentThing(n, AttachAllThisOverOrReplace)},
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
            handler?.Invoke(this, new AttachmentEventArgs(command.Option));
            return true;
        }

        private bool CanRemAttachItem(InventoryItem item, InventoryMap inventoryMap, UUID? uuid, AttachmentPoint? attachmentPoint)
        {
            if (item.AttachedTo == null)
            {
                return false;
            }

            if (item.Name.ToLower().Contains("nostrip"))
            {
                return false;
            }

            if (!inventoryMap.Folders.TryGetValue(item.FolderId, out var folder))
            {
                while (folder != null)
                {
                    if (folder.Name.Contains("nostrip"))
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

            if (uuid != null)
            {
                return item.Id == uuid.Value;
            }

            if (attachmentPoint != null)
            {
                return item.AttachedTo == attachmentPoint.Value;
            }

            return true;
        }

        private bool HandleRemAttach(RLVMessage command)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return false;
            }
            var inventoryMap = new InventoryMap(sharedFolder);

            UUID? uuid = null;
            if (UUID.TryParse(command.Option, out var uuidTemp))
            {
                uuid = uuidTemp;
            }

            AttachmentPoint? attachmentPoint = null;
            if (RLVCommon.RLVAttachmentPointMap.TryGetValue(command.Option, out var attachmentPointTemp))
            {
                attachmentPoint = attachmentPointTemp;
            }

            var itemsToDetach = inventoryMap.Items
                .Where(n => CanRemAttachItem(n.Value, inventoryMap, uuid, attachmentPoint))
                .Select(n => n.Value)
                .ToList();

            var itemIdsToDetach = itemsToDetach
                .Select(n => n.Id)
                .Distinct()
                .ToList();

            Detach?.Invoke(this, new DetachEventArgs(itemIdsToDetach));
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

        private bool HandleRemOutfit(RLVMessage command)
        {
            if (!RLVCommon.RLVWearableTypeMap.TryGetValue(command.Option, out WearableType part))
            {
                return false;
            }

            if (part == WearableType.Skin ||
               part == WearableType.Shape ||
               part == WearableType.Eyes ||
               part == WearableType.Hair ||
               part == WearableType.Invalid)
            {
                return false;
            }

            RemOutfit?.Invoke(this, new RemOutfitEventArgs(part));
            return true;
        }

        private bool HandleDetachMe(RLVMessage command)
        {
            DetachMe?.Invoke(this, new EventArgs());
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
