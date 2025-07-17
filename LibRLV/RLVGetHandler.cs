using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibRLV
{
    public class RLVGetHandler
    {
        private string RLVVersion => "RestrainedLove viewer v3.4.3 (RLVa 2.4.2)";
        private string RLVVersionNum => "2040213";

        public Func<RLVDataRequest, List<object>, CancellationToken, Task<string>> DataProviderAsync { get; set; }
        public Func<int, string, CancellationToken, Task> SendReplyAsync { get; set; }

        public Func<RLVGetEnvType, Task<string>> HandleGetEnv { get; set; }
        public Func<RLVGetDebugType, Task<string>> HandleGetDebug { get; set; }

        private readonly ImmutableDictionary<string, RLVDataRequest> RLVDataRequestToNameMap;
        private readonly IRestrictionProvider _restrictions;
        private readonly IBlacklistProvider _blacklist;

        internal RLVGetHandler(IBlacklistProvider blacklist, IRestrictionProvider restrictions)
        {
            _restrictions = restrictions;
            _blacklist = blacklist;

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

            var restrictions = _restrictions.GetRestrictions(filter, sender);
            StringBuilder sb = new StringBuilder();
            foreach (var restriction in restrictions)
            {
                if (!RLVRestrictionHandler.RestrictionToNameMap.TryGetValue(restriction.Behavior, out var behaviorName))
                {
                    continue;
                }

                sb.Append($"{separator}{behaviorName}:{string.Join(";", restriction.Args)}");
            }

            return sb.ToString();
        }

        public bool ProcessGetCommand(RLVMessage rlvMessage, int channel)
        {
            var blacklist = _blacklist.GetBlacklist();

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
                    if (blacklist.Count > 0)
                    {
                        response = $"{RLVVersionNum},{string.Join(",", blacklist)}";
                    }
                    else
                    {
                        response = RLVVersionNum;
                    }
                    break;
                case "getblacklist":
                    var filteredBlacklist = blacklist
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
                        var parsedOptions = RLVCommon.ParseOptions(rlvMessage.Option);
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
        
        internal bool ProcessInstantMessageCommand(string message, UUID senderId, string senderName, Action<UUID, string> instantMessageReply)
        {
            switch (message)
            {
                case "@version":
                    instantMessageReply?.Invoke(senderId, RLVVersion);
                    return true;
                case "@getblacklist":
                    var blacklist = _blacklist.GetBlacklist();
                    instantMessageReply?.Invoke(senderId, string.Join(",", _blacklist));
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
    }
}
