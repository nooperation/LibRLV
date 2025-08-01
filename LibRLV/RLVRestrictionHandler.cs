using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using LibRLV.EventArguments;
using OpenMetaverse;

namespace LibRLV
{
    public partial class RLVRestrictionHandler : IRestrictionProvider
    {
        internal static readonly ImmutableDictionary<string, RLVRestrictionType> NameToRestrictionMap = new Dictionary<string, RLVRestrictionType>(StringComparer.OrdinalIgnoreCase)
        {
            { "notify", RLVRestrictionType.Notify }, // Not a restriction - move to internal use
            { "permissive", RLVRestrictionType.Permissive }, // Not a restriction - exception to a restriction will be ignored if it is not issued by the same object that issued the restriction
            { "fly", RLVRestrictionType.Fly },
            { "jump", RLVRestrictionType.Jump },
            { "temprun", RLVRestrictionType.TempRun },
            { "alwaysrun", RLVRestrictionType.AlwaysRun },
            { "camzoommax", RLVRestrictionType.CamZoomMax },
            { "camzoommin", RLVRestrictionType.CamZoomMin },
            { "camdrawmin", RLVRestrictionType.CamDrawMin },
            { "camdrawmax", RLVRestrictionType.CamDrawMax },
            { "setcam_fovmin", RLVRestrictionType.SetCamFovMin },
            { "setcam_fovmax", RLVRestrictionType.SetCamFovMax },
            { "camdistmax", RLVRestrictionType.CamDistMax },
            { "camdistmin", RLVRestrictionType.CamDistMin },
            { "camdrawalphamin", RLVRestrictionType.CamDrawAlphaMin },
            { "camdrawalphamax", RLVRestrictionType.CamDrawAlphaMax },
            { "setcam_avdistmax", RLVRestrictionType.SetCamAvDistMax }, // synonym of camdistmax
            { "setcam_avdistmin", RLVRestrictionType.SetCamAvDistMin }, // synonym of camdistmin
            { "camdrawcolor", RLVRestrictionType.CamDrawColor },
            { "camunlock", RLVRestrictionType.CamUnlock },
            { "setcam_unlock", RLVRestrictionType.SetCamUnlock }, // synonym of camunlock
            { "camavdist", RLVRestrictionType.CamAvDist },
            { "camtextures", RLVRestrictionType.CamTextures },
            { "setcam_textures", RLVRestrictionType.SetCamTextures }, // synonym of camtextures
            { "sendchat", RLVRestrictionType.SendChat },
            { "chatshout", RLVRestrictionType.ChatShout },
            { "chatnormal", RLVRestrictionType.ChatNormal },
            { "chatwhisper", RLVRestrictionType.ChatWhisper },
            { "redirchat", RLVRestrictionType.RedirChat },
            { "recvchat", RLVRestrictionType.RecvChat }, // Has exception if UUID specified
            { "recvchat_sec", RLVRestrictionType.RecvChatSec }, // Uses exceptions from recvchat, but only from same object
            { "recvchatfrom", RLVRestrictionType.RecvChatFrom },
            { "sendgesture", RLVRestrictionType.SendGesture },
            { "emote", RLVRestrictionType.Emote },
            { "rediremote", RLVRestrictionType.RedirEmote },
            { "recvemote", RLVRestrictionType.RecvEmote }, // Has exception if UUID specified
            { "recvemotefrom", RLVRestrictionType.RecvEmoteFrom },
            { "recvemote_sec", RLVRestrictionType.RecvEmoteSec }, // Uses exceptions from recvemote, but only from same object
            { "sendchannel", RLVRestrictionType.SendChannel }, // Has exception if channel is specified
            { "sendchannel_sec", RLVRestrictionType.SendChannelSec },// Uses exceptions from sendchannel, but only from same object
            { "sendchannel_except", RLVRestrictionType.SendChannelExcept },
            { "sendim", RLVRestrictionType.SendIm },  // Has exception if UUID or group name specified, "allgroups" will block all groups
            { "sendim_sec", RLVRestrictionType.SendImSec }, // Uses exceptions from sendim, but only from same object
            { "sendimto", RLVRestrictionType.SendImTo }, // "allgroups" for group name will block all groups
            { "startim", RLVRestrictionType.StartIm },  // Has exception if UUID is specified
            { "startimto", RLVRestrictionType.StartImTo },
            { "recvim", RLVRestrictionType.RecvIm },  // Has exception if UUID or group name specified, "allgroups" will block all groups
            { "recvim_sec", RLVRestrictionType.RecvImSec }, // Uses exceptions from recvim, but only from same object
            { "recvimfrom", RLVRestrictionType.RecvImFrom }, // "allgroups" for group name will block all groups
            { "tplocal", RLVRestrictionType.TpLocal },
            { "tplm", RLVRestrictionType.TpLm },
            { "tploc", RLVRestrictionType.TpLoc },
            { "tplure", RLVRestrictionType.TpLure }, // Has exception if UUID specified
            { "tplure_sec", RLVRestrictionType.TpLureSec }, // Uses exceptions from tplure, but only from same object
            { "sittp", RLVRestrictionType.SitTp },
            { "standtp", RLVRestrictionType.StandTp },
            { "accepttp", RLVRestrictionType.AcceptTp }, // NOT A RESTRICTION - enables auto-accept teleport. uuid is optional
            { "accepttprequest", RLVRestrictionType.AcceptTpRequest }, // NOT A RESTRICTION - enables auto-accept teleport request. uuid is optional
            { "tprequest", RLVRestrictionType.TpRequest }, // Has exception if UUID specified
            { "tprequest_sec", RLVRestrictionType.TpRequestSec }, // Uses exceptions from tprequest, but only from same object
            { "showinv", RLVRestrictionType.ShowInv },
            { "viewnote", RLVRestrictionType.ViewNote },
            { "viewscript", RLVRestrictionType.ViewScript },
            { "viewtexture", RLVRestrictionType.ViewTexture },
            { "edit", RLVRestrictionType.Edit }, // Has exception if UUID specified
            { "rez", RLVRestrictionType.Rez },
            { "editobj", RLVRestrictionType.EditObj },
            { "editworld", RLVRestrictionType.EditWorld },
            { "editattach", RLVRestrictionType.EditAttach },
            { "share", RLVRestrictionType.Share }, // Has exception if UUID specified
            { "share_sec", RLVRestrictionType.ShareSec }, // Uses exceptions from share, but only from same object
            { "unsit", RLVRestrictionType.Unsit },
            { "sit", RLVRestrictionType.Sit },
            { "detach", RLVRestrictionType.Detach }, // sender cannot be detached when 'n', attachment point in general locked if attachment point name specified 
            { "addattach", RLVRestrictionType.AddAttach },
            { "remattach", RLVRestrictionType.RemAttach },
            { "defaultwear", RLVRestrictionType.DefaultWear },
            { "addoutfit", RLVRestrictionType.AddOutfit }, // If part is not specified, prevents from wearing anything beyond what the avatar is already wearing. 
            { "remoutfit", RLVRestrictionType.RemOutfit }, // If part is not specified, prevents from removing anything in what the avatar is wearing. 
            { "acceptpermission", RLVRestrictionType.AcceptPermission },
            { "denypermission", RLVRestrictionType.DenyPermission },
            { "unsharedwear", RLVRestrictionType.UnsharedWear },
            { "unsharedunwear", RLVRestrictionType.UnsharedUnwear },
            { "sharedwear", RLVRestrictionType.SharedWear },
            { "sharedunwear", RLVRestrictionType.SharedUnwear },
            { "detachthis", RLVRestrictionType.DetachThis },
            { "detachallthis", RLVRestrictionType.DetachAllThis },
            { "attachthis", RLVRestrictionType.AttachThis },
            { "attachallthis", RLVRestrictionType.AttachAllThis },
            { "detachthis_except", RLVRestrictionType.DetachThisExcept }, // single folder exception for detachthis 
            { "detachallthis_except", RLVRestrictionType.DetachAllThisExcept }, // single folder exception for detachallthis 
            { "attachthis_except", RLVRestrictionType.AttachThisExcept }, // single folder exception for attachthis
            { "attachallthis_except", RLVRestrictionType.AttachAllThisExcept }, // single folder exception for attachallthis
            { "fartouch", RLVRestrictionType.FarTouch },
            { "touchfar", RLVRestrictionType.TouchFar }, // synonym of fartouch
            { "touchall", RLVRestrictionType.TouchAll },
            { "touchworld", RLVRestrictionType.TouchWorld }, // Has exception if UUID specified
            { "touchthis", RLVRestrictionType.TouchThis },
            { "touchme", RLVRestrictionType.TouchMe },
            { "touchattach", RLVRestrictionType.TouchAttach }, // does not apply to HUD's
            { "touchattachself", RLVRestrictionType.TouchAttachSelf }, // does not apply to HUD's
            { "touchattachother", RLVRestrictionType.TouchAttachOther }, // UUID is a single restriction, no UUID = global restriction
            { "touchhud", RLVRestrictionType.TouchHud }, // UUID is a single restriction, no UUID = global restriction
            { "interact", RLVRestrictionType.Interact },
            { "showworldmap", RLVRestrictionType.ShowWorldMap },
            { "showminimap", RLVRestrictionType.ShowMiniMap },
            { "showloc", RLVRestrictionType.ShowLoc },
            { "shownames", RLVRestrictionType.ShowNames }, // Has exception if UUID specified
            { "shownames_sec", RLVRestrictionType.ShowNamesSec }, // Has exception if UUID specified and from the same object
            { "shownametags", RLVRestrictionType.ShowNameTags },
            { "shownearby", RLVRestrictionType.ShowNearby },
            { "showhovertextall", RLVRestrictionType.ShowHoverTextAll },
            { "showhovertext", RLVRestrictionType.ShowHoverText },
            { "showhovertexthud", RLVRestrictionType.ShowHoverTextHud },
            { "showhovertextworld", RLVRestrictionType.ShowHoverTextWorld },
            { "setgroup", RLVRestrictionType.SetGroup },
            { "setdebug", RLVRestrictionType.SetDebug },
            { "setenv", RLVRestrictionType.SetEnv },
            { "allowidle", RLVRestrictionType.AllowIdle },
        }.ToImmutableDictionary();

