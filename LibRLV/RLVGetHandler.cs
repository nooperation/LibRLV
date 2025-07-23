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
        private readonly ImmutableDictionary<string, RLVDataRequest> RLVDataRequestToNameMap;
        private readonly IRestrictionProvider _restrictions;
        private readonly IBlacklistProvider _blacklist;
        private readonly IRLVCallbacks _callbacks;

        internal RLVGetHandler(IBlacklistProvider blacklist, IRestrictionProvider restrictions, IRLVCallbacks callbacks)
        {
            _restrictions = restrictions;
            _blacklist = blacklist;
            _callbacks = callbacks;

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
                if (!RLVRestrictionHandler.RestrictionToNameMap.TryGetValue(restriction.OriginalBehavior, out var behaviorName))
                {
                    continue;
                }

                sb.Append($"{separator}{behaviorName}");
                if(restriction.Args.Count > 0)
                {
                    sb.Append($":{string.Join(";", restriction.Args)}");
                }
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
                    response = RLV.RLVVersion;
                    break;
                case "versionnum":
                    response = RLV.RLVVersionNum;
                    break;
                case "versionnumbl":
                    if (blacklist.Count > 0)
                    {
                        response = $"{RLV.RLVVersionNum},{string.Join(",", blacklist)}";
                    }
                    else
                    {
                        response = RLV.RLVVersionNum;
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
                    {
                        if (!Enum.TryParse<WearableType>(rlvMessage.Option, out var part))
                        {
                            return false;
                        }
                        args.Add(part);
                        break;
                    }
                    case RLVDataRequest.GetAttach:
                    {
                        if (!Enum.TryParse<AttachmentPoint>(rlvMessage.Option, out var attachmentPoint))
                        {
                            return false;
                        }
                        args.Add(attachmentPoint);
                        break;
                    }
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
                    {
                        // [uuid | layer | attachpt ]

                        var result = new List<object>();
                        var parsedOptions = rlvMessage.Option.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parsedOptions.Length != 1)
                        {
                            return false;
                        }

                        if (UUID.TryParse(parsedOptions[0], out var uuid))
                        {
                            args.Add(uuid);
                        }
                        else if (Enum.TryParse<WearableType>(parsedOptions[0], out var wearableType))
                        {
                            args.Add(wearableType);
                        }
                        else if (Enum.TryParse<AttachmentPoint>(parsedOptions[0], out var attachmentPoint))
                        {
                            args.Add(attachmentPoint);
                        }
                        else
                        {
                            return false;
                        }

                        return true;
                    }
                }

                response = _callbacks.ProvideDataAsync(name, args, CancellationToken.None).Result;
            }
            else if (rlvMessage.Behavior.StartsWith("getdebug_"))
            {
                var commandRaw = rlvMessage.Behavior.Substring("getdebug_".Length);
                if (Enum.TryParse<RLVGetDebugType>(commandRaw, true, out var command))
                {
                    response = _callbacks.GetDebugInfoAsync(command).Result;
                }
            }
            else if (rlvMessage.Behavior.StartsWith("getenv_"))
            {
                var commandRaw = rlvMessage.Behavior.Substring("getenv_".Length);
                if (Enum.TryParse<RLVGetEnvType>(commandRaw, true, out var command))
                {
                    response = _callbacks.GetEnvironmentAsync(command).Result;
                }
            }

            if (response != null)
            {
                _callbacks.SendReplyAsync(channel, response, CancellationToken.None).Wait();
                return true;
            }

            return false;
        }

        internal bool ProcessInstantMessageCommand(string message, UUID senderId, string senderName)
        {
            switch (message)
            {
                case "@version":
                    _callbacks.SendInstantMessageAsync(senderId, RLV.RLVVersion, CancellationToken.None).Wait();
                    return true;
                case "@getblacklist":
                    var blacklist = _blacklist.GetBlacklist();
                    _callbacks.SendInstantMessageAsync(senderId, string.Join(",", blacklist), CancellationToken.None).Wait();
                    return true;
            }

            return false;
        }
    }
}
