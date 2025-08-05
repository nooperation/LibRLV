using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LibRLV
{
    public class RLV
    {
        public const string RLVVersion = "RestrainedLove viewer v3.4.3 (RLVa 2.4.2)";
        public const string RLVVersionNum = "2040213";

        private volatile bool _enabled;
        private volatile bool _enableInstantMessageProcessing;

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public bool EnableInstantMessageProcessing
        {
            get => _enableInstantMessageProcessing;
            set => _enableInstantMessageProcessing = value;
        }

        public RLVCommandProcessor Commands { get; }
        public RLVRestrictionManager Restrictions { get; }
        public RLVPermissionsService Permissions { get; }
        public RLVBlacklist Blacklist { get; }

        internal IRLVCallbacks Callbacks { get; }
        internal RLVGetRequestHandler GetRequestHandler { get; }

        private readonly Regex _rlvRegexPattern = new Regex(@"(?<behavior>[^:=]+)(:(?<option>[^=]*))?=(?<param>.+)", RegexOptions.Compiled);

        public RLV(IRLVCallbacks callbacks, bool enabled)
        {
            Callbacks = callbacks;
            Blacklist = new RLVBlacklist();
            Restrictions = new RLVRestrictionManager(Callbacks);
            GetRequestHandler = new RLVGetRequestHandler(Blacklist, Restrictions, Callbacks);
            Permissions = new RLVPermissionsService(Restrictions);
            Commands = new RLVCommandProcessor(Permissions, Callbacks);
            Enabled = enabled;
        }

        private async Task<bool> ProcessRLVMessage(RLVMessage rlvMessage)
        {
            if (Blacklist.IsBlacklisted(rlvMessage.Behavior))
            {
                if (int.TryParse(rlvMessage.Param, out var channel))
                {
                    await Callbacks.SendReplyAsync(channel, "", CancellationToken.None);
                }

                return false;
            }

            if (rlvMessage.Behavior == "clear")
            {
                return await Restrictions.ProcessClearCommand(rlvMessage);
            }
            else if (rlvMessage.Param == "force")
            {
                return await Commands.ProcessActionCommand(rlvMessage);
            }
            else if (rlvMessage.Param == "y" || rlvMessage.Param == "n" || rlvMessage.Param == "add" || rlvMessage.Param == "rem")
            {
                return await Restrictions.ProcessRestrictionCommand(rlvMessage, rlvMessage.Option, rlvMessage.Param == "n" || rlvMessage.Param == "add");
            }
            else if (int.TryParse(rlvMessage.Param, out var channel))
            {
                if (channel == 0)
                {
                    return false;
                }

                return await GetRequestHandler.ProcessGetCommand(rlvMessage, channel);
            }

            return false;
        }

        private async Task<bool> ProcessSingleMessage(string message, Guid senderId, string senderName)
        {
            // Special hack for @clear, which doesn't match the standard pattern of @behavior=param
            if (message.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                return await ProcessRLVMessage(new RLVMessage()
                {
                    Behavior = "clear",
                    Option = "",
                    Param = "",
                    Sender = senderId,
                    SenderName = senderName
                });
            }

            var match = _rlvRegexPattern.Match(message);
            if (!match.Success)
            {
                return false;
            }

            var rlvMessage = new RLVMessage
            {
                Behavior = match.Groups["behavior"].Value.ToLowerInvariant(),
                Option = match.Groups["option"].Value,
                Param = match.Groups["param"].Value.ToLowerInvariant(),
                Sender = senderId,
                SenderName = senderName
            };

            return await ProcessRLVMessage(rlvMessage);
        }

        public async Task<bool> ProcessMessage(string message, Guid senderId, string senderName)
        {
            if (!Enabled || !message.StartsWith("@", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var result = true;
            foreach (var singleMessage in message.Substring(1).Split(','))
            {
                var isSuccessful = await ProcessSingleMessage(singleMessage, senderId, senderName);
                if (!isSuccessful)
                {
                    result = false;
                }
            }

            return result;
        }

        public async Task<bool> ProcessInstantMessage(string message, Guid senderId)
        {
            if (!EnableInstantMessageProcessing || !Enabled || !message.StartsWith("@", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (Blacklist.IsBlacklisted(message))
            {
                return false;
            }

            return await GetRequestHandler.ProcessInstantMessageCommand(message.ToLowerInvariant(), senderId);
        }

        public async Task ReportSendPublicMessage(string message)
        {
            IReadOnlyList<int> channels;

            if (message.StartsWith("/me ", StringComparison.OrdinalIgnoreCase))
            {
                if (!Permissions.IsRedirEmote(out channels))
                {
                    return;
                }
            }
            else
            {
                if (!Permissions.IsRedirChat(out channels))
                {
                    return;
                }
            }

            var tasks = channels
                .Select(channel => Callbacks.SendReplyAsync(channel, message, System.Threading.CancellationToken.None));

            await Task.WhenAll(tasks);
        }

        public enum InventoryOfferAction
        {
            Accepted = 1,
            Denied = 2
        }
        public async Task ReportInventoryOffer(string itemOrFolderPath, InventoryOfferAction action)
        {
            var isSharedFolder = false;

            if (itemOrFolderPath.StartsWith("#RLV/", StringComparison.Ordinal))
            {
                itemOrFolderPath = itemOrFolderPath.Substring("#RLV/".Length);
                isSharedFolder = true;
            }

            var notificationText = "";
            if (action == InventoryOfferAction.Accepted)
            {
                if (isSharedFolder)
                {
                    notificationText = $"/accepted_in_rlv inv_offer {itemOrFolderPath}";
                }
                else
                {
                    notificationText = $"/accepted_in_inv inv_offer {itemOrFolderPath}";
                }
            }
            else
            {
                notificationText = $"/declined inv_offer {itemOrFolderPath}";
            }

            await SendNotification(notificationText);
        }

        public enum WornItemChange
        {
            Attached = 1,
            Detached = 2
        }
        public async Task ReportWornItemChange(Guid objectFolderId, bool isShared, WearableType wearableType, WornItemChange changeType)
        {
            var notificationText = "";

            if (changeType == WornItemChange.Attached)
            {
                var isLegal = Permissions.CanAttach(objectFolderId, isShared, null, wearableType);

                if (isLegal)
                {
                    notificationText = $"/worn legally {wearableType.ToString().ToLowerInvariant()}";
                }
                else
                {
                    notificationText = $"/worn illegally {wearableType.ToString().ToLowerInvariant()}";
                }
            }
            else if (changeType == WornItemChange.Detached)
            {
                var isLegal = Permissions.CanDetach(objectFolderId, isShared, null, wearableType);

                if (isLegal)
                {
                    notificationText = $"/unworn legally {wearableType.ToString().ToLowerInvariant()}";
                }
                else
                {
                    notificationText = $"/unworn illegally {wearableType.ToString().ToLowerInvariant()}";
                }
            }
            else
            {
                return;
            }

            await SendNotification(notificationText);
        }

        public enum AttachedItemChange
        {
            Attached = 1,
            Detached = 2
        }
        public async Task ReportAttachedItemChange(Guid objectFolderId, bool isShared, AttachmentPoint attachmentPoint, AttachedItemChange changeType)
        {
            var notificationText = "";

            if (changeType == AttachedItemChange.Attached)
            {
                var isLegal = Permissions.CanAttach(objectFolderId, isShared, attachmentPoint, null);

                if (isLegal)
                {
                    notificationText = $"/attached legally {attachmentPoint.ToString().ToLowerInvariant()}";
                }
                else
                {
                    notificationText = $"/attached illegally {attachmentPoint.ToString().ToLowerInvariant()}";
                }
            }
            else if (changeType == AttachedItemChange.Detached)
            {
                var isLegal = Permissions.CanDetach(objectFolderId, isShared, attachmentPoint, null);

                if (isLegal)
                {
                    notificationText = $"/detached legally {attachmentPoint.ToString().ToLowerInvariant()}";
                }
                else
                {
                    notificationText = $"/detached illegally {attachmentPoint.ToString().ToLowerInvariant()}";
                }
            }
            else
            {
                return;
            }

            await SendNotification(notificationText);
        }

        public enum SitType
        {
            Sit = 1,
            Stand,
        }
        public async Task ReportSit(SitType sitType, Guid? objectId, float? objectDistance)
        {
            var notificationText = "";

            if (sitType == SitType.Sit && objectId != null)
            {
                var isLegal = Permissions.CanInteract() && Permissions.CanSit();

                if (Permissions.CanSitTp(out var maxObjectDistance))
                {
                    if (objectDistance == null || objectDistance > maxObjectDistance)
                    {
                        isLegal = false;
                    }
                }

                if (isLegal)
                {
                    notificationText = $"/sat object legally {objectId}";
                }
                else
                {
                    notificationText = $"/sat object illegally {objectId}";
                }
            }
            else if (sitType == SitType.Stand && objectId != null)
            {
                var isLegal = Permissions.CanInteract() && Permissions.CanUnsit();

                if (isLegal)
                {
                    notificationText = $"/unsat object legally {objectId}";
                }
                else
                {
                    notificationText = $"/unsat object illegally {objectId}";
                }
            }
            else if (sitType == SitType.Sit && objectId == null)
            {
                var isLegal = Permissions.CanSit();

                if (isLegal)
                {
                    notificationText = $"/sat ground legally";
                }
                else
                {
                    notificationText = $"/sat ground illegally";
                }
            }
            else if (sitType == SitType.Stand && objectId == null)
            {
                var isLegal = Permissions.CanUnsit();

                if (isLegal)
                {
                    notificationText = $"/unsat ground legally";
                }
                else
                {
                    notificationText = $"/unsat ground illegally";
                }
            }
            else
            {
                return;
            }

            await SendNotification(notificationText);
        }

        private async Task SendNotification(string notificationText)
        {
            var notificationRestrictions = Restrictions.GetRestrictionsByType(RLVRestrictionType.Notify);
            var tasks = new List<Task>(notificationRestrictions.Count);

            foreach (var notificationRestriction in notificationRestrictions)
            {
                if (!(notificationRestriction.Args[0] is int channel))
                {
                    continue;
                }

                if (!(notificationRestriction.Args.Count > 1 && notificationRestriction.Args[1] is string filter))
                {
                    filter = "";
                }

                if (notificationText.Contains(filter))
                {
                    tasks.Add(Callbacks.SendReplyAsync(channel, notificationText, System.Threading.CancellationToken.None));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