        internal static readonly ImmutableDictionary<RLVRestrictionType, string> RestrictionToNameMap = NameToRestrictionMap
            .ToImmutableDictionary(k => k.Value, v => v.Key);

        public event EventHandler<RestrictionUpdatedEventArgs> RestrictionUpdated;

        private readonly Dictionary<RLVRestrictionType, HashSet<RLVRestriction>> _currentRestrictions = new Dictionary<RLVRestrictionType, HashSet<RLVRestriction>>();

        private readonly IRLVCallbacks _callbacks;
        private readonly LockedFolderManager _lockedFolderManager;

        internal RLVRestrictionHandler(IRLVCallbacks callbacks)
        {
            _callbacks = callbacks;
            _lockedFolderManager = new LockedFolderManager(callbacks, this);
        }

        private void NotifyRestrictionChange(RLVRestriction restriction, bool wasAdded)
        {
            if (!RestrictionToNameMap.TryGetValue(restriction.OriginalBehavior, out var restrictionName))
            {
                return;
            }

            var notification = restrictionName;
            if (restriction.Args.Count > 0)
            {
                notification += ":" + string.Join(";", restriction.Args);
            }

            notification += wasAdded ? "=n" : "=y";

            NotifyRestrictionChange(restrictionName, notification);
        }

