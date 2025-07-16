using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LibRLV
{
    public class RLV
    {

        private string RLVVersion => "RestrainedLove viewer v3.4.3 (RLVa 2.4.2)";
        private string RLVVersionNum => "2040213";
        public bool Enabled { get; set; }

        private List<string> Blacklist = new List<string>();

        public Func<int, string, CancellationToken, Task> SendReplyAsync { get; set; }
        public Func<RLVDataRequest, List<object>, CancellationToken, Task<string>> DataProviderAsync { get; set; }

        public Func<RLVGetEnvType, Task<string>> HandleGetEnv { get; set; }
        public Func<RLVGetDebugType, Task<string>> HandleGetDebug { get; set; }

        public RLVActionHandler Actions { get; }

        public RLV()
        {
            Actions = new RLVActionHandler();

            RLVDataRequestToNameMap = new Dictionary<string, RLVDataRequest>()
            {
                { "getcam_avdistmin", RLVDataRequest.GetCamAvDistMin },
                { "getcam_avdistmax", RLVDataRequest.GetCamAvDistMax },
                { "getcam_fovmin", RLVDataRequest.GetCamFovMin },
                { "getcam_fovmax", RLVDataRequest.GetCamFovMax },
                { "getcam_zoommin", RLVDataRequest.GetCamZoomMin },
                { "getcam_fov", RLVDataRequest.GetCamFov },
                { "getsitid", RLVDataRequest.GetSitId },
                { "getoutfit", RLVDataRequest.GetOutfit },
                { "getattach", RLVDataRequest.GetAttach },
                { "getinv", RLVDataRequest.GetInv },
                { "getinvworn", RLVDataRequest.GetInvWorn },
                { "findfolder", RLVDataRequest.FindFolder },
                { "findfolders", RLVDataRequest.FindFolders },
                { "getpath", RLVDataRequest.GetPath },
                { "getpathnew", RLVDataRequest.GetPathNew },
                { "getgroup", RLVDataRequest.GetGroup }
            }.ToImmutableDictionary();
        }

        #region SETTERS

        private static readonly ImmutableDictionary<string, RLVRestriction> BehaviorMap = new Dictionary<string, RLVRestriction>(StringComparer.OrdinalIgnoreCase)
        {
            { "notify", RLVRestriction.Notify },
            { "permissive", RLVRestriction.Permissive },
            { "fly", RLVRestriction.Fly },
            { "temprun", RLVRestriction.TempRun },
            { "alwaysrun", RLVRestriction.AlwaysRun },
            { "camzoommax", RLVRestriction.CamZoomMax },
            { "camzoommin", RLVRestriction.CamZoomMin },
            { "setcam_fovmin", RLVRestriction.SetCamFovMin },
            { "setcam_fovmax", RLVRestriction.SetCamFovMax },
            { "camdistmax", RLVRestriction.CamDistMax },
            { "setcam_avdistmax", RLVRestriction.SetCamAvDistMax },
            { "camdistmin", RLVRestriction.CamDistMin },
            { "setcam_avdistmin", RLVRestriction.SetCamAvDistMin },
            { "camdrawalphamax", RLVRestriction.CamDrawAlphaMax },
            { "camdrawcolor", RLVRestriction.CamDrawColor },
            { "camunlock", RLVRestriction.CamUnlock },
            { "setcam_unlock", RLVRestriction.SetCamUnlock },
            { "camavdist", RLVRestriction.CamAvDist },
            { "camtextures", RLVRestriction.CamTextures },
            { "setcam_textures", RLVRestriction.SetCamTextures },
            { "sendchat", RLVRestriction.SendChat },
            { "chatshout", RLVRestriction.ChatShout },
            { "chatnormal", RLVRestriction.ChatNormal },
            { "chatwhisper", RLVRestriction.ChatWhisper },
            { "redirchat", RLVRestriction.RedirChat },
            { "recvchat", RLVRestriction.RecvChat },
            { "recvchat_sec", RLVRestriction.RecvChatSec },
            { "recvchatfrom", RLVRestriction.RecvChatFrom },
            { "sendgesture", RLVRestriction.SendGesture },
            { "emote", RLVRestriction.Emote },
            { "rediremote", RLVRestriction.RedirEmote },
            { "recvemote", RLVRestriction.RecvEmote },
            { "recvemotefrom", RLVRestriction.RecvEmoteFrom },
            { "recvemote_sec", RLVRestriction.RecvEmoteSec },
            { "sendchannel", RLVRestriction.SendChannel },
            { "sendchannel_sec", RLVRestriction.SendChannelSec },
            { "sendchannel_except", RLVRestriction.SendChannelExcept },
            { "sendim", RLVRestriction.SendIm },
            { "sendim_sec", RLVRestriction.SendImSec },
            { "sendimto", RLVRestriction.SendImTo },
            { "startim", RLVRestriction.StartIm },
            { "startimto", RLVRestriction.StartImTo },
            { "recvim", RLVRestriction.RecvIm },
            { "recvim_sec", RLVRestriction.RecvImSec },
            { "recvimfrom", RLVRestriction.RecvImFrom },
            { "tplocal", RLVRestriction.TpLocal },
            { "tplm", RLVRestriction.TpLm },
            { "tploc", RLVRestriction.TpLoc },
            { "tplure", RLVRestriction.TpLure },
            { "tplure_sec", RLVRestriction.TpLureSec },
            { "sittp", RLVRestriction.SitTp },
            { "standtp", RLVRestriction.StandTp },
            { "accepttp", RLVRestriction.AcceptTp },
            { "accepttprequest", RLVRestriction.AcceptTpRequest },
            { "tprequest", RLVRestriction.TpRequest },
            { "tprequest_sec", RLVRestriction.TpRequestSec },
            { "showinv", RLVRestriction.ShowInv },
            { "viewnote", RLVRestriction.ViewNote },
            { "viewscript", RLVRestriction.ViewScript },
            { "viewtexture", RLVRestriction.ViewTexture },
            { "edit", RLVRestriction.Edit },
            { "rez", RLVRestriction.Rez },
            { "editobj", RLVRestriction.EditObj },
            { "editworld", RLVRestriction.EditWorld },
            { "editattach", RLVRestriction.EditAttach },
            { "share", RLVRestriction.Share },
            { "share_sec", RLVRestriction.ShareSec },
            { "unsit", RLVRestriction.Unsit },
            { "sit", RLVRestriction.Sit },
            { "detach", RLVRestriction.Detach },
            { "addattach", RLVRestriction.AddAttach },
            { "remattach", RLVRestriction.RemAttach },
            { "defaultwear", RLVRestriction.DefaultWear },
            { "addoutfit", RLVRestriction.AddOutfit },
            { "remoutfit", RLVRestriction.RemOutfit },
            { "acceptpermission", RLVRestriction.AcceptPermission },
            { "denypermission", RLVRestriction.DenyPermission },
            { "unsharedwear", RLVRestriction.UnsharedWear },
            { "unsharedunwear", RLVRestriction.UnsharedUnwear },
            { "sharedwear", RLVRestriction.SharedWear },
            { "sharedunwear", RLVRestriction.SharedUnwear },
            { "detachthis", RLVRestriction.DetachThis },
            { "detachallthis", RLVRestriction.DetachAllThis },
            { "attachthis", RLVRestriction.AttachThis },
            { "attachallthis", RLVRestriction.AttachAllThis },
            { "detachthis_except", RLVRestriction.DetachThisExcept },
            { "detachallthis_except", RLVRestriction.DetachAllThisExcept },
            { "attachthis_except", RLVRestriction.AttachThisExcept },
            { "attachallthis_except", RLVRestriction.AttachAllThisExcept },
            { "fartouch", RLVRestriction.FarTouch },
            { "touchfar", RLVRestriction.TouchFar },
            { "touchall", RLVRestriction.TouchAll },
            { "touchworld", RLVRestriction.TouchWorld },
            { "touchthis", RLVRestriction.TouchThis },
            { "touchme", RLVRestriction.TouchMe },
            { "touchattach", RLVRestriction.TouchAttach },
            { "touchattachself", RLVRestriction.TouchAttachSelf },
            { "touchattachother", RLVRestriction.TouchAttachOther },
            { "touchhud", RLVRestriction.TouchHud },
            { "interact", RLVRestriction.Interact },
            { "showworldmap", RLVRestriction.ShowWorldMap },
            { "showminimap", RLVRestriction.ShowMiniMap },
            { "showloc", RLVRestriction.ShowLoc },
            { "shownames", RLVRestriction.ShowNames },
            { "shownames_sec", RLVRestriction.ShowNamesSec },
            { "shownametags", RLVRestriction.ShowNameTags },
            { "shownearby", RLVRestriction.ShowNearby },
            { "showhovertextall", RLVRestriction.ShowHoverTextAll },
            { "showhovertext", RLVRestriction.ShowHoverText },
            { "showhovertexthud", RLVRestriction.ShowHoverTextHud },
            { "showhovertextworld", RLVRestriction.ShowHoverTextWorld },
            { "setgroup", RLVRestriction.SetGroup },
            { "setdebug", RLVRestriction.SetDebug },
            { "setenv", RLVRestriction.SetEnv },
            { "allowidle", RLVRestriction.AllowIdle },
        }.ToImmutableDictionary();

        private readonly ImmutableDictionary<RLVRestriction, string> BehaviorMapReverse = BehaviorMap
            .ToImmutableDictionary(k => k.Value, v => v.Key);

        private readonly Dictionary<RLVRestriction, List<RLVRule>> CurrentRules = new Dictionary<RLVRestriction, List<RLVRule>>();
        private readonly ImmutableDictionary<string, RLVDataRequest> RLVDataRequestToNameMap;

        public event EventHandler<RulesUpdatedEventArgs> RulesUpdated;

        private static List<object> ParseOptions(string options)
        {
            var result = new List<object>();
            var args = options.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var arg in args)
            {
                if (UUID.TryParse(arg, out var id))
                {
                    result.Add(id);
                    continue;
                }
                else if (int.TryParse(arg, out var intValue))
                {
                    result.Add(intValue);
                    continue;
                }
                else if (float.TryParse(arg, out var floatValue))
                {
                    result.Add(floatValue);
                    continue;
                }
                else if (Enum.TryParse(arg, true, out WearableType part) && part != WearableType.Invalid)
                {
                    result.Add(part);
                    continue;
                }
                else if (Enum.TryParse(arg, true, out AttachmentPoint attachmentPoint))
                {
                    result.Add(attachmentPoint);
                    continue;
                }
                else
                {
                    result.Add(arg);
                }
            }

            return result;
        }

        private void AddOrUpdateRule(RLVRule newCommand)
        {
            if (!CurrentRules.TryGetValue(newCommand.Behavior, out var rules))
            {
                CurrentRules.Add(newCommand.Behavior, new List<RLVRule>()
                {
                    newCommand
                });

                return;
            }

            var existingRule = rules
                .Where(n => n.Sender == newCommand.Sender && n.Args.SequenceEqual(newCommand.Args))
                .FirstOrDefault();
            if (existingRule == null)
            {
                rules.Add(newCommand);
                RulesUpdated?.Invoke(this, new RulesUpdatedEventArgs()
                {
                    IsDeleted = false,
                    IsNewRule = true,
                    Rule = newCommand
                });
                return;
            }

            if (existingRule.IsException != newCommand.IsException)
            {
                existingRule.IsException = newCommand.IsException;
                RulesUpdated?.Invoke(this, new RulesUpdatedEventArgs()
                {
                    IsDeleted = false,
                    IsNewRule = false,
                    Rule = newCommand
                });
            }
        }

        public void RemoveRulesFromObjects(ICollection<UUID> objectIds)
        {
            var objectsMap = objectIds.ToImmutableHashSet();

            foreach (var item in CurrentRules)
            {
                item.Value.RemoveAll(n => objectsMap.Contains(n.Sender));
            }
        }

        private bool ProcessSetCommand(RLVMessage message, string option, bool isException)
        {
            if (!BehaviorMap.TryGetValue(message.Behavior, out var behavior))
            {
                return false;
            }

            var args = ParseOptions(option);
            var newCommand = new RLVRule(behavior, isException, message.Sender, message.SenderName, args);

            if (!newCommand.Validate())
            {
                return false;
            }

            AddOrUpdateRule(newCommand);
            return true;
        }

        #endregion


        #region GETTERS

        private string HandleGetStatus(string option, UUID? sender)
        {
            var parts = option.Split(';');
            var filter = string.Empty;
            var separator = "/";

            if (parts.Length > 0)
            {
                filter = parts[0].ToLower();
            }
            if (parts.Length > 1)
            {
                separator = parts[1];
            }

            StringBuilder sb = new StringBuilder();
            foreach (var item in CurrentRules)
            {
                if (!BehaviorMapReverse.TryGetValue(item.Key, out var behaviorName))
                {
                    throw new Exception($"CurrentRules has a behavior '{item.Key.ToString()}' that is not defined in the reverse behavior map");
                }

                if (!behaviorName.Contains(filter))
                {
                    continue;
                }

                foreach (var rule in item.Value)
                {
                    if (sender != null && rule.Sender != sender)
                    {
                        continue;
                    }

                    sb.Append($"{separator}{behaviorName}:{string.Join(";", rule.Args)}");
                }
            }

            return sb.ToString();
        }

        private bool ProcessGetCommand(RLVMessage rlvMessage, int channel)
        {
            string response = null;
            switch (rlvMessage.Behavior)
            {
                case "version":
                case "versionnew":
                    response = RLVVersion;
                    break;
                case "versionnum":
                    response = RLVVersionNum;
                    break;
                case "versionnumbl":
                    if (Blacklist.Count > 0)
                    {
                        response = $"{RLVVersionNum},{string.Join(",", Blacklist)}";
                    }
                    else
                    {
                        response = RLVVersionNum;
                    }
                    break;
                case "getblacklist":
                    var filteredBlacklist = Blacklist
                        .Where(n => n.Contains(rlvMessage.Option));
                    response = string.Join(",", filteredBlacklist);
                    break;
                case "getstatus":
                    response = HandleGetStatus(rlvMessage.Option, rlvMessage.Sender);
                    break;
                case "getstatusall":
                    response = HandleGetStatus(rlvMessage.Option, null);
                    break;
            }

            if (RLVDataRequestToNameMap.TryGetValue(rlvMessage.Behavior, out var name))
            {
                var args = new List<object>();

                switch (name)
                {
                    case RLVDataRequest.GetOutfit:
                        if (!Enum.TryParse<WearableType>(rlvMessage.Option, out var part))
                        {
                            return false;
                        }
                        args.Add(part);
                        break;
                    case RLVDataRequest.GetAttach:
                        if (!Enum.TryParse<AttachmentPoint>(rlvMessage.Option, out var attachmentPoint))
                        {
                            return false;
                        }
                        args.Add(attachmentPoint);
                        break;
                    case RLVDataRequest.GetInv:
                    case RLVDataRequest.GetInvWorn:
                        args.Add(rlvMessage.Option);
                        break;
                    case RLVDataRequest.FindFolder:
                        args.Add(rlvMessage.Option);
                        break;
                    case RLVDataRequest.FindFolders:
                        args.AddRange(rlvMessage.Option.Split(';'));
                        break;
                    case RLVDataRequest.GetPath:
                    case RLVDataRequest.GetPathNew:
                        var parsedOptions = ParseOptions(rlvMessage.Option);
                        if (parsedOptions.Count != 1)
                        {
                            return false;
                        }
                        if (!parsedOptions.All(n => n is UUID || n is WearableType || n is AttachmentPoint))
                        {
                            return false;
                        }

                        args.AddRange(parsedOptions);
                        break;
                }

                if (DataProviderAsync == null)
                {
                    response = string.Empty;
                }
                else
                {
                    response = DataProviderAsync(name, args, CancellationToken.None).Result;
                }
            }
            else if (rlvMessage.Behavior.StartsWith("getdebug_"))
            {
                var commandRaw = rlvMessage.Behavior.Substring("getdebug_".Length);
                if (Enum.TryParse<RLVGetDebugType>(commandRaw, true, out var command))
                {
                    if (HandleGetDebug != null)
                    {
                        response = HandleGetDebug(command).Result;
                    }
                    else
                    {
                        response = ProcessGetDebug(command);
                    }
                }
            }
            else if (rlvMessage.Behavior.StartsWith("getenv_"))
            {
                var commandRaw = rlvMessage.Behavior.Substring("getenv_".Length);
                if (Enum.TryParse<RLVGetEnvType>(commandRaw, true, out var command))
                {
                    if (HandleGetEnv != null)
                    {
                        response = HandleGetEnv(command).Result;
                    }
                    else
                    {
                        response = ProcessGetEnv(command);
                    }
                }
            }

            if (response != null)
            {
                SendReplyAsync?.Invoke(channel, response, CancellationToken.None).Wait();
                return true;
            }

            return false;
        }

        private static string ProcessGetEnv(RLVGetEnvType command)
        {
            switch (command)
            {
                case RLVGetEnvType.Daytime:
                case RLVGetEnvType.AmbientR:
                case RLVGetEnvType.AmbientG:
                case RLVGetEnvType.AmbientB:
                case RLVGetEnvType.AmbientI:
                case RLVGetEnvType.BlueDensityR:
                case RLVGetEnvType.BlueDensityG:
                case RLVGetEnvType.BlueDensityB:
                case RLVGetEnvType.BlueDensityI:
                case RLVGetEnvType.BlueHorizonR:
                case RLVGetEnvType.BlueHorizonG:
                case RLVGetEnvType.BlueHorizonB:
                case RLVGetEnvType.BlueHorizonI:
                case RLVGetEnvType.CloudColorR:
                case RLVGetEnvType.CloudColorG:
                case RLVGetEnvType.CloudColorB:
                case RLVGetEnvType.CloudColorI:
                case RLVGetEnvType.CloudCoverage:
                case RLVGetEnvType.CloudX:
                case RLVGetEnvType.CloudY:
                case RLVGetEnvType.CloudD:
                case RLVGetEnvType.CloudDetailX:
                case RLVGetEnvType.CloudDetailY:
                case RLVGetEnvType.CloudDetailD:
                case RLVGetEnvType.CloudScale:
                case RLVGetEnvType.CloudScrollX:
                case RLVGetEnvType.CloudScrollY:
                case RLVGetEnvType.CloudVariance:
                case RLVGetEnvType.DensityMultiplier:
                case RLVGetEnvType.DistanceMultiplier:
                case RLVGetEnvType.DropletRadius:
                case RLVGetEnvType.EastAngle:
                case RLVGetEnvType.IceLevel:
                case RLVGetEnvType.HazeDensity:
                case RLVGetEnvType.HazeHorizon:
                case RLVGetEnvType.MaxAltitude:
                case RLVGetEnvType.MoistureLevel:
                case RLVGetEnvType.MoonAzim:
                case RLVGetEnvType.MoonNBrightness:
                case RLVGetEnvType.MoonElev:
                case RLVGetEnvType.MoonScale:
                case RLVGetEnvType.SceneGamma:
                case RLVGetEnvType.StarBrightness:
                case RLVGetEnvType.SunGlowFocus:
                case RLVGetEnvType.SunAzim:
                case RLVGetEnvType.SunElev:
                case RLVGetEnvType.SunScale:
                case RLVGetEnvType.SunMoonPosition:
                case RLVGetEnvType.SunMoonColorR:
                case RLVGetEnvType.SunMoonColorG:
                case RLVGetEnvType.SunMoonColorB:
                case RLVGetEnvType.SunMoonColorI:
                    return "0";

                case RLVGetEnvType.Ambient:
                case RLVGetEnvType.BlueDensity:
                case RLVGetEnvType.BlueHorizon:
                case RLVGetEnvType.CloudColor:
                case RLVGetEnvType.Cloud:
                case RLVGetEnvType.CloudDetail:
                case RLVGetEnvType.SunMoonColor:
                    return "0;0;0";

                case RLVGetEnvType.CloudScroll:
                    return "0;0";

                case RLVGetEnvType.Preset:
                case RLVGetEnvType.Asset:
                    return "";

                case RLVGetEnvType.MoonImage:
                case RLVGetEnvType.SunImage:
                case RLVGetEnvType.CloudImage:
                    return UUID.Zero.ToString();

                case RLVGetEnvType.SunGlowSize:
                    return "1";
            }

            return null;
        }

        private static string ProcessGetDebug(RLVGetDebugType command)
        {
            switch (command)
            {
                case RLVGetDebugType.AvatarSex:
                case RLVGetDebugType.RestrainedLoveForbidGiveToRLV:
                case RLVGetDebugType.WindLightUseAtmosShaders:
                    return "0";

                case RLVGetDebugType.RenderResolutionDivisor:
                case RLVGetDebugType.RestrainedLoveNoSetEnv:
                    return "1";
            }

            return null;
        }
        #endregion

        private bool ProcessRule(RLVMessage command)
        {
            if (Blacklist.Contains(command.Behavior))
            {
                return false;
            }

            if (command.Behavior == "clear")
            {
                return ProcessClearCommand(command);
            }
            else if (command.Param == "force")
            {
                return Actions.ProcessActionCommand(command);
            }
            else if (command.Param == "y" || command.Param == "n" || command.Param == "add" || command.Param == "rem")
            {
                return ProcessSetCommand(command, command.Option, command.Param == "n" || command.Param == "add");
            }
            else if (int.TryParse(command.Param, out int channel))
            {
                return ProcessGetCommand(command, channel);
            }

            return false;
        }

        private bool ProcessClearCommand(RLVMessage command)
        {
            var filteredRules = BehaviorMapReverse
                .Where(n => n.Value.Contains(command.Option))
                .Select(n => n.Key)
                .ToList();

            foreach (var ruleType in filteredRules)
            {
                if (!CurrentRules.TryGetValue(ruleType, out var rules))
                {
                    continue;
                }

                foreach (var rule in rules)
                {
                    RulesUpdated?.Invoke(this, new RulesUpdatedEventArgs()
                    {
                        IsDeleted = true,
                        IsNewRule = false,
                        Rule = rule
                    });
                }
                rules.Clear();

                CurrentRules.Remove(ruleType);
            }

            return true;
        }

        private readonly Regex RLVRegexPattern = new Regex(@"(?<behavior>[^:=]+)(:(?<option>[^=]*))?=(?<param>\w+)", RegexOptions.Compiled);
        private bool ProcessSingleMessage(string message, UUID senderId, string senderName)
        {
            var match = RLVRegexPattern.Match(message);
            if (!match.Success)
            {
                return false;
            }

            var command = new RLVMessage
            {
                Behavior = match.Groups["behavior"].ToString().ToLower(),
                Option = match.Groups["option"].ToString(),
                Param = match.Groups["param"].ToString().ToLower(),
                Sender = senderId,
                SenderName = senderName
            };

            return ProcessRule(command);
        }

        public bool ProcessMessage(string message, UUID senderId, string senderName)
        {
            if (!Enabled || !message.StartsWith("@"))
            {
                return false;
            }

            var result = true;
            foreach (var singleMessage in message.Substring(1).Split(','))
            {
                var isSuccessful = ProcessSingleMessage(singleMessage, senderId, senderName);
                if (!isSuccessful)
                {
                    result = false;
                }
            }

            return result;
        }
    }
}
