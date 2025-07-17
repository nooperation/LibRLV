using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace LibRLV
{
    public class RLVRestrictionHandler : IRestrictionProvider
    {
        internal static readonly ImmutableDictionary<string, RLVRestrictionType> NameToRestrictionMap = new Dictionary<string, RLVRestrictionType>(StringComparer.OrdinalIgnoreCase)
        {
            { "notify", RLVRestrictionType.Notify },
            { "permissive", RLVRestrictionType.Permissive },
            { "fly", RLVRestrictionType.Fly },
            { "temprun", RLVRestrictionType.TempRun },
            { "alwaysrun", RLVRestrictionType.AlwaysRun },
            { "camzoommax", RLVRestrictionType.CamZoomMax },
            { "camzoommin", RLVRestrictionType.CamZoomMin },
            { "setcam_fovmin", RLVRestrictionType.SetCamFovMin },
            { "setcam_fovmax", RLVRestrictionType.SetCamFovMax },
            { "camdistmax", RLVRestrictionType.CamDistMax },
            { "setcam_avdistmax", RLVRestrictionType.SetCamAvDistMax },
            { "camdistmin", RLVRestrictionType.CamDistMin },
            { "setcam_avdistmin", RLVRestrictionType.SetCamAvDistMin },
            { "camdrawalphamax", RLVRestrictionType.CamDrawAlphaMax },
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
        }.ToImmutableDictionary();

        internal static readonly ImmutableDictionary<RLVRestrictionType, string> RestrictionToNameMap = NameToRestrictionMap
            .ToImmutableDictionary(k => k.Value, v => v.Key);

        public event EventHandler<RestrictionUpdatedEventArgs> RestrictionUpdated;

        private readonly Dictionary<RLVRestrictionType, List<RLVRestriction>> _currentRestrictions = new Dictionary<RLVRestrictionType, List<RLVRestriction>>();

        public RLVRestrictionHandler()
        {
            
        }

        public List<RLVRestriction> GetRestrictions(string filter = "", UUID? sender = null)
        {
            var restrictions = new List<RLVRestriction>();

            var sb = new StringBuilder();
            foreach (var item in _currentRestrictions)
            {
                if (!RestrictionToNameMap.TryGetValue(item.Key, out var behaviorName))
                {
                    throw new Exception($"_currentRestrictions has a behavior '{item.Key.ToString()}' that is not defined in the reverse behavior map");
                }

                if (!behaviorName.Contains(filter))
                {
                    continue;
                }

                foreach (var restriction in item.Value)
                {
                    if (sender != null && restriction.Sender != sender)
                    {
                        continue;
                    }

                    restrictions.Add(restriction);
                }
            }

            return restrictions;
        }

        private void AddOrUpdateRestriction(RLVRestriction newCommand)
        {
            if (!_currentRestrictions.TryGetValue(newCommand.Behavior, out var restrictions))
            {
                _currentRestrictions.Add(newCommand.Behavior, new List<RLVRestriction>()
                {
                    newCommand
                });

                return;
            }

            var existingRestriction = restrictions
                .Where(n => n.Sender == newCommand.Sender && n.Args.SequenceEqual(newCommand.Args))
                .FirstOrDefault();
            if (existingRestriction == null)
            {
                restrictions.Add(newCommand);
                RestrictionUpdated?.Invoke(this, new RestrictionUpdatedEventArgs()
                {
                    IsDeleted = false,
                    IsNew = true,
                    Restriction = newCommand
                });
                return;
            }

            if (existingRestriction.IsException != newCommand.IsException)
            {
                existingRestriction.IsException = newCommand.IsException;
                RestrictionUpdated?.Invoke(this, new RestrictionUpdatedEventArgs()
                {
                    IsDeleted = false,
                    IsNew = false,
                    Restriction = newCommand
                });
            }
        }

        public void RemoveRestrictionsRelatedToObjects(ICollection<UUID> objectIds)
        {
            var objectsMap = objectIds.ToImmutableHashSet();

            foreach (var item in _currentRestrictions)
            {
                item.Value.RemoveAll(n => objectsMap.Contains(n.Sender));
            }
        }

        internal bool ProcessClearCommand(RLVMessage command)
        {
            var filteredRestrictions = RestrictionToNameMap
                .Where(n => n.Value.Contains(command.Option))
                .Select(n => n.Key)
                .ToList();

            foreach (var restrictionType in filteredRestrictions)
            {
                if (!_currentRestrictions.TryGetValue(restrictionType, out var restrictions))
                {
                    continue;
                }

                foreach (var restriction in restrictions)
                {
                    RestrictionUpdated?.Invoke(this, new RestrictionUpdatedEventArgs()
                    {
                        IsDeleted = true,
                        IsNew = false,
                        Restriction = restriction
                    });
                }
                restrictions.Clear();

                _currentRestrictions.Remove(restrictionType);
            }

            return true;
        }

        internal bool ProcessRestrictionCommand(RLVMessage message, string option, bool isException)
        {
            if (!NameToRestrictionMap.TryGetValue(message.Behavior, out var behavior))
            {
                return false;
            }

            var args = RLVCommon.ParseOptions(option);
            var newCommand = new RLVRestriction(behavior, isException, message.Sender, message.SenderName, args);

            if (!newCommand.Validate())
            {
                return false;
            }

            AddOrUpdateRestriction(newCommand);
            return true;
        }
    }
}