        private void NotifyRestrictionChange(string restrictionName, string notificationMessage)
        {
            if (!_currentRestrictions.TryGetValue(RLVRestrictionType.Notify, out var notificationRestrictions))
            {
                return;
            }

            foreach (var notificationRestriction in notificationRestrictions)
            {
                var filter = "";

                if (!(notificationRestriction.Args[0] is int notificationChannel))
                {
                    continue;
                }

                if (notificationRestriction.Args.Count > 1)
                {
                    filter = notificationRestriction.Args[1].ToString();
                }

                if (!restrictionName.Contains(filter))
                {
                    continue;
                }

                _callbacks.SendReplyAsync(
                    notificationChannel,
                    $"/{notificationMessage}",
                    System.Threading.CancellationToken.None
                ).Wait();
            }
        }

        public ImmutableList<RLVRestriction> GetRestrictions(RLVRestrictionType restrictionType)
        {
            restrictionType = RLVRestriction.GetRealRestriction(restrictionType);

            if (!_currentRestrictions.TryGetValue(restrictionType, out var restrictions))
            {
                return ImmutableList<RLVRestriction>.Empty;
            }

            return restrictions.ToImmutableList();
        }

        public ImmutableList<RLVRestriction> GetRestrictions(string behaviorNameFilter = "", UUID? senderFilter = null)
        {
            var restrictions = new List<RLVRestriction>();

            foreach (var item in _currentRestrictions)
            {
                if (!RestrictionToNameMap.TryGetValue(item.Key, out var behaviorName))
                {
                    throw new Exception($"_currentRestrictions has a behavior '{item.Key}' that is not defined in the reverse behavior map");
                }

                if (!behaviorName.Contains(behaviorNameFilter))
                {
                    continue;
                }

                if (!_currentRestrictions.TryGetValue(item.Key, out var realRestrictions))
                {
                    continue;
                }

                foreach (var restriction in realRestrictions)
                {
                    if (senderFilter != null && restriction.Sender != senderFilter)
                    {
                        continue;
                    }

                    restrictions.Add(restriction);
                }
            }

            return restrictions.ToImmutableList();
        }

