using LibRLV.EventArguments;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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

        public RLVActionHandler()
        {
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
                { "detach", HandleDetach},
                { "remattach", HandleDetach},
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
            if (!RLVActionHandlers.TryGetValue(command.Behavior, out var func))
            {
                return false;
            }

            if (command.Behavior.StartsWith("setdebug_"))
            {
                RLVActionHandlers["setdebug_"](command);
            }
            if (command.Behavior.StartsWith("setenv_"))
            {
                RLVActionHandlers["setenv_"](command);
            }

            return func(command);
        }

        private bool HandleSetDebug(RLVMessage command)
        {
            var settingName = command.Behavior.Substring(command.Behavior.IndexOf('_'));
            SetDebug?.Invoke(this, new SetSettingEventArgs(settingName, command.Option));

            return true;
        }

        private bool HandleSetEnv(RLVMessage command)
        {
            var settingName = command.Behavior.Substring(command.Behavior.IndexOf('_'));
            SetEnv?.Invoke(this, new SetSettingEventArgs(settingName, command.Option));

            return true;
        }

        private bool HandleSetGroup(RLVMessage command)
        {
            SetGroup?.Invoke(this, new SetGroupEventArgs(command.Option));
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

        private bool HandleDetach(RLVMessage command)
        {
            if (UUID.TryParse(command.Option, out UUID uuid))
            {
                Detach?.Invoke(this, new DetachEventArgs(uuid, null));
                return true;
            }

            Detach?.Invoke(this, new DetachEventArgs(null, command.Option));
            return true;
        }

        private bool HandleUnsit(RLVMessage command)
        {
            Unsit?.Invoke(this, new EventArgs());
            return true;
        }

        private bool HandleSitGround(RLVMessage command)
        {
            SitGround?.Invoke(this, new EventArgs());
            return true;
        }

        private bool HandleRemOutfit(RLVMessage command)
        {
            if (!Enum.TryParse(command.Option, true, out WearableType part))
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
            if (!double.TryParse(command.Option, out double angleInRadians))
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

            if (!double.TryParse(args[0], out double distance))
            {
                return false;
            }

            var factor = 1.0;
            var deltaInMeters = 0.0;

            if (args.Length > 1 && !double.TryParse(args[1], out factor))
            {
                factor = 1;
            }

            if (args.Length > 2 && !double.TryParse(args[2], out deltaInMeters))
            {
                deltaInMeters = 0;
            }

            AdjustHeight?.Invoke(this, new AdjustHeightEventArgs(distance, factor, deltaInMeters));
            return true;
        }

        private bool HandleSetCamFOV(RLVMessage command)
        {
            if (!double.TryParse(command.Option, out double fov))
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

            Sit?.Invoke(this, new SitEventArgs(sitTarget));
            return true;
        }

        private bool HandleTpTo(RLVMessage command)
        {
            var args = command.Option.Split('/');
            if (args.Length == 3)
            {
                if (!double.TryParse(args[0], out double x))
                {
                    return false;
                }
                if (!double.TryParse(args[1], out double y))
                {
                    return false;
                }
                if (!double.TryParse(args[2], out double z))
                {
                    return false;
                }

                TpTo?.Invoke(this, new TpToEventArgs(x, y, z, null, null));
                return true;
            }
            else if (args.Length == 4)
            {
                var regionName = args[0];
                var subargs = args[3].Split(';');

                if (!double.TryParse(args[1], out double x))
                {
                    return false;
                }
                if (!double.TryParse(args[2], out double y))
                {
                    return false;
                }
                if (!double.TryParse(subargs[0], out double z))
                {
                    return false;
                }

                double? lookat = null;
                if (subargs.Length == 2)
                {
                    if (!double.TryParse(subargs[1], out double val))
                    {
                        return false;
                    }

                    lookat = val;
                }

                TpTo?.Invoke(this, new TpToEventArgs(x, y, z, regionName, lookat));
                return true;
            }

            return false;
        }
    }
}
