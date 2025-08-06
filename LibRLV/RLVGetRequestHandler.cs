using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibRLV
{
    internal class RLVGetRequestHandler
    {
        private readonly ImmutableDictionary<string, RLVDataRequest> _rlvDataRequestToNameMap;
        private readonly IRestrictionProvider _restrictions;
        private readonly IBlacklistProvider _blacklist;
        private readonly IRLVCallbacks _callbacks;

        internal RLVGetRequestHandler(IBlacklistProvider blacklist, IRestrictionProvider restrictions, IRLVCallbacks callbacks)
        {
            _restrictions = restrictions;
            _blacklist = blacklist;
            _callbacks = callbacks;

            _rlvDataRequestToNameMap = new Dictionary<string, RLVDataRequest>()
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
            }.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        }

        private string HandleGetStatus(string option, Guid? sender)
        {
            var parts = option.Split(';');
            var filter = string.Empty;
            var separator = "/";

            if (parts.Length > 0)
            {
                filter = parts[0].ToLowerInvariant();
            }
            if (parts.Length > 1)
            {
                separator = parts[1];
            }

            var restrictions = _restrictions.FindRestrictions(filter, sender);
            StringBuilder sb = new StringBuilder();
            foreach (var restriction in restrictions)
            {
                if (!RLVRestrictionManager.TryGetRestrictionNameFromType(restriction.OriginalBehavior, out var behaviorName))
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

        private async Task<string> ProcessGetOutfit(WearableType? specificType)
        {
            var (hasCurrentOutfit, currentOutfit) = await _callbacks.TryGetCurrentOutfitAsync();
            if (!hasCurrentOutfit || currentOutfit == null)
            {
                return string.Empty;
            }

            if (specificType != null)
            {
                if (currentOutfit.Where(n => n.WornOn == specificType).Any())
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }

            var wornTypes = currentOutfit
                .Where(n => n.WornOn.HasValue)
                .Select(n => n.WornOn!.Value)
                .Distinct()
                .ToImmutableHashSet();

            var sb = new StringBuilder(16);

            // gloves,jacket,pants,shirt,shoes,skirt,socks,underpants,undershirt,skin,eyes,hair,shape,alpha,tattoo,physics
            sb.Append(wornTypes.Contains(WearableType.Gloves) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Jacket) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Pants) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Shirt) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Shoes) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Skirt) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Socks) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Underpants) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Undershirt) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Skin) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Eyes) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Hair) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Shape) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Alpha) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Tattoo) ? "1" : "0");
            sb.Append(wornTypes.Contains(WearableType.Physics) ? "1" : "0");

            return sb.ToString();
        }

        private async Task<string> ProcessGetAttach(AttachmentPoint? specificType)
        {
            var (hasCurrentOutfit, currentOutfit) = await _callbacks.TryGetCurrentOutfitAsync();
            if (!hasCurrentOutfit || currentOutfit == null)
            {
                return string.Empty;
            }

            if (specificType != null)
            {
                if (currentOutfit.Where(n => n.AttachedTo == specificType).Any())
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }

            var wornTypes = currentOutfit
                .Where(n => n.AttachedTo.HasValue)
                .Select(n => n.AttachedTo!.Value)
                .Distinct()
                .ToImmutableHashSet();

            var attachmentPointTypes = Enum.GetValues(typeof(AttachmentPoint));
            var sb = new StringBuilder(attachmentPointTypes.Length);

            // digit corresponds directly to the value of enum, unlike ProcessGetOutfit
            foreach (AttachmentPoint attachmentPoint in attachmentPointTypes)
            {
                sb.Append(wornTypes.Contains(attachmentPoint) ? '1' : '0');
            }

            return sb.ToString();
        }

        internal async Task<bool> ProcessGetCommand(RLVMessage rlvMessage, int channel)
        {
            var blacklist = _blacklist.GetBlacklist();

            string? response = null;
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

            if (_rlvDataRequestToNameMap.TryGetValue(rlvMessage.Behavior, out var name))
            {
                switch (name)
                {
                    case RLVDataRequest.GetSitId:
                    {
                        var (hasSitId, sitId) = await _callbacks.TryGetSitIdAsync();
                        if (!hasSitId || sitId == Guid.Empty)
                        {
                            response = "NULL_KEY";
                        }
                        else
                        {
                            response = sitId.ToString();
                        }

                        break;
                    }
                    case RLVDataRequest.GetCamAvDistMin:
                    case RLVDataRequest.GetCamAvDistMax:
                    case RLVDataRequest.GetCamFovMin:
                    case RLVDataRequest.GetCamFovMax:
                    case RLVDataRequest.GetCamZoomMin:
                    case RLVDataRequest.GetCamFov:
                    {
                        var (hasCameraSettings, cameraSettings) = await _callbacks.TryGetCameraSettingsAsync();
                        if (!hasCameraSettings || cameraSettings == null)
                        {
                            return false;
                        }

                        switch (name)
                        {
                            case RLVDataRequest.GetCamAvDistMin:
                            {
                                response = cameraSettings.Value.AvDistMin.ToString(CultureInfo.InvariantCulture);
                                break;
                            }
                            case RLVDataRequest.GetCamAvDistMax:
                            {
                                response = cameraSettings.Value.AvDistMax.ToString(CultureInfo.InvariantCulture);
                                break;
                            }
                            case RLVDataRequest.GetCamFovMin:
                            {
                                response = cameraSettings.Value.FovMin.ToString(CultureInfo.InvariantCulture);
                                break;
                            }
                            case RLVDataRequest.GetCamFovMax:
                            {
                                response = cameraSettings.Value.FovMax.ToString(CultureInfo.InvariantCulture);
                                break;
                            }
                            case RLVDataRequest.GetCamZoomMin:
                            {
                                response = cameraSettings.Value.ZoomMin.ToString(CultureInfo.InvariantCulture);
                                break;
                            }
                            case RLVDataRequest.GetCamFov:
                            {
                                response = cameraSettings.Value.CurrentFov.ToString(CultureInfo.InvariantCulture);
                                break;
                            }
                        }

                        break;
                    }

                    case RLVDataRequest.GetGroup:
                    {
                        var (hasGroup, group) = await _callbacks.TryGetActiveGroupNameAsync();

                        if (!hasGroup)
                        {
                            response = "none";
                        }
                        else
                        {
                            response = group;
                        }
                        break;
                    }
                    case RLVDataRequest.GetOutfit:
                    {
                        WearableType? wearableType = null;
                        if (RLVCommon.RLVWearableTypeMap.TryGetValue(rlvMessage.Option, out var wearableTypeTemp))
                        {
                            wearableType = wearableTypeTemp;
                        }

                        response = await ProcessGetOutfit(wearableType);
                        break;
                    }
                    case RLVDataRequest.GetAttach:
                    {
                        AttachmentPoint? attachmentPointType = null;
                        if (RLVCommon.RLVAttachmentPointMap.TryGetValue(rlvMessage.Option, out var attachmentPointTemp))
                        {
                            attachmentPointType = attachmentPointTemp;
                        }

                        response = await ProcessGetAttach(attachmentPointType);
                        break;
                    }
                    case RLVDataRequest.GetInv:
                        response = await HandleGetInv(rlvMessage.Option);
                        break;
                    case RLVDataRequest.GetInvWorn:
                        response = await HandleGetInvWorn(rlvMessage.Option);
                        break;
                    case RLVDataRequest.FindFolder:
                    case RLVDataRequest.FindFolders:
                    {
                        var findFolderParts = rlvMessage.Option.Split(';');
                        var separator = ",";
                        var searchTerms = findFolderParts[0]
                            .Split(["&&"], StringSplitOptions.RemoveEmptyEntries);

                        if (findFolderParts.Length > 1)
                        {
                            separator = findFolderParts[1];
                        }

                        response = await HandleFindFolders(name == RLVDataRequest.FindFolder, searchTerms, separator);
                        break;
                    }
                    case RLVDataRequest.GetPath:
                    case RLVDataRequest.GetPathNew:
                    {
                        // [] | [uuid | layer | attachpt ]
                        var parsedOptions = rlvMessage.Option.Split([';'], StringSplitOptions.RemoveEmptyEntries).ToList();

                        if (parsedOptions.Count > 1)
                        {
                            return false;
                        }

                        if (parsedOptions.Count == 0)
                        {
                            response = await HandleGetPath(name == RLVDataRequest.GetPath, rlvMessage.Sender, null, null);
                        }
                        else if (Guid.TryParse(parsedOptions[0], out var uuid))
                        {
                            response = await HandleGetPath(name == RLVDataRequest.GetPath, uuid, null, null);
                        }
                        else if (RLVCommon.RLVWearableTypeMap.TryGetValue(parsedOptions[0], out var wearableType))
                        {
                            response = await HandleGetPath(name == RLVDataRequest.GetPath, null, null, wearableType);
                        }
                        else if (RLVCommon.RLVAttachmentPointMap.TryGetValue(parsedOptions[0], out var attachmentPoint))
                        {
                            response = await HandleGetPath(name == RLVDataRequest.GetPath, null, attachmentPoint, null);
                        }
                        else
                        {
                            return false;
                        }

                        break;
                    }
                }
            }
            else if (rlvMessage.Behavior.StartsWith("getdebug_", StringComparison.OrdinalIgnoreCase))
            {
                var commandRaw = rlvMessage.Behavior.Substring("getdebug_".Length);
                var (success, debugInfo) = await _callbacks.TryGetDebugSettingValueAsync(commandRaw);

                if (success)
                {
                    response = debugInfo;
                }
            }
            else if (rlvMessage.Behavior.StartsWith("getenv_", StringComparison.OrdinalIgnoreCase))
            {
                var commandRaw = rlvMessage.Behavior.Substring("getenv_".Length);
                var (success, envInfo) = await _callbacks.TryGetEnvironmentSettingValueAsync(commandRaw);

                if (success)
                {
                    response = envInfo;
                }
            }

            if (response != null)
            {
                await _callbacks.SendReplyAsync(channel, response, CancellationToken.None);
                return true;
            }

            return false;
        }

        private sealed class InvWornInfoContainer
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
        private static void GetInvWornInfo_Internal(InventoryTree folder, bool recursive, ref int totalItems, ref int totalItemsWorn)
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

        private static string GetInvWornInfo(InventoryTree folder)
        {
            // 0 : No item is present in that folder
            // 1 : Some items are present in that folder, but none of them is worn
            // 2 : Some items are present in that folder, and some of them are worn
            // 3 : Some items are present in that folder, and all of them are worn

            var totalItemsWorn = 0;
            var totalItems = 0;
            GetInvWornInfo_Internal(folder, false, ref totalItems, ref totalItemsWorn);

            var result = string.Empty;
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

        private async Task<string> HandleGetInvWorn(string args)
        {
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync();
            if (!hasSharedFolder || sharedFolder == null)
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
                new("", GetInvWornInfo(target))
            };

            var foldersInInv = target.Children
                .Where(n => !n.Name.StartsWith(".", StringComparison.OrdinalIgnoreCase));

            foreach (var folder in foldersInInv)
            {
                var invWornInfo = GetInvWornInfo(folder);
                resultItems.Add(new InvWornInfoContainer(folder.Name, invWornInfo));
            }

            var result = string.Join(",", resultItems);
            return result;
        }

        private static void SearchFoldersForName(InventoryTree root, bool stopOnFirstResult, IEnumerable<string> searchTerms, List<InventoryTree> outFoundFolders)
        {
            if (searchTerms.All(root.Name.Contains))
            {
                outFoundFolders.Add(root);

                if (stopOnFirstResult)
                {
                    return;
                }
            }

            foreach (var child in root.Children)
            {
                if (child.Name.StartsWith(".", StringComparison.OrdinalIgnoreCase) ||
                    child.Name.StartsWith("~", StringComparison.OrdinalIgnoreCase))
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

        private async Task<string> HandleFindFolders(bool stopOnFirstResult, IEnumerable<string> searchTerms, string separator = ",")
        {
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync();
            if (!hasSharedFolder || sharedFolder == null)
            {
                return string.Empty;
            }

            var inventoryMap = new InventoryMap(sharedFolder);

            var foundFolders = new List<InventoryTree>();
            SearchFoldersForName(sharedFolder, stopOnFirstResult, searchTerms, foundFolders);

            // TODO: Just add full path to the InventoryTree so we don't have to calculate it every time?
            var foundFolderPaths = new List<string>();
            foreach (var folder in foundFolders)
            {
                if (inventoryMap.TryBuildPathToFolder(folder.Id, out var foundPath))
                {
                    foundFolderPaths.Add(foundPath);
                }
            }

            var result = string.Join(separator, foundFolderPaths);

            return result;
        }

        private async Task<string> HandleGetInv(string args)
        {
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync();
            if (!hasSharedFolder || sharedFolder == null)
            {
                return string.Empty;
            }

            var inventoryMap = new InventoryMap(sharedFolder);

            var target = sharedFolder;
            if (args.Length != 0)
            {
                if (!inventoryMap.TryGetFolderFromPath(args, true, out target))
                {
                    return string.Empty;
                }
            }

            var foldersNamesInInv = target.Children
                .Where(n => !n.Name.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                .Select(n => n.Name);

            var result = string.Join(",", foldersNamesInInv);
            return result;
        }

        private async Task<string> HandleGetPath(bool limitToOneResult, Guid? itemId, AttachmentPoint? attachmentPoint, WearableType? wearableType)
        {
            var (hasSharedFolder, sharedFolder) = await _callbacks.TryGetSharedFolderAsync();
            if (!hasSharedFolder || sharedFolder == null)
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
                    sb.Append(',');
                }

                if (inventoryMap.TryBuildPathToFolder(folder.Id, out var foundPath))
                {
                    sb.Append(foundPath);
                }
            }

            return sb.ToString();
        }

        internal async Task<bool> ProcessInstantMessageCommand(string message, Guid senderId)
        {
            switch (message)
            {
                case "@version":
                    await _callbacks.SendInstantMessageAsync(senderId, RLV.RLVVersion, CancellationToken.None);
                    return true;
                case "@getblacklist":
                    var blacklist = _blacklist.GetBlacklist();
                    await _callbacks.SendInstantMessageAsync(senderId, string.Join(",", blacklist), CancellationToken.None);
                    return true;
            }

            return false;
        }
    }
}