        private void RemoveRestriction(RLVRestriction restriction)
        {
            if (!_currentRestrictions.TryGetValue(restriction.Behavior, out var restrictions))
            {
                NotifyRestrictionChange(restriction, false);
                return;
            }

            if (restrictions.Contains(restriction))
            {
                RestrictionUpdated?.Invoke(this, new RestrictionUpdatedEventArgs()
                {
                    IsDeleted = true,
                    IsNew = false,
                    Restriction = restriction
                });
                restrictions.Remove(restriction);
            }

            if (restrictions.Count == 0)
            {
                _currentRestrictions.Remove(restriction.Behavior);
            }

            NotifyRestrictionChange(restriction, false);
        }

        private void AddRestriction(RLVRestriction newRestriction)
        {
            if (!_currentRestrictions.TryGetValue(newRestriction.Behavior, out var restrictions))
            {
                restrictions = new HashSet<RLVRestriction>();
                _currentRestrictions.Add(newRestriction.Behavior, restrictions);
            }

            if (!restrictions.Contains(newRestriction))
            {
                // TODO: Check newRestriction args to confirm they're within bounds (like camdrawmin must be at least 0.40f)?

                restrictions.Add(newRestriction);
                RestrictionUpdated?.Invoke(this, new RestrictionUpdatedEventArgs()
                {
                    IsDeleted = false,
                    IsNew = true,
                    Restriction = newRestriction
                });
            }

            NotifyRestrictionChange(newRestriction, true);
        }

        internal bool ProcessClearCommand(RLVMessage command)
        {
            var filteredRestrictions = RestrictionToNameMap
                .Where(n => n.Value.Contains(command.Param))
                .Select(n => n.Key)
                .ToList();

            foreach (var restrictionType in filteredRestrictions)
            {
                if (!_currentRestrictions.TryGetValue(restrictionType, out var restrictionsToRemove))
                {
                    continue;
                }

                foreach (var restrictionToRemove in restrictionsToRemove)
                {
                    if (restrictionToRemove.Sender != command.Sender)
                    {
                        continue;
                    }

                    RemoveRestriction(restrictionToRemove);
                }
            }


            var notificationMessage = "clear";
            if (command.Param != "")
            {
                notificationMessage += $":{command.Param}";
            }

            NotifyRestrictionChange("clear", notificationMessage);
            return true;
        }

        internal bool ProcessRestrictionCommand(RLVMessage message, string option, bool isAddingRestriction)
        {
            if (!NameToRestrictionMap.TryGetValue(message.Behavior, out var behavior))
            {
                return false;
            }

            if (!RLVRestriction.ParseOptions(behavior, option, out var args))
            {
                return false;
            }

            var newCommand = new RLVRestriction(behavior, message.Sender, message.SenderName, args);

            if (isAddingRestriction)
            {
                AddRestriction(newCommand);

                switch (newCommand.Behavior)
                {
                    case RLVRestrictionType.DetachThis:
                    case RLVRestrictionType.DetachAllThis:
                    case RLVRestrictionType.AttachThis:
                    case RLVRestrictionType.AttachAllThis:
                        _lockedFolderManager.ProcessFolderRestrictions(newCommand);
                        break;
                    case RLVRestrictionType.DetachThisExcept:
                    case RLVRestrictionType.DetachAllThisExcept:
                    case RLVRestrictionType.AttachThisExcept:
                    case RLVRestrictionType.AttachAllThisExcept:
                        _lockedFolderManager.ProcessFolderException(newCommand);
                        break;
                }
            }
            else
            {
                RemoveRestriction(newCommand);

                switch (newCommand.Behavior)
                {
                    case RLVRestrictionType.DetachThis:
                    case RLVRestrictionType.DetachAllThis:
                    case RLVRestrictionType.AttachThis:
                    case RLVRestrictionType.AttachAllThis:
                        _lockedFolderManager.RebuildLockedFolders();
                        break;
                    case RLVRestrictionType.DetachThisExcept:
                    case RLVRestrictionType.DetachAllThisExcept:
                    case RLVRestrictionType.AttachThisExcept:
                    case RLVRestrictionType.AttachAllThisExcept:
                        _lockedFolderManager.RebuildLockedFolders();
                        break;
                }
            }

            return true;
        }

        public bool TryGetLockedFolder(UUID folderId, out LockedFolderPublic lockedFolder)
        {
            return _lockedFolderManager.TryGetLockedFolder(folderId, out lockedFolder);
        }

        public ImmutableDictionary<UUID, LockedFolderPublic> GetLockedFolders()
        {
            return _lockedFolderManager.GetLockedFolders();
        }
    }
}
