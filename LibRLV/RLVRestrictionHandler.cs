using LibRLV.EventArguments;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LibRLV
{
    public partial class RLVRestrictionHandler : IRestrictionProvider
    {
        internal static readonly ImmutableDictionary<string, RLVRestrictionType> NameToRestrictionMap = new Dictionary<string, RLVRestrictionType>(StringComparer.OrdinalIgnoreCase)
        {
            { "notify", RLVRestrictionType.Notify }, // Not a restriction - move to internal use
            { "permissive", RLVRestrictionType.Permissive }, // Not a restriction - exception to a restriction will be ignored if it is not issued by the same object that issued the restriction
            { "fly", RLVRestrictionType.Fly },
            { "temprun", RLVRestrictionType.TempRun },
            { "alwaysrun", RLVRestrictionType.AlwaysRun },
            { "camzoommax", RLVRestrictionType.CamZoomMax },
            { "camzoommin", RLVRestrictionType.CamZoomMin },
            { "setcam_fovmin", RLVRestrictionType.SetCamFovMin },
            { "setcam_fovmax", RLVRestrictionType.SetCamFovMax },
            { "camdistmax", RLVRestrictionType.CamDistMax },
            { "setcam_avdistmax", RLVRestrictionType.SetCamAvDistMax }, // synonym of camdistmax 
            { "camdistmin", RLVRestrictionType.CamDistMin },
            { "setcam_avdistmin", RLVRestrictionType.SetCamAvDistMin }, // synonym of camdistmin
            { "camdrawalphamax", RLVRestrictionType.CamDrawAlphaMax },
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

        public Dictionary<UUID, LockedFolder> LockedFolders { get; set; } = new Dictionary<UUID, LockedFolder>();

        public RLVRestrictionHandler(IRLVCallbacks callbacks)
        {
            this._callbacks = callbacks;
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

        public bool IsPermissiveMode { get; set; }

        private RLVRestrictionType GetSecureRestriction(RLVRestrictionType restrictionType)
        {
            switch (restrictionType)
            {
                case RLVRestrictionType.RecvChat:
                    return RLVRestrictionType.RecvChatSec;
                case RLVRestrictionType.RecvEmoteFrom:
                    return RLVRestrictionType.RecvEmoteSec;
                case RLVRestrictionType.SendChannel:
                    return RLVRestrictionType.SendChannelSec;
                case RLVRestrictionType.SendIm:
                    return RLVRestrictionType.SendImSec;
                case RLVRestrictionType.RecvIm:
                    return RLVRestrictionType.RecvImSec;
                case RLVRestrictionType.TpLure:
                    return RLVRestrictionType.TpLureSec;
                case RLVRestrictionType.TpRequest:
                    return RLVRestrictionType.TpRequestSec;
                case RLVRestrictionType.Share:
                    return RLVRestrictionType.ShareSec;
                case RLVRestrictionType.ShowNames:
                    return RLVRestrictionType.ShowNamesSec;
            }

            return restrictionType;
        }

        private bool IsSecureRestriction(RLVRestrictionType restrictionType)
        {
            switch (restrictionType)
            {
                case RLVRestrictionType.RecvChatSec:
                case RLVRestrictionType.RecvEmoteSec:
                case RLVRestrictionType.SendChannelSec:
                case RLVRestrictionType.SendImSec:
                case RLVRestrictionType.RecvImSec:
                case RLVRestrictionType.TpLureSec:
                case RLVRestrictionType.TpRequestSec:
                case RLVRestrictionType.ShareSec:
                case RLVRestrictionType.ShowNamesSec:
                    return true;
            }

            return false;
        }

        private RLVRestrictionType GetInsecureRestriction(RLVRestrictionType restrictionType)
        {
            if (!IsPermissiveMode)
            {
                switch (restrictionType)
                {
                    case RLVRestrictionType.RecvChatSec:
                        return RLVRestrictionType.RecvChat;
                    case RLVRestrictionType.RecvEmoteSec:
                        return RLVRestrictionType.RecvEmoteFrom;
                    case RLVRestrictionType.SendChannelSec:
                        return RLVRestrictionType.SendChannel;
                    case RLVRestrictionType.SendImSec:
                        return RLVRestrictionType.SendIm;
                    case RLVRestrictionType.RecvImSec:
                        return RLVRestrictionType.RecvIm;
                    case RLVRestrictionType.TpLureSec:
                        return RLVRestrictionType.TpLure;
                    case RLVRestrictionType.TpRequestSec:
                        return RLVRestrictionType.TpRequest;
                    case RLVRestrictionType.ShareSec:
                        return RLVRestrictionType.Share;
                    case RLVRestrictionType.ShowNamesSec:
                        return RLVRestrictionType.ShowNames;
                }
            }

            return restrictionType;
        }

        public ImmutableList<RLVRestriction> GetRestrictions(RLVRestrictionType restrictionType, UUID? sender = null)
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

        private static bool IsFolderLockCommand(RLVRestrictionType restrictionType)
        {
            switch (restrictionType)
            {
                case RLVRestrictionType.DetachThis:
                case RLVRestrictionType.DetachAllThis:
                case RLVRestrictionType.AttachThis:
                case RLVRestrictionType.AttachAllThis:
                case RLVRestrictionType.DetachThisExcept:
                case RLVRestrictionType.DetachAllThisExcept:
                case RLVRestrictionType.AttachThisExcept:
                case RLVRestrictionType.AttachAllThisExcept:
                    return true;
            }

            return false;
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

        public void RemoveRestrictionsRelatedToObjects(ICollection<UUID> objectIds)
        {
            var objectsMap = objectIds.ToImmutableHashSet();
            var emptyRestrictions = new List<RLVRestrictionType>();

            var restrictionsToRemove = new List<RLVRestriction>();
            foreach (var item in _currentRestrictions)
            {
                restrictionsToRemove.AddRange(
                    item.Value.Where(n => objectsMap.Contains(n.Sender))
                );
            }

            foreach (var restrictionToRemove in restrictionsToRemove)
            {
                RemoveRestriction(restrictionToRemove);
            }
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

        private bool BuildInventoryMap(InventoryTree tree, Dictionary<UUID, InventoryTree> outTree)
        {
            if (outTree.ContainsKey(tree.Id))
            {
                return false;
            }

            outTree[tree.Id] = tree;

            foreach (var child in tree.Children)
            {
                if (!BuildInventoryMap(child, outTree))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TryGetFolderFromPath(string path, InventoryTree root, out InventoryTree foundFolder)
        {
            var splitPath = path.Split('/');

            var treeIter = root;
            foreach (var part in splitPath)
            {
                var child = treeIter.Children.Where(n => n.Name == part).FirstOrDefault();
                if (child == null)
                {
                    foundFolder = default;
                    return false;
                }

                treeIter = child;
            }

            foundFolder = treeIter;
            return true;
        }

        private static void GetWornItems(InventoryTree root, WearableType wearableType, List<InventoryTree.InventoryItem> outWornItems)
        {
            outWornItems.AddRange(root.Items.Where(n => n.WornOn == wearableType));

            foreach (var item in root.Children)
            {
                GetWornItems(item, wearableType, outWornItems);
            }
        }

        private static void GetAttachedItems(InventoryTree root, AttachmentPoint attachmentPoint, List<InventoryTree.InventoryItem> outAttachedItems)
        {
            outAttachedItems.AddRange(root.Items.Where(n => n.AttachedTo == attachmentPoint));

            foreach (var item in root.Children)
            {
                GetAttachedItems(item, attachmentPoint, outAttachedItems);
            }
        }

        private static List<InventoryTree> GetFoldersForItems(Dictionary<UUID, InventoryTree> rootMap, List<InventoryTree.InventoryItem> items)
        {
            var result = new Dictionary<UUID, InventoryTree>();

            foreach (var item in items)
            {
                if (!rootMap.TryGetValue(item.FolderId, out InventoryTree folder))
                {
                    continue;
                }

                result[folder.Id] = folder;
            }

            return result.Values.ToList();
        }


        private void AddLockedFolder(InventoryTree folder, RLVRestriction restriction)
        {
            if (!LockedFolders.TryGetValue(folder.Id, out var existingLockedFolder))
            {
                existingLockedFolder = new LockedFolder(folder);
                LockedFolders[folder.Id] = existingLockedFolder;
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
                existingLockedFolder.AttachExceptions.Add(restriction);
            }
            else if (restriction.Behavior == RLVRestrictionType.AttachAllThisExcept || restriction.Behavior == RLVRestrictionType.AttachThisExcept)
            {
                existingLockedFolder.DetachExceptions.Add(restriction);
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

        private void RebuildLockedFolders()
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

            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return;
            }

            LockedFolders.Clear();

            var sharedFolderMap = new Dictionary<UUID, InventoryTree>();
            if (!BuildInventoryMap(sharedFolder, sharedFolderMap))
            {
                return;
            }

            if (_currentRestrictions.TryGetValue(RLVRestrictionType.DetachThis, out var detachThisRestrictions))
            {
                foreach (var restriction in detachThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, sharedFolderMap);
                }
            }
            if (_currentRestrictions.TryGetValue(RLVRestrictionType.DetachAllThis, out var detachAllThisRestrictions))
            {
                foreach (var restriction in detachAllThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, sharedFolderMap);
                }
            }
            if (_currentRestrictions.TryGetValue(RLVRestrictionType.AttachThis, out var attachThisRestrictions))
            {
                foreach (var restriction in attachThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, sharedFolderMap);
                }
            }
            if (_currentRestrictions.TryGetValue(RLVRestrictionType.AttachAllThis, out var attachAllThisRestrictions))
            {
                foreach (var restriction in attachAllThisRestrictions)
                {
                    ProcessFolderRestrictions(restriction, sharedFolder, sharedFolderMap);
                }
            }

            if (_currentRestrictions.TryGetValue(RLVRestrictionType.DetachThisExcept, out var detachThisExceptions))
            {
                foreach (var exception in detachThisExceptions)
                {
                    ProcessFolderException(exception, sharedFolder);
                }
            }
            if (_currentRestrictions.TryGetValue(RLVRestrictionType.DetachAllThisExcept, out var detachAllThisExceptions))
            {
                foreach (var exception in detachAllThisExceptions)
                {
                    ProcessFolderException(exception, sharedFolder);
                }
            }
            if (_currentRestrictions.TryGetValue(RLVRestrictionType.AttachThisExcept, out var attachThisExceptions))
            {
                foreach (var exception in attachThisExceptions)
                {
                    ProcessFolderException(exception, sharedFolder);
                }
            }
            if (_currentRestrictions.TryGetValue(RLVRestrictionType.AttachAllThisExcept, out var attachAllThisExceptions))
            {
                foreach (var exception in attachAllThisExceptions)
                {
                    ProcessFolderException(exception, sharedFolder);
                }
            }
        }

        public bool TryGetLockedFolder(UUID folderId, out LockedFolder lockedFolder)
        {
            return LockedFolders.TryGetValue(folderId, out lockedFolder);
        }

        private bool ProcessFolderException(RLVRestriction exception)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
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
                if (!TryGetFolderFromPath(path, sharedFolder, out var folder))
                {
                    return false;
                }

                AddLockedFolder(folder, exception);
            }

            return true;
        }

        private bool ProcessFolderRestrictions(RLVRestriction restriction)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return false;
            }

            var sharedFolderMap = new Dictionary<UUID, InventoryTree>();
            if (!BuildInventoryMap(sharedFolder, sharedFolderMap))
            {
                return false;
            }

            return ProcessFolderRestrictions(restriction, sharedFolder, sharedFolderMap);
        }

        private bool ProcessFolderRestrictions(RLVRestriction restriction, InventoryTree sharedFolder, Dictionary<UUID, InventoryTree> sharedFolderMap)
        {
            if (restriction.Args.Count == 0)
            {
                if (!sharedFolderMap.TryGetValue(restriction.Sender, out var folder))
                {
                    return false;
                }

                AddLockedFolder(folder, restriction);
            }
            else if (restriction.Args[0] is WearableType wearableType)
            {
                var wornItems = new List<InventoryTree.InventoryItem>();

                GetWornItems(sharedFolder, wearableType, wornItems);
                var foldersToLock = GetFoldersForItems(sharedFolderMap, wornItems);

                foreach (var folder in foldersToLock)
                {
                    AddLockedFolder(folder, restriction);
                }
            }
            else if (restriction.Args[0] is AttachmentPoint attachmentPoint)
            {
                var attachedItems = new List<InventoryTree.InventoryItem>();

                GetAttachedItems(sharedFolder, attachmentPoint, attachedItems);
                var foldersToLock = GetFoldersForItems(sharedFolderMap, attachedItems);

                foreach (var folder in foldersToLock)
                {
                    AddLockedFolder(folder, restriction);
                }
            }
            else if (restriction.Args[0] is string path)
            {
                if (!TryGetFolderFromPath(path, sharedFolder, out var folder))
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
                        ProcessFolderRestrictions(newCommand);
                        break;
                    case RLVRestrictionType.DetachThisExcept:
                    case RLVRestrictionType.DetachAllThisExcept:
                    case RLVRestrictionType.AttachThisExcept:
                    case RLVRestrictionType.AttachAllThisExcept:
                        ProcessFolderException(newCommand);
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
                        RebuildLockedFolders();
                        break;
                    case RLVRestrictionType.DetachThisExcept:
                    case RLVRestrictionType.DetachAllThisExcept:
                    case RLVRestrictionType.AttachThisExcept:
                    case RLVRestrictionType.AttachAllThisExcept:
                        RebuildLockedFolders();
                        break;
                }
            }

            return true;
        }
    }
}
