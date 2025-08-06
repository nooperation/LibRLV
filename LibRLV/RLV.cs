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

        internal IRLVCallbacks Callbacks { get; }
        internal RLVGetRequestHandler GetRequestHandler { get; }

        private readonly Regex _rlvRegexPattern = new(@"(?<behavior>[^:=]+)(:(?<option>[^=]*))?=(?<param>.+)", RegexOptions.Compiled);

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

        #region public
        /// <summary>
        /// Process an RLV command
        /// </summary>
        /// <param name="message">Message containing the command or commands</param>
        /// <param name="senderId">ID of the object sending the command</param>
        /// <param name="senderName">Name of the object sending the command</param>
        /// <returns>True if all of the command were processed successfully</returns>
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

        /// <summary>
        /// Process an instant message containing an RLV command
        /// </summary>
        /// <param name="message">Instant message command</param>
        /// <param name="senderId">ID of the user sending the instant message</param>
        /// <returns>True if the command was successfully processed</returns>
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

        /// <summary>
        /// Report the sending of a public message by the current user
        /// </summary>
        /// <param name="message">Message being sent to public chat (channel 0)</param>
        /// <returns>Task</returns>
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


        /// <summary>
        /// Report that the user has just accepted an inventory offer
        /// </summary>
        /// <param name="folderPath">Path to the accepted folder</param>
        /// <returns>Task</returns>
        public async Task ReportInventoryOfferAccepted(string folderPath)
        {
            var isSharedFolder = false;

            if (folderPath.StartsWith("#RLV/", StringComparison.Ordinal))
            {
                folderPath = folderPath.Substring("#RLV/".Length);
                isSharedFolder = true;
            }

            var notificationText = "";
            if (isSharedFolder)
            {
                notificationText = $"/accepted_in_rlv inv_offer {folderPath}";
            }
            else
            {
                notificationText = $"/accepted_in_inv inv_offer {folderPath}";
            }

            await SendNotification(notificationText);
        }

        /// <summary>
        /// Report that the user has just declined an inventory offer
        /// </summary>
        /// <param name="folderPath">Path to the declined folder</param>
        /// <returns>Task</returns>
        public async Task ReportInventoryOfferDeclined(string folderPath)
        {
            if (folderPath.StartsWith("#RLV/", StringComparison.Ordinal))
            {
                folderPath = folderPath.Substring("#RLV/".Length);
            }

            var notificationText = $"/declined inv_offer {folderPath}";
            await SendNotification(notificationText);
        }

        /// <summary>
        /// Report that the user has worn an item
        /// </summary>
        /// <param name="objectFolderId">Folder id containing the item being worn</param>
        /// <param name="isShared">True if this folder is a shared folder</param>
        /// <param name="wearableType">Type of wearable being worn</param>
        /// <returns>Task</returns>
        public async Task ReportItemWorn(Guid objectFolderId, bool isShared, WearableType wearableType)
        {
            var notificationText = "";
            var isLegal = Permissions.CanAttach(objectFolderId, isShared, null, wearableType);

            if (isLegal)
            {
                notificationText = $"/worn legally {wearableType.ToString().ToLowerInvariant()}";
            }
            else
            {
                notificationText = $"/worn illegally {wearableType.ToString().ToLowerInvariant()}";
            }

            await SendNotification(notificationText);
        }

        /// <summary>
        /// Report that the user has removed a worn item
        /// </summary>
        /// <param name="objectFolderId">Folder id containing the item being removed</param>
        /// <param name="isShared">True if this folder is a shared folder</param>
        /// <param name="wearableType">Type of wearable being removed</param>
        /// <returns>Task</returns>
        public async Task ReportItemUnworn(Guid objectFolderId, bool isShared, WearableType wearableType)
        {
            var notificationText = "";
            var isLegal = Permissions.CanDetach(objectFolderId, isShared, null, wearableType);

            if (isLegal)
            {
                notificationText = $"/unworn legally {wearableType.ToString().ToLowerInvariant()}";
            }
            else
            {
                notificationText = $"/unworn illegally {wearableType.ToString().ToLowerInvariant()}";
            }

            await SendNotification(notificationText);
        }

        /// <summary>
        /// Report that the user has attached an item
        /// </summary>
        /// <param name="objectFolderId">ID of the folder containing the item being attached</param>
        /// <param name="isShared">True if the folder is a shared folder</param>
        /// <param name="attachmentPoint">Attachment point where the item was attached</param>
        /// <returns>Task</returns>
        public async Task ReportItemAttached(Guid objectFolderId, bool isShared, AttachmentPoint attachmentPoint)
        {
            var notificationText = "";
            var isLegal = Permissions.CanAttach(objectFolderId, isShared, attachmentPoint, null);

            if (isLegal)
            {
                notificationText = $"/attached legally {attachmentPoint.ToString().ToLowerInvariant()}";
            }
            else
            {
                notificationText = $"/attached illegally {attachmentPoint.ToString().ToLowerInvariant()}";
            }

            await SendNotification(notificationText);
        }

        /// <summary>
        /// Report that the user has detached an item
        /// </summary>
        /// <param name="objectFolderId">ID of the folder containing the item being detached</param>
        /// <param name="isShared">True if the folder is a shared folder</param>
        /// <param name="attachmentPoint">Attachment point where the item was detached from</param>
        /// <returns>Task</returns>
        public async Task ReportItemDetached(Guid objectFolderId, bool isShared, AttachmentPoint attachmentPoint)
        {
            var notificationText = "";
            var isLegal = Permissions.CanDetach(objectFolderId, isShared, attachmentPoint, null);

            if (isLegal)
            {
                notificationText = $"/detached legally {attachmentPoint.ToString().ToLowerInvariant()}";
            }
            else
            {
                notificationText = $"/detached illegally {attachmentPoint.ToString().ToLowerInvariant()}";
            }

            await SendNotification(notificationText);
        }

        /// <summary>
        /// Report that the current user just sat on the ground or an object
        /// </summary>
        /// <param name="objectId">Null if user sat on the ground, otherwise ID of the object being sat on</param>
        /// <returns></returns>
        public async Task ReportSit(Guid? objectId)
        {
            var notificationText = "";

            if (objectId != null)
            {
                var isLegal = Permissions.CanInteract() && Permissions.CanSit();

                if (isLegal)
                {
                    notificationText = $"/sat object legally {objectId}";
                }
                else
                {
                    notificationText = $"/sat object illegally {objectId}";
                }
            }
            else
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

            await SendNotification(notificationText);
        }

        /// <summary>
        /// Report that the user stands up from sitting on the ground or an object
        /// </summary>
        /// <param name="objectId">Null if user was sitting on the ground, otherwise ID of the object user was sitting on</param>
        /// <returns>Task</returns>
        public async Task ReportUnsit(Guid? objectId)
        {
            var notificationText = "";

            if (objectId != null)
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
            else
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

            await SendNotification(notificationText);
        }
        #endregion

        #region Private
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
            else if (rlvMessage.Param is "y" or "n" or "add" or "rem")
            {
                return await Restrictions.ProcessRestrictionCommand(rlvMessage, rlvMessage.Option, rlvMessage.Param is "n" or "add");
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
                return await ProcessRLVMessage(new RLVMessage(
                    behavior: "clear",
                    option: "",
                    param: "",
                    sender: senderId,
                    senderName: senderName
                ));
            }

            var match = _rlvRegexPattern.Match(message);
            if (!match.Success)
            {
                return false;
            }

            var rlvMessage = new RLVMessage(
                behavior: match.Groups["behavior"].Value.ToLowerInvariant(),
                option: match.Groups["option"].Value,
                param: match.Groups["param"].Value.ToLowerInvariant(),
                sender: senderId,
                senderName: senderName
            );

            return await ProcessRLVMessage(rlvMessage);
        }

        private async Task SendNotification(string notificationText)
        {
            var notificationRestrictions = Restrictions.GetRestrictionsByType(RLVRestrictionType.Notify);
            var tasks = new List<Task>(notificationRestrictions.Count);

            foreach (var notificationRestriction in notificationRestrictions)
            {
                if (notificationRestriction.Args[0] is not int channel)
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
        #endregion
    }
}
