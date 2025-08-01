using System.Text.RegularExpressions;
using System.Threading;
using OpenMetaverse;

namespace LibRLV
{
    public class RLV
    {
        public const string RLVVersion = "RestrainedLove viewer v3.4.3 (RLVa 2.4.2)";
        public const string RLVVersionNum = "2040213";

        public bool Enabled { get; set; }
        public bool EnableInstantMessageProcessing { get; set; }

        public RLVActionHandler Actions { get; }
        public RLVRestrictionHandler RestrictionsHandler { get; }
        public RLVManager Restrictions { get; }
        public RLVBlacklist Blacklist { get; }

        internal IRLVCallbacks Callbacks { get; }
        internal RLVGetRequestHandler GetRequestHandler { get; }
        private readonly Regex RLVRegexPattern = new Regex(@"(?<behavior>[^:=]+)(:(?<option>[^=]*))?=(?<param>.+)", RegexOptions.Compiled);

        public RLV(IRLVCallbacks callbacks, bool enabled)
        {
            Callbacks = callbacks;
            Blacklist = new RLVBlacklist();
            RestrictionsHandler = new RLVRestrictionHandler(Callbacks);
            GetRequestHandler = new RLVGetRequestHandler(Blacklist, RestrictionsHandler, Callbacks);
            Restrictions = new RLVManager(RestrictionsHandler);
            Actions = new RLVActionHandler(Restrictions, Callbacks);
            Enabled = enabled;
        }

        private bool ProcessRLVMessage(RLVMessage rlvMessage)
        {
            if (Blacklist.IsBlacklisted(rlvMessage.Behavior))
            {
                if (int.TryParse(rlvMessage.Param, out var channel))
                {
                    Callbacks.SendReplyAsync(channel, "", CancellationToken.None);
                }

                return false;
            }

            if (rlvMessage.Behavior == "clear")
            {
                return RestrictionsHandler.ProcessClearCommand(rlvMessage);
            }
            else if (rlvMessage.Param == "force")
            {
                return Actions.ProcessActionCommand(rlvMessage);
            }
            else if (rlvMessage.Param == "y" || rlvMessage.Param == "n" || rlvMessage.Param == "add" || rlvMessage.Param == "rem")
            {
                return RestrictionsHandler.ProcessRestrictionCommand(rlvMessage, rlvMessage.Option, rlvMessage.Param == "n" || rlvMessage.Param == "add");
            }
            else if (int.TryParse(rlvMessage.Param, out var channel))
            {
                if (channel == 0)
                {
                    return false;
                }

                return GetRequestHandler.ProcessGetCommand(rlvMessage, channel);
            }

            return false;
        }

