using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using LibRLV.EventArguments;

namespace LibRLV
{
    public partial class RLVRestrictionManager : IRestrictionProvider
    {
        internal static readonly ImmutableDictionary<string, RLVRestrictionType> NameToRestrictionMap = new Dictionary<string, RLVRestrictionType>(StringComparer.OrdinalIgnoreCase)
        {
            { "notify", RLVRestrictionType.Notify },
            { "permissive", RLVRestrictionType.Permissive },
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
            { "setcam_avdistmax", RLVRestrictionType.SetCamAvDistMax },
            { "setcam_avdistmin", RLVRestrictionType.SetCamAvDistMin },
            { "camdrawcolor", RLVRestrictionType.CamDrawColor },
            { "camunlock", RLVRestrictionType.CamUnlock },
            { "setcam_unlock", RLVRestrictionType.SetCamUnlock },
            { "camavdist", RLVRestrictionType.CamAvDist },
            { "camtextures", RLVRestrictionType.CamTextures },
            { "setcam_textures", RLVRestrictionType.SetCamTextures },
            { "sendchat", RLVRestrictionType.SendChat },
            { "chatshout", RLVRestrictionType.ChatShout },
            { "chatnormal", RLVRestrictionType.ChatNormal },
            { "chatwhisper", RLVRestrictionType.ChatWhisper },
            { "redirchat", RLVRestrictionType.RedirChat },
            { "recvchat", RLVRestrictionType.RecvChat },
            { "recvchat_sec", RLVRestrictionType.RecvChatSec },
            { "recvchatfrom", RLVRestrictionType.RecvChatFrom },
            { "sendgesture", RLVRestrictionType.SendGesture },
            { "emote", RLVRestrictionType.Emote },
            { "rediremote", RLVRestrictionType.RedirEmote },
            { "recvemote", RLVRestrictionType.RecvEmote },
            { "recvemotefrom", RLVRestrictionType.RecvEmoteFrom },
            { "recvemote_sec", RLVRestrictionType.RecvEmoteSec },
            { "sendchannel", RLVRestrictionType.SendChannel },
            { "sendchannel_sec", RLVRestrictionType.SendChannelSec },
            { "sendchannel_except", RLVRestrictionType.SendChannelExcept },
            { "sendim", RLVRestrictionType.SendIm },
            { "sendim_sec", RLVRestrictionType.SendImSec },
            { "sendimto", RLVRestrictionType.SendImTo },
            { "startim", RLVRestrictionType.StartIm },
            { "startimto", RLVRestrictionType.StartImTo },
            { "recvim", RLVRestrictionType.RecvIm },
            { "recvim_sec", RLVRestrictionType.RecvImSec },
            { "recvimfrom", RLVRestrictionType.RecvImFrom },
            { "tplocal", RLVRestrictionType.TpLocal },
            { "tplm", RLVRestrictionType.TpLm },
            { "tploc", RLVRestrictionType.TpLoc },
            { "tplure", RLVRestrictionType.TpLure },
            { "tplure_sec", RLVRestrictionType.TpLureSec },
            { "sittp", RLVRestrictionType.SitTp },
            { "standtp", RLVRestrictionType.StandTp },
            { "accepttp", RLVRestrictionType.AcceptTp },
            { "accepttprequest", RLVRestrictionType.AcceptTpRequest },
            { "tprequest", RLVRestrictionType.TpRequest },
            { "tprequest_sec", RLVRestrictionType.TpRequestSec },
            { "showinv", RLVRestrictionType.ShowInv },
            { "viewnote", RLVRestrictionType.ViewNote },
            { "viewscript", RLVRestrictionType.ViewScript },
            { "viewtexture", RLVRestrictionType.ViewTexture },
            { "edit", RLVRestrictionType.Edit },
            { "rez", RLVRestrictionType.Rez },
            { "editobj", RLVRestrictionType.EditObj },
            { "editworld", RLVRestrictionType.EditWorld },
            { "editattach", RLVRestrictionType.EditAttach },
            { "share", RLVRestrictionType.Share },
            { "share_sec", RLVRestrictionType.ShareSec },
            { "unsit", RLVRestrictionType.Unsit },
            { "sit", RLVRestrictionType.Sit },
            { "detach", RLVRestrictionType.Detach },
            { "addattach", RLVRestrictionType.AddAttach },
            { "remattach", RLVRestrictionType.RemAttach },
            { "defaultwear", RLVRestrictionType.DefaultWear },
            { "addoutfit", RLVRestrictionType.AddOutfit },
            { "remoutfit", RLVRestrictionType.RemOutfit },
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
            { "detachthis_except", RLVRestrictionType.DetachThisExcept },
            { "detachallthis_except", RLVRestrictionType.DetachAllThisExcept },
            { "attachthis_except", RLVRestrictionType.AttachThisExcept },
            { "attachallthis_except", RLVRestrictionType.AttachAllThisExcept },
            { "fartouch", RLVRestrictionType.FarTouch },
            { "touchfar", RLVRestrictionType.TouchFar },
            { "touchall", RLVRestrictionType.TouchAll },
            { "touchworld", RLVRestrictionType.TouchWorld },
            { "touchthis", RLVRestrictionType.TouchThis },
            { "touchme", RLVRestrictionType.TouchMe },
            { "touchattach", RLVRestrictionType.TouchAttach },
            { "touchattachself", RLVRestrictionType.TouchAttachSelf },
            { "touchattachother", RLVRestrictionType.TouchAttachOther },
            { "touchhud", RLVRestrictionType.TouchHud },
            { "interact", RLVRestrictionType.Interact },
            { "showworldmap", RLVRestrictionType.ShowWorldMap },
            { "showminimap", RLVRestrictionType.ShowMiniMap },
            { "showloc", RLVRestrictionType.ShowLoc },
            { "shownames", RLVRestrictionType.ShowNames },
            { "shownames_sec", RLVRestrictionType.ShowNamesSec },
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
        }.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly ImmutableDictionary<RLVRestrictionType, string> RestrictionToNameMap = NameToRestrictionMap
            .ToImmutableDictionary(k => k.Value, v => v.Key);

