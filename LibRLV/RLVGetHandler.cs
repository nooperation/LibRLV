using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

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
                if (restriction.Args.Count > 0)
                {
                    sb.Append($":{string.Join(";", restriction.Args)}");
                }
            }

            return sb.ToString();
        }

        private bool ProcessGetOutfit(WearableType? specificType, out string response)
        {
            if (!_callbacks.TryGetCurrentOutfit(out var currentOutfit).Result)
            {
                response = string.Empty;
                return false;
            }

            if (specificType != null)
            {
                if (currentOutfit.Where(n => n.WornOn == specificType).Any())
                {
                    response = "1";
                }
                else
                {
                    response = "0";
                }

                return true;
            }

            var wornTypes = currentOutfit
                .Where(n => n.WornOn != null)
                .Select(n => n.WornOn)
                .Distinct()
                .ToDictionary(k => k.Value, v => v.Value);

            var sb = new StringBuilder();

            // gloves,jacket,pants,shirt,shoes,skirt,socks,underpants,undershirt,skin,eyes,hair,shape,alpha,tattoo,physics
            sb.Append(wornTypes.ContainsKey(WearableType.Gloves) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Jacket) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Pants) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Shirt) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Shoes) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Skirt) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Socks) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Underpants) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Undershirt) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Skin) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Eyes) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Hair) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Shape) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Alpha) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Tattoo) ? "1" : "0");
            sb.Append(wornTypes.ContainsKey(WearableType.Physics) ? "1" : "0");

            response = sb.ToString();
            return true;
        }

        private bool ProcessGetAttach(AttachmentPoint? specificType, out string response)
        {
            if (!_callbacks.TryGetCurrentOutfit(out var currentOutfit).Result)
            {
                response = string.Empty;
                return false;
            }

            if (specificType != null)
            {
                if (currentOutfit.Where(n => n.AttachedTo == specificType).Any())
                {
                    response = "1";
                }
                else
                {
                    response = "0";
                }

                return true;
            }

            var wornTypes = currentOutfit
                .Where(n => n.AttachedTo != null)
                .Select(n => n.AttachedTo)
                .Distinct()
                .ToDictionary(k => k.Value, v => v.Value);

            var attachmentPointTypes = Enum.GetValues(typeof(AttachmentPoint));
            var sb = new StringBuilder(attachmentPointTypes.Length);

            // digit corresponds directly to the value of enum, unlike ProcessGetOutfit
            foreach (AttachmentPoint attachmentPoint in attachmentPointTypes)
            {
                sb.Append(wornTypes.ContainsKey(attachmentPoint) ? '1' : '0');
            }

            response = sb.ToString();
            return true;
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
                    case RLVDataRequest.GetSitId:
                        if (!_callbacks.TryGetSitId(out var sitId).Result || sitId == UUID.Zero)
                        {
                            response = "NULL_KEY";
                        }
                        else
                        {
                            response = sitId.ToString();
                        }
                        break;
                    case RLVDataRequest.GetCamAvDistMin:
                        if (!_callbacks.TryGetCamAvDistMin(out var camAvDistMin).Result)
                        {
                            return false;
                        }
                        response = camAvDistMin.ToString();
                        break;
                    case RLVDataRequest.GetCamAvDistMax:
                        if (!_callbacks.TryGetCamAvDistMax(out var camAvDistMax).Result)
                        {
                            return false;
                        }
                        response = camAvDistMax.ToString();
                        break;
                    case RLVDataRequest.GetCamFovMin:
                        if (!_callbacks.TryGetCamFovMin(out var camFovMin).Result)
                        {
                            return false;
                        }
                        response = camFovMin.ToString();
                        break;

                    case RLVDataRequest.GetCamFovMax:
                        if (!_callbacks.TryGetCamFovMax(out var camFovMax).Result)
                        {
                            return false;
                        }
                        response = camFovMax.ToString();
                        break;
                    case RLVDataRequest.GetCamZoomMin:
                        if (!_callbacks.TryGetCamZoomMin(out var camZoomMin).Result)
                        {
                            return false;
                        }
                        response = camZoomMin.ToString();
                        break;
                    case RLVDataRequest.GetCamFov:
                        if (!_callbacks.TryGetCamFov(out var camFov).Result)
                        {
                            return false;
                        }
                        response = camFov.ToString();
                        break;
                    case RLVDataRequest.GetGroup:
                        if (!_callbacks.TryGetGroup(out var activeGroupName).Result)
                        {
                            response = "none";
                        }
                        else
                        {
                            response = activeGroupName;
                        }
                        break;
                    case RLVDataRequest.GetOutfit:
                    {
                        WearableType? wearableType = null;
                        if (RLVCommon.RLVWearableTypeMap.TryGetValue(rlvMessage.Option, out var wearableTypeTemp))
                        {
                            wearableType = wearableTypeTemp;
                        }

                        if (!ProcessGetOutfit(wearableType, out response))
                        {
                            return false;
                        }

                        break;
                    }
                    case RLVDataRequest.GetAttach:
                    {
                        AttachmentPoint? attachmentPointType = null;
                        if (RLVCommon.RLVAttachmentPointMap.TryGetValue(rlvMessage.Option, out var attachmentPointTemp))
                        {
                            attachmentPointType = attachmentPointTemp;
                        }

                        if (!ProcessGetAttach(attachmentPointType, out response))
                        {
                            return false;
                        }
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
                        else if (RLVCommon.RLVWearableTypeMap.TryGetValue(parsedOptions[0], out var wearableType))
                        {
                            args.Add(wearableType);
                        }
                        else if (RLVCommon.RLVAttachmentPointMap.TryGetValue(parsedOptions[0], out var attachmentPoint))
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

                if (response == null)
                {
                    response = _callbacks.ProvideDataAsync(name, args, CancellationToken.None).Result;
                }
            }
            else if (rlvMessage.Behavior.StartsWith("getdebug_"))
            {
                var commandRaw = rlvMessage.Behavior.Substring("getdebug_".Length);
                response = _callbacks.GetDebugInfoAsync(commandRaw).Result;
            }
            else if (rlvMessage.Behavior.StartsWith("getenv_"))
            {
                var commandRaw = rlvMessage.Behavior.Substring("getenv_".Length);
                response = _callbacks.GetEnvironmentAsync(commandRaw).Result;
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
