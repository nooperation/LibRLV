using System;
using System.Collections.Generic;
using System.Linq;

namespace LibRLV
{
    public class RLVPermissionsService
    {
        private readonly IRestrictionProvider _restrictionProvider;
        private static readonly char[] _invalidMessageCharacters = new char[] { '(', ')', '"', '-', '*', '=', '_', '^' };

        internal RLVPermissionsService(IRestrictionProvider restrictionProvider)
        {
            _restrictionProvider = restrictionProvider;
        }

        internal static bool TryGetRestrictionValueMax(IRestrictionProvider restrictionProvider, RLVRestrictionType restrictionType, out float val)
        {
            var restriction = restrictionProvider.GetRestrictionsByType(restrictionType);
            if (restriction.Count == 0)
            {
                val = default;
                return false;
            }

            val = restriction
                .Where(n => n.Args.Count > 0 && n.Args[0] is float)
                .Select(n => (float)n.Args[0])
                .Max();

            return true;
        }

        internal static bool TryGetRestrictionValueMin(IRestrictionProvider restrictionProvider, RLVRestrictionType restrictionType, out float val)
        {
            var restriction = restrictionProvider.GetRestrictionsByType(restrictionType);
            if (restriction.Count == 0)
            {
                val = default;
                return false;
            }

            val = restriction
                .Where(n => n.Args.Count > 0 && n.Args[0] is float)
                .Select(n => (float)n.Args[0])
                .Min();

            return true;
        }

        internal static bool TryGetOptionalRestrictionValueMin(IRestrictionProvider restrictionProvider, RLVRestrictionType restrictionType, float defaultVal, out float val)
        {
            var restrictions = restrictionProvider.GetRestrictionsByType(restrictionType);
            if (restrictions.Count == 0)
            {
                val = defaultVal;
                return false;
            }

            if (restrictions.FirstOrDefault(n => n.Args.Count == 0) != null)
            {
                val = defaultVal;
            }
            else
            {
                val = restrictions
                    .Where(n => n.Args.Count > 0 && n.Args[0] is float)
                    .Select(n => (float)n.Args[0])
                    .Min();
            }

            return true;
        }