        private bool ProcessSingleMessage(string message, UUID senderId, string senderName)
        {
            // Special hack for @clear, which doesn't match the standard pattern of @behavior=param
            if (message == "clear")
            {
                return ProcessRLVMessage(new RLVMessage()
                {
                    Behavior = message,
                    Option = "",
                    Param = "",
                    Sender = senderId,
                    SenderName = senderName
                });
            }

            var match = RLVRegexPattern.Match(message);
            if (!match.Success)
            {
                return false;
            }

            var rlvMessage = new RLVMessage
            {
                Behavior = match.Groups["behavior"].ToString().ToLower(),
                Option = match.Groups["option"].ToString(),
                Param = match.Groups["param"].ToString().ToLower(),
                Sender = senderId,
                SenderName = senderName
            };

            return ProcessRLVMessage(rlvMessage);
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

        public bool ProcessInstantMessage(string message, UUID senderId, string senderName)
        {
            if (!EnableInstantMessageProcessing || !Enabled || !message.StartsWith("@"))
            {
                return false;
            }

            if (Blacklist.IsBlacklisted(message))
            {
                return false;
            }

            return GetRequestHandler.ProcessInstantMessageCommand(message.ToLower(), senderId, senderName);
        }

        public void ReportSendPublicMessage(string message)
        {
            if (message.StartsWith("/me"))
            {
                if (!Restrictions.IsRedirEmote(out var channels))
                {
                    return;
                }

                foreach (var channel in channels)
                {
                    Callbacks.SendReplyAsync(channel, message, System.Threading.CancellationToken.None);
                }
            }
            else
            {
                if (!Restrictions.IsRedirChat(out var channels))
                {
                    return;
                }

                foreach (var channel in channels)
                {
                    Callbacks.SendReplyAsync(channel, message, System.Threading.CancellationToken.None);
                }
            }
        }

        public enum InventoryOfferAction
        {
            Accepted = 1,
            Denied = 2
        }
        public void ReportInventoryOffer(string itemOrFolderPath, InventoryOfferAction action)
        {
            var isSharedFolder = false;

            if (itemOrFolderPath.StartsWith("#RLV/"))
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

            SendNotification(notificationText);
        }

        public enum WornItemChange
        {
            Attached = 1,
            Detached = 2
        }
        public void ReportWornItemChange(UUID objectFolderId, bool isShared, WearableType wearableType, WornItemChange changeType)
        {
            var notificationText = "";

            if (changeType == WornItemChange.Attached)
            {
                var isLegal = Restrictions.CanAttach(objectFolderId, isShared, null, wearableType);

                if (isLegal)
                {
                    notificationText = $"/worn legally {wearableType.ToString().ToLower()}";
                }
                else
                {
                    notificationText = $"/worn illegally {wearableType.ToString().ToLower()}";
                }
            }
            else if (changeType == WornItemChange.Detached)
            {
                var isLegal = Restrictions.CanDetach(objectFolderId, isShared, null, wearableType);

                if (isLegal)
                {
                    notificationText = $"/unworn legally {wearableType.ToString().ToLower()}";
                }
                else
                {
                    notificationText = $"/unworn illegally {wearableType.ToString().ToLower()}";
                }
            }
            else
            {
                return;
            }

            SendNotification(notificationText);
        }

        public enum AttachedItemChange
        {
            Attached = 1,
            Detached = 2
        }
        public void ReportAttachedItemChange(UUID objectFolderId, bool isShared, AttachmentPoint attachmentPoint, AttachedItemChange changeType)
        {
            var notificationText = "";

            if (changeType == AttachedItemChange.Attached)
            {
                var isLegal = Restrictions.CanAttach(objectFolderId, isShared, attachmentPoint, null);

                if (isLegal)
                {
                    notificationText = $"/attached legally {attachmentPoint.ToString().ToLower()}";
                }
                else
                {
                    notificationText = $"/attached illegally {attachmentPoint.ToString().ToLower()}";
                }
            }
            else if (changeType == AttachedItemChange.Detached)
            {
                var isLegal = Restrictions.CanDetach(objectFolderId, isShared, attachmentPoint, null);

                if (isLegal)
                {
                    notificationText = $"/detached legally {attachmentPoint.ToString().ToLower()}";
                }
                else
                {
                    notificationText = $"/detached illegally {attachmentPoint.ToString().ToLower()}";
                }
            }
            else
            {
                return;
            }

            SendNotification(notificationText);
        }

        public enum SitType
        {
            Sit = 1,
            Stand,
        }
        public void ReportSit(SitType sitType, UUID? objectId, float? objectDistance)
        {
            var notificationText = "";

            if (sitType == SitType.Sit && objectId != null)
            {
                var isLegal = Restrictions.CanInteract() && Restrictions.CanSit();

                if (Restrictions.CanSitTp(out var maxObjectDistance))
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
                var isLegal = Restrictions.CanInteract() && Restrictions.CanUnsit();

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
                var isLegal = Restrictions.CanSit();

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
                var isLegal = Restrictions.CanUnsit();

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

            SendNotification(notificationText);
        }

        private void SendNotification(string notificationText)
        {
            var notificationRestrictions = RestrictionsHandler.GetRestrictions(RLVRestrictionType.Notify);

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
                    Callbacks.SendReplyAsync(channel, notificationText, System.Threading.CancellationToken.None);
                }
            }
        }
    }
}
