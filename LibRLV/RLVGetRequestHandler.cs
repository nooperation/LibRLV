using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace LibRLV
{
    internal class RLVGetRequestHandler
    {
        private readonly ImmutableDictionary<string, RLVDataRequest> RLVDataRequestToNameMap;
        private readonly IRestrictionProvider _restrictions;
        private readonly IBlacklistProvider _blacklist;
        private readonly IRLVCallbacks _callbacks;

        internal RLVGetRequestHandler(IBlacklistProvider blacklist, IRestrictionProvider restrictions, IRLVCallbacks callbacks)
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

        private string HandleGetStatus(string option, Guid? sender)
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

        internal bool ProcessGetCommand(RLVMessage rlvMessage, int channel)
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
                        if (!_callbacks.TryGetSitId(out var sitId).Result || sitId == Guid.Empty)
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
                        response = HandleGetInv(rlvMessage.Option);
                        break;
                    case RLVDataRequest.GetInvWorn:
                        response = HandleGetInvWorn(rlvMessage.Option);
                        break;
                    case RLVDataRequest.FindFolder:
                    case RLVDataRequest.FindFolders:
                    {
                        var findFolderParts = rlvMessage.Option.Split(';');
                        var separator = ",";
                        var searchTerms = findFolderParts[0]
                            .Split(new[] { "&&" }, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();

                        if (findFolderParts.Length > 1)
                        {
                            separator = findFolderParts[1];
                        }

                        response = HandleFindFolders(name == RLVDataRequest.FindFolder, searchTerms, separator);
                        break;
                    }
                    case RLVDataRequest.GetPath:
                    case RLVDataRequest.GetPathNew:
                    {
                        // [] | [uuid | layer | attachpt ]

                        var result = new List<object>();
                        var parsedOptions = rlvMessage.Option.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        if (parsedOptions.Count > 1)
                        {
                            return false;
                        }

                        if (parsedOptions.Count == 0)
                        {
                            response = HandleGetPath(name == RLVDataRequest.GetPath, rlvMessage.Sender, null, null);
                        }
                        else if (Guid.TryParse(parsedOptions[0], out var uuid))
                        {
                            response = HandleGetPath(name == RLVDataRequest.GetPath, uuid, null, null);
                        }
                        else if (RLVCommon.RLVWearableTypeMap.TryGetValue(parsedOptions[0], out var wearableType))
                        {
                            response = HandleGetPath(name == RLVDataRequest.GetPath, null, null, wearableType);
                        }
                        else if (RLVCommon.RLVAttachmentPointMap.TryGetValue(parsedOptions[0], out var attachmentPoint))
                        {
                            response = HandleGetPath(name == RLVDataRequest.GetPath, null, attachmentPoint, null);
                        }
                        else
                        {
                            return false;
                        }

                        break;
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

        private class InvWornInfoContainer
        {
            public string FolderName { get; }
            public string CountIndicator { get; }

            public InvWornInfoContainer(string folderName, string countIndicator)
            {
                FolderName = folderName;
                CountIndicator = countIndicator;
            }

            public override string ToString()
            {
                return $"{FolderName}|{CountIndicator}";
            }
        }
        private void GetInvWornInfo_Internal(InventoryTree folder, bool recursive, ref int totalItems, ref int totalItemsWorn)
        {
            totalItemsWorn += folder.Items.Count(n => n.AttachedTo != null || n.WornOn != null);
            totalItems += folder.Items.Count;

            if (recursive)
            {
                foreach (var child in folder.Children)
                {
                    GetInvWornInfo_Internal(child, recursive, ref totalItems, ref totalItemsWorn);
                }
            }
        }

        private string GetInvWornInfo(InventoryTree folder)
        {
            // 0 : No item is present in that folder
            // 1 : Some items are present in that folder, but none of them is worn
            // 2 : Some items are present in that folder, and some of them are worn
            // 3 : Some items are present in that folder, and all of them are worn

            var totalItemsWorn = 0;
            var totalItems = 0;
            GetInvWornInfo_Internal(folder, false, ref totalItems, ref totalItemsWorn);

            var result = "";
            if (totalItems == 0)
            {
                result += "0";
            }
            else if (totalItemsWorn == 0)
            {
                result += "1";
            }
            else if (totalItems != totalItemsWorn)
            {
                result += "2";
            }
            else
            {
                result += "3";
            }

            var totalItemsWornRecursive = 0;
            var totalItemsRecursive = 0;
            GetInvWornInfo_Internal(folder, true, ref totalItemsRecursive, ref totalItemsWornRecursive);

            if (totalItemsRecursive == 0)
            {
                result += "0";
            }
            else if (totalItemsWornRecursive == 0)
            {
                result += "1";
            }
            else if (totalItemsRecursive != totalItemsWornRecursive)
            {
                result += "2";
            }
            else
            {
                result += "3";
            }

            return result;
        }

        private string HandleGetInvWorn(string args)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return string.Empty;
            }

            var inventoryMap = new InventoryMap(sharedFolder);
            var folders = new List<InventoryTree>();

            var target = sharedFolder;
            if (args.Length != 0)
            {
                if (!inventoryMap.TryGetFolderFromPath(args, true, out target))
                {
                    return string.Empty;
                }
            }

            var resultItems = new List<InvWornInfoContainer>
            {
                new InvWornInfoContainer("", GetInvWornInfo(target))
            };

            var foldersInInv = target.Children
                .Where(n => !n.Name.StartsWith("."));

            foreach (var folder in foldersInInv)
            {
                var weirdItemCountThing = GetInvWornInfo(folder);
                resultItems.Add(new InvWornInfoContainer(folder.Name, weirdItemCountThing));
            }

            var result = string.Join(",", resultItems);
            return result;
        }

        private void SearchFoldersForName(InventoryTree root, bool stopOnFirstResult, List<string> searchTerms, List<InventoryTree> outFoundFolders)
        {
            if (searchTerms.All(n => root.Name.Contains(n)))
            {
                outFoundFolders.Add(root);

                if (stopOnFirstResult)
                {
                    return;
                }
            }

            foreach (var child in root.Children)
            {
                if (child.Name.StartsWith(".") || child.Name.StartsWith("~"))
                {
                    continue;
                }

                SearchFoldersForName(child, stopOnFirstResult, searchTerms, outFoundFolders);
                if (stopOnFirstResult && outFoundFolders.Count > 0)
                {
                    return;
                }
            }
        }

        private string HandleFindFolders(bool stopOnFirstResult, List<string> searchTerms, string separator = ",")
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return string.Empty;
            }

            var inventoryMap = new InventoryMap(sharedFolder);

            var foundFolders = new List<InventoryTree>();
            SearchFoldersForName(sharedFolder, stopOnFirstResult, searchTerms, foundFolders);

            // TODO: Just add full path to the InventoryTree so we don't have to calculate it every time?
            var foundFolderPaths = foundFolders
                .Select(n => inventoryMap.BuildPathToFolder(n.Id))
                .ToList();

            var result = string.Join(separator, foundFolderPaths);

            return result;
        }

        private string HandleGetInv(string args)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return string.Empty;
            }

            var inventoryMap = new InventoryMap(sharedFolder);
            var folders = new List<InventoryTree>();

            var target = sharedFolder;
            if (args.Length != 0)
            {
                if (!inventoryMap.TryGetFolderFromPath(args, true, out target))
                {
                    return string.Empty;
                }
            }

            var foldersNamesInInv = target.Children
                .Where(n => !n.Name.StartsWith("."))
                .Select(n => n.Name);

            var result = string.Join(",", foldersNamesInInv);
            return result;
        }

        private string HandleGetPath(bool limitToOneResult, Guid? itemId, AttachmentPoint? attachmentPoint, WearableType? wearableType)
        {
            if (!_callbacks.TryGetRlvInventoryTree(out var sharedFolder).Result)
            {
                return string.Empty;
            }

            var inventoryMap = new InventoryMap(sharedFolder);
            var folders = inventoryMap.FindFoldersContaining(limitToOneResult, itemId, attachmentPoint, wearableType);

            var sb = new StringBuilder();
            foreach (var folder in folders.OrderBy(n => n.Name))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                var path = inventoryMap.BuildPathToFolder(folder.Id);
                if (path != null)
                {
                    sb.Append(path);
                }
            }

            return sb.ToString();
        }

        internal bool ProcessInstantMessageCommand(string message, Guid senderId, string senderName)
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