        private bool CheckSecureRestriction(Guid? userId, string? groupName, RLVRestrictionType normalType, RLVRestrictionType? secureType, RLVRestrictionType? fromToType)
        {
            // Explicit restrictions
            if (fromToType != null)
            {
                var isRestrictedBySendImTo = _restrictionProvider.GetRestrictionsByType(fromToType.Value)
                    .Where(n => n.Args.Count == 1 &&
                        ((userId != null && n.Args[0] is Guid restrictedId && userId == restrictedId) ||
                        (groupName != null && n.Args[0] is string restrictedGroupName && (restrictedGroupName == "allgroups" || restrictedGroupName == groupName)))
                    ).Any();
                if (isRestrictedBySendImTo)
                {
                    return false;
                }
            }

            var sendImRestrictions = _restrictionProvider.GetRestrictionsByType(normalType);
            var sendImExceptions = sendImRestrictions
                .Where(n => n.IsException && n.Args.Count == 1 &&
                    ((userId != null && n.Args[0] is Guid restrictedId && userId == restrictedId) ||
                    (groupName != null && n.Args[0] is string restrictedGroupName && (restrictedGroupName == "allgroups" || restrictedGroupName == groupName)))
                ).ToList();

            // Secure restrictions
            if (secureType != null)
            {
                var sendImRestrictionsSecure = _restrictionProvider.GetRestrictionsByType(secureType.Value);
                foreach (var item in sendImRestrictionsSecure)
                {
                    var hasException = sendImExceptions
                        .Where(n => n.Sender == item.Sender)
                        .Any();
                    if (hasException)
                    {
                        continue;
                    }

                    return false;
                }
            }

            // Normal restrictions
            var permissiveMode = IsPermissive();
            foreach (var restriction in sendImRestrictions.Where(n => !n.IsException && n.Args.Count == 0))
            {
                var hasException = sendImExceptions
                    .Where(n => permissiveMode || n.Sender == restriction.Sender)
                    .Any();
                if (hasException)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public bool CanFly()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.Fly).Count == 0;
        }
        public bool CanJump()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.Jump).Count == 0;
        }
        public bool CanTempRun()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.TempRun).Count == 0;
        }
        public bool CanAlwaysRun()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.AlwaysRun).Count == 0;
        }
        public bool CanUnsit()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.Unsit).Count == 0;
        }
        public bool CanSit()
        {
            if (!CanInteract())
            {
                return false;
            }

            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.Sit).Count == 0;
        }

        #region TP

        public bool CanTpLm()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.TpLm).Count == 0;
        }
        public bool CanTpLoc()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.TpLoc).Count == 0;
        }
        public bool CanSitTp(out float sitTpDist)
        {
            return TryGetOptionalRestrictionValueMin(_restrictionProvider, RLVRestrictionType.SitTp, 1.5f, out sitTpDist);
        }
        public bool CanTpLocal(out float tpLocalDist)
        {
            return TryGetOptionalRestrictionValueMin(_restrictionProvider, RLVRestrictionType.TpLocal, 0.0f, out tpLocalDist);
        }
        public bool CanStandTp()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.StandTp).Count == 0;
        }

        public bool CanTPLure(Guid? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.TpLure, RLVRestrictionType.TpLureSec, null);
        }

        public bool CanTpRequest(Guid? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.TpRequest, RLVRestrictionType.TpRequestSec, null);
        }

        public bool IsAutoAcceptTp(Guid? userId = null)
        {
            var restrictions = _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.AcceptTp);
            foreach (var restriction in restrictions)
            {
                if (restriction.Args.Count == 0)
                {
                    return true;
                }

                if (restriction.Args[0] is Guid allowedUserID && allowedUserID == userId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAutoAcceptTpRequest(Guid? userId = null)
        {
            var restrictions = _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.AcceptTpRequest);
            foreach (var restriction in restrictions)
            {
                if (restriction.Args.Count == 0)
                {
                    return true;
                }

                if (restriction.Args[0] is Guid allowedUserID && allowedUserID == userId)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        public bool CanShowInv()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ShowInv).Count == 0;
        }
        public bool CanViewNote()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ViewNote).Count == 0;
        }
        public bool CanViewScript()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ViewScript).Count == 0;
        }
        public bool CanViewTexture()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ViewTexture).Count == 0;
        }

        public bool CanDefaultWear()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.DefaultWear).Count == 0;
        }
        public bool CanSetGroup()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SetGroup).Count == 0;
        }
        public bool CanSetDebug()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SetDebug).Count == 0;
        }
        public bool CanSetEnv()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SetEnv).Count == 0;
        }
        public bool CanAllowIdle()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.AllowIdle).Count == 0;
        }
        public bool CanInteract()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.Interact).Count == 0;
        }
        public bool CanShowWorldMap()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ShowWorldMap).Count == 0;
        }
        public bool CanShowMiniMap()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ShowMiniMap).Count == 0;
        }
        public bool CanShowLoc()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ShowLoc).Count == 0;
        }
        public bool CanShowNearby()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ShowNearby).Count == 0;
        }

        public bool IsAutoDenyPermissions()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.DenyPermission).Count != 0;
        }

        #region Camera

        public CameraRestrictions GetCameraRestrictions()
        {
            var restrictions = new CameraRestrictions(_restrictionProvider);
            return restrictions;
        }

        #endregion

        #region Chat
        public bool CanStartIM(Guid? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.StartIm, null, RLVRestrictionType.StartImTo);
        }

        public bool CanSendIM(string message, Guid? userId, string? groupName = null)
        {
            return CheckSecureRestriction(userId, groupName, RLVRestrictionType.SendIm, RLVRestrictionType.SendImSec, RLVRestrictionType.SendImTo);
        }

        public bool CanReceiveIM(string message, Guid? userId, string? groupName = null)
        {
            return CheckSecureRestriction(userId, groupName, RLVRestrictionType.RecvIm, RLVRestrictionType.RecvImSec, RLVRestrictionType.RecvImFrom);
        }

        public bool CanReceiveChat(string message, Guid? userId)
        {
            if (message.StartsWith("/me ", StringComparison.OrdinalIgnoreCase))
            {
                return CheckSecureRestriction(userId, null, RLVRestrictionType.RecvEmote, RLVRestrictionType.RecvEmoteSec, RLVRestrictionType.RecvEmoteFrom);
            }
            else
            {
                return CheckSecureRestriction(userId, null, RLVRestrictionType.RecvChat, RLVRestrictionType.RecvChatSec, RLVRestrictionType.RecvChatFrom);
            }
        }

        public bool CanChatShout()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ChatShout).Count == 0;
        }
        public bool CanChatWhisper()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ChatWhisper).Count == 0;
        }
        public bool CanChatNormal()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ChatNormal).Count == 0;
        }
        public bool CanSendChat()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SendChat).Count == 0;
        }
        public bool CanEmote()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.Emote).Count == 0;
        }
        public bool CanSendGesture()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SendGesture).Count == 0;
        }

        public bool IsRedirChat(out IReadOnlyList<int> channels)
        {
            channels = _restrictionProvider
                .GetRestrictionsByType(RLVRestrictionType.RedirChat)
                .Where(n => n.Args.Count == 1 && n.Args[0] is int)
                .Select(n => (int)n.Args[0])
                .Distinct()
                .ToList();

            return channels.Count > 0;
        }

        public bool IsRedirEmote(out IReadOnlyList<int> channels)
        {
            channels = _restrictionProvider
                .GetRestrictionsByType(RLVRestrictionType.RedirEmote)
                .Where(n => n.Args.Count == 1 && n.Args[0] is int)
                .Select(n => (int)n.Args[0])
                .Distinct()
                .ToList();

            return channels.Count > 0;
        }

        private bool CanChatOnChannelPrivateChannel(int channel)
        {
            var sendChannelExceptRestrictions = _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SendChannelExcept);

            foreach (var restriction in sendChannelExceptRestrictions)
            {
                if (restriction.Args.Count == 0)
                {
                    continue;
                }

                if (restriction.Args[0] is not int restrictedChannel)
                {
                    continue;
                }

                if (channel == restrictedChannel)
                {
                    return false;
                }
            }

            var sendChannelRestrictionsSecure = _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SendChannelSec);
            var sendChannelRestrictions = _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SendChannel);
            var channelExceptions = sendChannelRestrictions
                .Where(n =>
                    n.IsException &&
                    n.Args.Count > 0 &&
                    n.Args[0] is int exceptionChannel &&
                    exceptionChannel == channel
                )
                .ToList();

            foreach (var restriction in sendChannelRestrictionsSecure)
            {
                var hasSecureException = channelExceptions
                    .Where(n => n.Sender == restriction.Sender)
                    .Any();
                if (hasSecureException)
                {
                    continue;
                }

                return false;
            }

            var permissiveMode = IsPermissive();
            foreach (var restriction in sendChannelRestrictions.Where(n => !n.IsException && n.Args.Count == 0))
            {
                var hasException = channelExceptions
                    .Where(n => permissiveMode || n.Sender == restriction.Sender)
                    .Any();
                if (hasException)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public bool CanChat(int channel, string message)
        {
            if (channel == 0)
            {
                var canEmote = _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.Emote).Count == 0;
                if (message.StartsWith("/me ", StringComparison.OrdinalIgnoreCase) && !canEmote)
                {
                    return false;
                }

                if (!CanSendChat())
                {
                    // TODO: Implement weird hacked on restrictions from @sendchat?
                    //  emotes and messages beginning with a slash ('/') will go through,
                    //  truncated to strings of 30 and 15 characters long respectively (likely
                    //  to change later). Messages with special signs like ()"-*=_^ are prohibited,
                    //  and will be discarded. When a period ('.') is present, the rest of the
                    //  message is discarded. 

                    if (message.IndexOfAny(_invalidMessageCharacters) != -1)
                    {
                        return false;
                    }

                    if (!message.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!CanChatOnChannelPrivateChannel(channel))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        public bool CanShowNames(Guid? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.ShowNames, RLVRestrictionType.ShowNamesSec, null);
        }

        public bool CanShowNameTags(Guid? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.ShowNameTags, null, null);
        }

        public bool CanShare(Guid? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.Share, RLVRestrictionType.ShareSec, null);
        }

        public bool IsAutoAcceptPermissions()
        {
            if (IsAutoDenyPermissions())
            {
                return false;
            }

            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.AcceptPermission).Count != 0;
        }


        public bool IsPermissive()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.Permissive).Count == 0;
        }

        public bool CanRez()
        {
            if (!CanInteract())
            {
                return false;
            }

            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.Rez).Count == 0;
        }

        public enum ObjectLocation
        {
            Hud,
            Attached,
            RezzedInWorld
        }
        public bool CanEdit(ObjectLocation objectLocation, Guid? objectId)
        {
            if (!CanInteract())
            {
                return false;
            }

            var canEditObject = CheckSecureRestriction(objectId, null, RLVRestrictionType.Edit, null, RLVRestrictionType.EditObj);
            if (!canEditObject)
            {
                return false;
            }

            if (objectLocation == ObjectLocation.RezzedInWorld)
            {
                var hasEditWorldRestriction = _restrictionProvider
                    .GetRestrictionsByType(RLVRestrictionType.EditWorld)
                    .Count != 0;
                if (hasEditWorldRestriction)
                {
                    return false;
                }
            }

            if (objectLocation == ObjectLocation.Attached)
            {
                var hasEditAttachRestriction = _restrictionProvider
                    .GetRestrictionsByType(RLVRestrictionType.EditAttach)
                    .Count != 0;
                if (hasEditAttachRestriction)
                {
                    return false;
                }
            }

            return true;
        }

        #region Touch
        public bool CanFarTouch(out float farTouchDist)
        {
            return TryGetOptionalRestrictionValueMin(_restrictionProvider, RLVRestrictionType.FarTouch, 1.5f, out farTouchDist);
        }

        private bool CanTouchHud(Guid objectId)
        {
            if (!CanInteract())
            {
                return false;
            }

            return !_restrictionProvider
                .GetRestrictionsByType(RLVRestrictionType.TouchHud)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is Guid restrictedObjectId && restrictedObjectId == objectId))
                .Any();
        }

        private bool CanTouchAttachment(bool isAttachedToSelf, Guid? otherUserId)
        {
            if (_restrictionProvider.GetRestrictionsByType(RLVRestrictionType.TouchAttach).Count != 0)
            {
                return false;
            }

            if (isAttachedToSelf)
            {
                if (_restrictionProvider.GetRestrictionsByType(RLVRestrictionType.TouchAttachSelf).Count != 0)
                {
                    return false;
                }
            }
            else
            {
                var isForbiddenFromTouchingOthers = _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.TouchAttachOther)
                    .Where(n => n.Args.Count == 0 || (n.Args[0] is Guid restrictedUserId && restrictedUserId == otherUserId))
                    .Any();
                if (isForbiddenFromTouchingOthers)
                {
                    return false;
                }
            }

            return true;
        }

        public enum TouchLocation
        {
            Hud,
            AttachedSelf,
            AttachedOther,
            RezzedInWorld
        }
        public bool CanTouch(TouchLocation location, Guid objectId, Guid? userId, float? distance)
        {
            if (distance != null)
            {
                if (TryGetRestrictionValueMin(_restrictionProvider, RLVRestrictionType.FarTouch, out var maxTouchDistance))
                {
                    if (distance > maxTouchDistance)
                    {
                        return false;
                    }
                }
            }

            if (!CanInteract())
            {
                return false;
            }

            if (_restrictionProvider
                .GetRestrictionsByType(RLVRestrictionType.TouchMe)
                .Where(n => n.Sender == objectId)
                .Any())
            {
                return true;
            }

            if (_restrictionProvider
                .GetRestrictionsByType(RLVRestrictionType.TouchThis)
                .Where(n => n.Args.Count == 1 && n.Args[0] is Guid restrictedItemId && restrictedItemId == objectId)
                .Any())
            {
                return false;
            }

            if (location != TouchLocation.Hud)
            {
                if (_restrictionProvider.GetRestrictionsByType(RLVRestrictionType.TouchAll).Count != 0)
                {
                    return false;
                }
            }

            if (location == TouchLocation.RezzedInWorld)
            {
                var touchWorldRestrictions = _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.TouchWorld);
                var hasException = touchWorldRestrictions
                    .Where(n => n.IsException && n.Args.Count == 1 && n.Args[0] is Guid allowedObjectId && allowedObjectId == objectId)
                    .Any();

                if (!hasException && touchWorldRestrictions.Any(n => n.Args.Count == 0))
                {
                    return false;
                }
            }
            else if (location is TouchLocation.AttachedSelf or TouchLocation.AttachedOther)
            {
                if (!CanTouchAttachment(location == TouchLocation.AttachedSelf, userId))
                {
                    return false;
                }
            }

            if (location == TouchLocation.Hud)
            {
                if (!CanTouchHud(objectId))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        public enum HoverTextLocation
        {
            World,
            Hud
        }
        public bool CanShowHoverText(HoverTextLocation location, Guid? objectId)
        {
            if (_restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ShowHoverTextAll).Count != 0)
            {
                return false;
            }

            if (_restrictionProvider
                .GetRestrictionsByType(RLVRestrictionType.ShowHoverText)
                .Where(n => n.Args.Count == 1 && n.Args[0] is Guid restrictedObjectId && restrictedObjectId == objectId)
                .Any())
            {
                return false;
            }

            if (location == HoverTextLocation.Hud)
            {
                if (_restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ShowHoverTextHud).Count != 0)
                {
                    return false;
                }
            }
            else if (location == HoverTextLocation.World)
            {
                if (_restrictionProvider.GetRestrictionsByType(RLVRestrictionType.ShowHoverTextWorld).Count != 0)
                {
                    return false;
                }
            }

            return true;
        }

        #region Attach / Detach
        private bool CanUnsharedWear()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.UnsharedWear).Count == 0;
        }
        private bool CanUnsharedUnwear()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.UnsharedUnwear).Count == 0;
        }
        private bool CanSharedWear()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SharedWear).Count == 0;
        }
        private bool CanSharedUnwear()
        {
            return _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SharedUnwear).Count == 0;
        }

        private bool CanAttachWearable(WearableType? typeToRemove)
        {
            return !_restrictionProvider
                .GetRestrictionsByType(RLVRestrictionType.AddOutfit)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is WearableType restrictedType && typeToRemove == restrictedType))
                .Any();
        }
        private bool CanDetachWearable(WearableType? typeToRemove)
        {
            return !_restrictionProvider
                .GetRestrictionsByType(RLVRestrictionType.RemOutfit)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is WearableType restrictedType && typeToRemove == restrictedType))
                .Any();
        }
        private bool CanDetachAttached(AttachmentPoint? attachmentPoint)
        {
            return !_restrictionProvider
                .GetRestrictionsByType(RLVRestrictionType.RemAttach)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is AttachmentPoint restrictedAttachmentPoint && attachmentPoint == restrictedAttachmentPoint))
                .Any();
        }
        private bool CanAttachAttached(AttachmentPoint? attachmentPoint)
        {
            return !_restrictionProvider
                .GetRestrictionsByType(RLVRestrictionType.AddAttach)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is AttachmentPoint restrictedAttachmentPoint && attachmentPoint == restrictedAttachmentPoint))
                .Any();
        }

        public bool CanAttach(InventoryItem item, bool isShared)
        {
            return CanAttach(
                item.FolderId,
                isShared,
                item.AttachedTo,
                item.WornOn
            );
        }
        public bool CanAttach(Guid? objectFolderId, bool isShared, AttachmentPoint? attachmentPoint, WearableType? wearableType)
        {
            if (wearableType != null && !CanAttachWearable(wearableType))
            {
                return false;
            }

            if (attachmentPoint != null && !CanAttachAttached(attachmentPoint))
            {
                return false;
            }

            if (isShared)
            {
                if (!CanSharedWear())
                {
                    return false;
                }

                if (!objectFolderId.HasValue)
                {
                    return false;
                }

                if (_restrictionProvider.TryGetLockedFolder(objectFolderId.Value, out var lockedFolder))
                {
                    if (!lockedFolder.CanAttach)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!CanUnsharedWear())
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanDetach(InventoryItem item, bool isShared)
        {
            return CanDetach(
                item.FolderId,
                isShared,
                item.AttachedTo,
                item.WornOn
            );
        }
        public bool CanDetach(Guid? folderId, bool isShared, AttachmentPoint? attachmentPoint, WearableType? wearableType)
        {
            if (wearableType != null && !CanDetachWearable(wearableType))
            {
                return false;
            }

            if (attachmentPoint != null && !CanDetachAttached(attachmentPoint))
            {
                return false;
            }

            var detachRestrictions = _restrictionProvider.GetRestrictionsByType(RLVRestrictionType.Detach);
            foreach (var restriction in detachRestrictions)
            {
                if (restriction.Args.Count == 0)
                {
                    return false;
                }

                if (restriction.Args[0] is AttachmentPoint restrictedAttachmentPoint && attachmentPoint == restrictedAttachmentPoint)
                {
                    return false;
                }
            }

            if (isShared)
            {
                if (!CanSharedUnwear())
                {
                    return false;
                }

                if (!folderId.HasValue)
                {
                    return false;
                }

                if (_restrictionProvider.TryGetLockedFolder(folderId.Value, out var lockedFolder))
                {
                    if (!lockedFolder.CanDetach)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!CanUnsharedUnwear())
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