        public event EventHandler<RestrictionUpdatedEventArgs> RestrictionUpdated;

        private readonly Dictionary<RLVRestrictionType, HashSet<RLVRestriction>> _currentRestrictions = new Dictionary<RLVRestrictionType, HashSet<RLVRestriction>>();
        private readonly object _currentRestrictionsLock = new object();

        private readonly IRLVCallbacks _callbacks;
        private readonly LockedFolderManager _lockedFolderManager;

        internal RLVRestrictionManager(IRLVCallbacks callbacks)
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
            List<RLVRestriction> notificationRestrictions;

            lock (_currentRestrictionsLock)
            {
                if (!_currentRestrictions.TryGetValue(RLVRestrictionType.Notify, out var notificationRestrictionsTemp))
                {
                    return;
                }

                notificationRestrictions = notificationRestrictionsTemp.ToList();
            }

            foreach (var notificationRestriction in notificationRestrictions)
            {
                var filter = "";

                if (notificationRestriction.Args.Count == 0)
                {
                    continue;
                }

                if (!(notificationRestriction.Args[0] is int notificationChannel))
                {
                    continue;
                }

                if (notificationRestriction.Args.Count > 1)
                {
                    filter = notificationRestriction.Args[1].ToString();
                }

                if (!restrictionName.Contains(filter.ToLowerInvariant()))
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

        public IEnumerable<Guid> GetTrackedObjectIds()
        {
            lock (_currentRestrictionsLock)
            {
                return _currentRestrictions
                    .SelectMany(n => n.Value)
                    .Select(n => n.Sender)
                    .Distinct()
                    .ToList();
            }
        }

        public void RemoveRestrictionsForObjects(IEnumerable<Guid> objectIds)
        {
            var objectIdMap = objectIds.ToImmutableHashSet();
            var removedRestrictions = new List<RLVRestriction>();

            lock (_currentRestrictionsLock)
            {
                var restrictionsToRemove = _currentRestrictions
                    .SelectMany(n => n.Value)
                    .Where(n => objectIdMap.Contains(n.Sender))
                    .ToList();

                foreach (var restriction in restrictionsToRemove)
                {
                    var removedRestriction = false;

                    removedRestriction = RemoveRestriction_InternalUnsafe(restriction);
                    if (removedRestriction)
                    {
                        removedRestrictions.Add(restriction);
                    }
                }
            }

            foreach (var restriction in removedRestrictions)
            {
                var handler = RestrictionUpdated;
                handler?.Invoke(this, new RestrictionUpdatedEventArgs(restriction, false, true));

                NotifyRestrictionChange(restriction, false);
            }
        }

        public IReadOnlyList<RLVRestriction> GetRestrictions(RLVRestrictionType restrictionType)
        {
            restrictionType = RLVRestriction.GetRealRestriction(restrictionType);

            lock (_currentRestrictionsLock)
            {
                if (!_currentRestrictions.TryGetValue(restrictionType, out var restrictions))
                {
                    return ImmutableList<RLVRestriction>.Empty;
                }

                return restrictions.ToImmutableList();
            }
        }

        public IReadOnlyList<RLVRestriction> GetRestrictions(string behaviorNameFilter = "", Guid? senderFilter = null)
        {
            var restrictions = new List<RLVRestriction>();

            lock (_currentRestrictionsLock)
            {
                foreach (var item in _currentRestrictions)
                {
                    if (!RestrictionToNameMap.TryGetValue(item.Key, out var behaviorName))
                    {
                        throw new KeyNotFoundException($"_currentRestrictions has a behavior '{item.Key}' that is not defined in the reverse behavior map");
                    }

                    if (!behaviorName.Contains(behaviorNameFilter.ToLowerInvariant()))
                    {
                        continue;
                    }

                    foreach (var restriction in item.Value)
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
        }

        private bool RemoveRestriction_InternalUnsafe(RLVRestriction restriction)
        {
            var removedRestriction = false;

            if (_currentRestrictions.TryGetValue(restriction.Behavior, out var restrictions))
            {
                removedRestriction = restrictions.Remove(restriction);

                if (restrictions.Count == 0)
                {
                    _currentRestrictions.Remove(restriction.Behavior);
                }
            }

            return removedRestriction;
        }

        private void RemoveRestriction(RLVRestriction restriction)
        {
            var removedRestriction = false;

            lock (_currentRestrictionsLock)
            {
                removedRestriction = RemoveRestriction_InternalUnsafe(restriction);
            }

            if (removedRestriction)
            {
                var handler = RestrictionUpdated;
                handler?.Invoke(this, new RestrictionUpdatedEventArgs(restriction, false, true));
            }

            NotifyRestrictionChange(restriction, false);
        }

        private void AddRestriction(RLVRestriction newRestriction)
        {
            var restrictionAdded = false;

            lock (_currentRestrictionsLock)
            {
                if (!_currentRestrictions.TryGetValue(newRestriction.Behavior, out var restrictions))
                {
                    restrictions = new HashSet<RLVRestriction>();
                    _currentRestrictions.Add(newRestriction.Behavior, restrictions);
                }

                if (restrictions.Add(newRestriction))
                {
                    restrictionAdded = true;
                }
            }

            if (restrictionAdded)
            {
                var handler = RestrictionUpdated;
                handler?.Invoke(this, new RestrictionUpdatedEventArgs(newRestriction, true, false));
            }

            NotifyRestrictionChange(newRestriction, true);
        }

        internal bool ProcessClearCommand(RLVMessage command)
        {
            var filteredRestrictions = RestrictionToNameMap
                .Where(n => n.Value.Contains(command.Param.ToLowerInvariant()))
                .Select(n => n.Key)
                .ToList();

            var removedRestrictions = new List<RLVRestriction>();
            lock (_currentRestrictionsLock)
            {
                foreach (var restrictionType in filteredRestrictions)
                {
                    if (!_currentRestrictions.TryGetValue(restrictionType, out var restrictionsToRemove))
                    {
                        continue;
                    }

                    var restrictionsToRemoveSnapshot = restrictionsToRemove.ToList();
                    foreach (var restrictionToRemove in restrictionsToRemoveSnapshot)
                    {
                        if (restrictionToRemove.Sender != command.Sender)
                        {
                            continue;
                        }

                        if (RemoveRestriction_InternalUnsafe(restrictionToRemove))
                        {
                            removedRestrictions.Add(restrictionToRemove);
                        }
                    }
                }
            }
            _lockedFolderManager.RebuildLockedFolders();

            foreach (var removedRestriction in removedRestrictions)
            {
                var handler = RestrictionUpdated;
                handler?.Invoke(this, new RestrictionUpdatedEventArgs(removedRestriction, false, true));

                NotifyRestrictionChange(removedRestriction, false);
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
            }
            else
            {
                RemoveRestriction(newCommand);
            }

            switch (newCommand.Behavior)
            {
                case RLVRestrictionType.DetachThis:
                case RLVRestrictionType.DetachAllThis:
                case RLVRestrictionType.AttachThis:
                case RLVRestrictionType.AttachAllThis:
                {
                    if (isAddingRestriction)
                    {
                        _lockedFolderManager.ProcessFolderRestrictions(newCommand);
                    }
                    else
                    {
                        _lockedFolderManager.RebuildLockedFolders();
                    }
                    break;
                }
                case RLVRestrictionType.DetachThisExcept:
                case RLVRestrictionType.DetachAllThisExcept:
                case RLVRestrictionType.AttachThisExcept:
                case RLVRestrictionType.AttachAllThisExcept:
                {
                    if (isAddingRestriction)
                    {
                        _lockedFolderManager.ProcessFolderException(newCommand);
                    }
                    else
                    {
                        _lockedFolderManager.RebuildLockedFolders();

                    }
                    break;
                }
            }

            return true;
        }

        public bool TryGetLockedFolder(Guid folderId, out LockedFolderPublic lockedFolder)
        {
            return _lockedFolderManager.TryGetLockedFolder(folderId, out lockedFolder);
        }

        public IReadOnlyDictionary<Guid, LockedFolderPublic> GetLockedFolders()
        {
            return _lockedFolderManager.GetLockedFolders();
        }
    }
}
