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

        internal static bool GetRestrictionValueMax<T>(IRestrictionProvider _restrictionProvider, RLVRestrictionType restrictionType, out T val)
        {
            var restriction = _restrictionProvider.GetRestrictions(restrictionType);
            if (restriction.Count == 0)
            {
                val = default;
                return false;
            }

            val = restriction
                .Where(n => n.Args.Count > 0 && n.Args[0] is T)
                .Select(n => (T)n.Args[0])
                .Max();

            return true;
        }

        internal static bool GetRestrictionValueMin<T>(IRestrictionProvider _restrictionProvider, RLVRestrictionType restrictionType, out T val)
        {
            var restriction = _restrictionProvider.GetRestrictions(restrictionType);
            if (restriction.Count == 0)
            {
                val = default;
                return false;
            }

            val = restriction
                .Where(n => n.Args.Count > 0 && n.Args[0] is T)
                .Select(n => (T)n.Args[0])
                .Min();

            return true;
        }

        internal static bool GetOptionalRestrictionValueMin<T>(IRestrictionProvider _restrictionProvider, RLVRestrictionType restrictionType, T defaultVal, out T val)
        {
            var restrictions = _restrictionProvider.GetRestrictions(restrictionType);
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
                    .Where(n => n.Args.Count > 0 && n.Args[0] is T)
                    .Select(n => (T)n.Args[0])
                    .Min();
            }

            return true;
        }

        private bool CheckSecureRestriction(Guid? userId, string groupName, RLVRestrictionType normalType, RLVRestrictionType? secureType, RLVRestrictionType? fromToType)
        {
            // Explicit restrictions
            if (fromToType != null)
            {
                var isRestrictedBySendImTo = _restrictionProvider.GetRestrictions(fromToType.Value)
                    .Where(n => n.Args.Count == 1 &&
                        ((userId != null && n.Args[0] is Guid restrictedId && userId == restrictedId) ||
                        (groupName != null && n.Args[0] is string restrictedGroupName && (restrictedGroupName == "allgroups" || restrictedGroupName == groupName)))
                    ).Any();
                if (isRestrictedBySendImTo)
                {
                    return false;
                }
            }

            var sendImRestrictions = _restrictionProvider.GetRestrictions(normalType);
            var sendImExceptions = sendImRestrictions
                .Where(n => n.IsException && n.Args.Count == 1 &&
                    ((userId != null && n.Args[0] is Guid restrictedId && userId == restrictedId) ||
                    (groupName != null && n.Args[0] is string restrictedGroupName && (restrictedGroupName == "allgroups" || restrictedGroupName == groupName)))
                ).ToList();

            // Secure restrictions
            if (secureType != null)
            {
                var sendImRestrictionsSecure = _restrictionProvider.GetRestrictions(secureType.Value);
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
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.Fly).Count == 0;
        }
        public bool CanJump()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.Jump).Count == 0;
        }
        public bool CanTempRun()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.TempRun).Count == 0;
        }
        public bool CanAlwaysRun()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.AlwaysRun).Count == 0;
        }
        public bool CanUnsit()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.Unsit).Count == 0;
        }
        public bool CanSit()
        {
            if (!CanInteract())
            {
                return false;
            }

            return _restrictionProvider.GetRestrictions(RLVRestrictionType.Sit).Count == 0;
        }

        #region TP

        public bool CanTpLm()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.TpLm).Count == 0;
        }
        public bool CanTpLoc()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.TpLoc).Count == 0;
        }
        public bool CanSitTp(out float sitTpDist)
        {
            return GetOptionalRestrictionValueMin(_restrictionProvider, RLVRestrictionType.SitTp, 1.5f, out sitTpDist);
        }
        public bool CanTpLocal(out float tpLocalDist)
        {
            return GetOptionalRestrictionValueMin(_restrictionProvider, RLVRestrictionType.TpLocal, 0.0f, out tpLocalDist);
        }
        public bool CanStandTp()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.StandTp).Count == 0;
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
            var restrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.AcceptTp);
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
            var restrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.AcceptTpRequest);
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
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.ShowInv).Count == 0;
        }
        public bool CanViewNote()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.ViewNote).Count == 0;
        }
        public bool CanViewScript()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.ViewScript).Count == 0;
        }
        public bool CanViewTexture()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.ViewTexture).Count == 0;
        }

        public bool CanDefaultWear()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.DefaultWear).Count == 0;
        }
        public bool CanSetGroup()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.SetGroup).Count == 0;
        }
        public bool CanSetDebug()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.SetDebug).Count == 0;
        }
        public bool CanSetEnv()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.SetEnv).Count == 0;
        }
        public bool CanAllowIdle()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.AllowIdle).Count == 0;
        }
        public bool CanInteract()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.Interact).Count == 0;
        }
        public bool CanShowWorldMap()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.ShowWorldMap).Count == 0;
        }
        public bool CanShowMiniMap()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.ShowMiniMap).Count == 0;
        }
        public bool CanShowLoc()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.ShowLoc).Count == 0;
        }
        public bool CanShowNearby()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.ShowNearby).Count == 0;
        }

        public bool IsAutoDenyPermissions()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.DenyPermission).Count != 0;
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

        public bool CanSendIM(string message, Guid? userId, string groupName = null)
        {
            return CheckSecureRestriction(userId, groupName, RLVRestrictionType.SendIm, RLVRestrictionType.SendImSec, RLVRestrictionType.SendImTo);
        }

        public bool CanReceiveIM(string message, Guid? userId, string groupName = null)
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
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.ChatShout).Count == 0;
        }
        public bool CanChatWhisper()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.ChatWhisper).Count == 0;
        }
        public bool CanChatNormal()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.ChatNormal).Count == 0;
        }
        public bool CanSendChat()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.SendChat).Count == 0;
        }
        public bool CanEmote()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.Emote).Count == 0;
        }
        public bool CanSendGesture()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.SendGesture).Count == 0;
        }

        public bool IsRedirChat(out List<int> channels)
        {
            channels = _restrictionProvider
                .GetRestrictions(RLVRestrictionType.RedirChat)
                .Where(n => n.Args.Count == 1 && n.Args[0] is int)
                .Select(n => (int)n.Args[0])
                .Distinct()
                .ToList();

            return channels.Count > 0;
        }

        public bool IsRedirEmote(out List<int> channels)
        {
            channels = _restrictionProvider
                .GetRestrictions(RLVRestrictionType.RedirEmote)
                .Where(n => n.Args.Count == 1 && n.Args[0] is int)
                .Select(n => (int)n.Args[0])
                .Distinct()
                .ToList();

            return channels.Count > 0;
        }

        private bool CanChatOnChannelPrivateChannel(int channel)
        {
            var sendChannelExceptRestrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.SendChannelExcept);

            // @sendchannel_except
            foreach (var restriction in sendChannelExceptRestrictions)
            {
                if (restriction.Args.Count == 0)
                {
                    continue;
                }

                if (!(restriction.Args[0] is int restrictedChannel))
                {
                    continue;
                }

                if (channel == restrictedChannel)
                {
                    return false;
                }
            }

            var sendChannelRestrictionsSecure = _restrictionProvider.GetRestrictions(RLVRestrictionType.SendChannelSec);
            var sendChannelRestrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.SendChannel);
            var channelExceptions = sendChannelRestrictions
                .Where(n =>
                    n.IsException &&
                    n.Args.Count > 0 &&
                    n.Args[0] is int exceptionChannel &&
                    exceptionChannel == channel
                )
                .ToList();

            // @sendchannel_sec
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

            // @sendchannel
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
                // @sendchat=<y/n>
                //      @emote=<rem/add>

                var canEmote = _restrictionProvider.GetRestrictions(RLVRestrictionType.Emote).Count == 0;
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

            return _restrictionProvider.GetRestrictions(RLVRestrictionType.AcceptPermission).Count != 0;
        }


        public bool IsPermissive()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.Permissive).Count == 0;
        }

        public bool CanRez()
        {
            if (!CanInteract())
            {
                return false;
            }

            return _restrictionProvider.GetRestrictions(RLVRestrictionType.Rez).Count == 0;
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

            // @edit=<y/n>
            // @edit:<Guid>=<rem/add>
            // @editobj:<Guid>=<y/n>
            var canEditObject = CheckSecureRestriction(objectId, null, RLVRestrictionType.Edit, null, RLVRestrictionType.EditObj);
            if (!canEditObject)
            {
                return false;
            }

            if (objectLocation == ObjectLocation.RezzedInWorld)
            {
                // @editworld=<y/n>
                var hasEditWorldRestriction = _restrictionProvider
                    .GetRestrictions(RLVRestrictionType.EditWorld)
                    .Count != 0;
                if (hasEditWorldRestriction)
                {
                    return false;
                }
            }

            if (objectLocation == ObjectLocation.Attached)
            {
                // @editattach=<y/n>
                var hasEditAttachRestriction = _restrictionProvider
                    .GetRestrictions(RLVRestrictionType.EditAttach)
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
            return GetOptionalRestrictionValueMin(_restrictionProvider, RLVRestrictionType.FarTouch, 1.5f, out farTouchDist);
        }

        private bool CanTouchHud(Guid objectId)
        {
            if (!CanInteract())
            {
                return false;
            }

            return !_restrictionProvider
                .GetRestrictions(RLVRestrictionType.TouchHud)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is Guid restrictedObjectId && restrictedObjectId == objectId))
                .Any();
        }

        private bool CanTouchAttachment(bool isAttachedToSelf, Guid? otherUserId)
        {
            // @touchattach
            if (_restrictionProvider.GetRestrictions(RLVRestrictionType.TouchAttach).Count != 0)
            {
                return false;
            }

            if (isAttachedToSelf)
            {
                // @touchattachself
                if (_restrictionProvider.GetRestrictions(RLVRestrictionType.TouchAttachSelf).Count != 0)
                {
                    return false;
                }
            }
            else
            {
                // @touchattachother
                var isForbiddenFromTouchingOthers = _restrictionProvider.GetRestrictions(RLVRestrictionType.TouchAttachOther)
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
            // @FarTouch | TouchFar ?
            if (distance != null)
            {
                if (GetRestrictionValueMin(_restrictionProvider, RLVRestrictionType.FarTouch, out float? maxTouchDistance))
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

            // @TouchMe
            if (_restrictionProvider
                .GetRestrictions(RLVRestrictionType.TouchMe)
                .Where(n => n.Sender == objectId)
                .Any())
            {
                return true;
            }

            // @TouchThis
            if (_restrictionProvider
                .GetRestrictions(RLVRestrictionType.TouchThis)
                .Where(n => n.Args.Count == 1 && n.Args[0] is Guid restrictedItemId && restrictedItemId == objectId)
                .Any())
            {
                return false;
            }

            if (location != TouchLocation.Hud)
            {
                // @TouchAll
                if (_restrictionProvider.GetRestrictions(RLVRestrictionType.TouchAll).Count != 0)
                {
                    return false;
                }
            }

            if (location == TouchLocation.RezzedInWorld)
            {
                // @touchworld
                var touchWorldRestrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.TouchWorld);
                var hasException = touchWorldRestrictions
                    .Where(n => n.IsException && n.Args.Count == 1 && n.Args[0] is Guid allowedObjectId && allowedObjectId == objectId)
                    .Any();

                if (!hasException && touchWorldRestrictions.Any(n => n.Args.Count == 0))
                {
                    return false;
                }
            }
            else if (location == TouchLocation.AttachedSelf || location == TouchLocation.AttachedOther)
            {
                // @TouchAttachOther
                // @TouchAttach
                // @TouchAttachSelf
                if (!CanTouchAttachment(location == TouchLocation.AttachedSelf, userId))
                {
                    return false;
                }
            }

            if (location == TouchLocation.Hud)
            {
                // @TouchHud
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
            // @showhovertextall
            if (_restrictionProvider.GetRestrictions(RLVRestrictionType.ShowHoverTextAll).Count != 0)
            {
                return false;
            }

            // @showhovertext:<Guid>
            if (_restrictionProvider
                .GetRestrictions(RLVRestrictionType.ShowHoverText)
                .Where(n => n.Args.Count == 1 && n.Args[0] is Guid restrictedObjectId && restrictedObjectId == objectId)
                .Any())
            {
                return false;
            }

            if (location == HoverTextLocation.Hud)
            {
                // @showhovertexthud
                if (_restrictionProvider.GetRestrictions(RLVRestrictionType.ShowHoverTextHud).Count != 0)
                {
                    return false;
                }
            }
            else if (location == HoverTextLocation.World)
            {
                // @showhovertexthud
                if (_restrictionProvider.GetRestrictions(RLVRestrictionType.ShowHoverTextWorld).Count != 0)
                {
                    return false;
                }
            }

            return true;
        }

        #region Attach / Detach
        private bool CanUnsharedWear()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.UnsharedWear).Count == 0;
        }
        private bool CanUnsharedUnwear()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.UnsharedUnwear).Count == 0;
        }
        private bool CanSharedWear()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.SharedWear).Count == 0;
        }
        private bool CanSharedUnwear()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.SharedUnwear).Count == 0;
        }

        private bool CanAttachWearable(WearableType? typeToRemove)
        {
            return !_restrictionProvider
                .GetRestrictions(RLVRestrictionType.AddOutfit)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is WearableType restrictedType && typeToRemove == restrictedType))
                .Any();
        }
        private bool CanDetachWearable(WearableType? typeToRemove)
        {
            return !_restrictionProvider
                .GetRestrictions(RLVRestrictionType.RemOutfit)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is WearableType restrictedType && typeToRemove == restrictedType))
                .Any();
        }
        private bool CanDetachAttached(AttachmentPoint? attachmentPoint)
        {
            return !_restrictionProvider
                .GetRestrictions(RLVRestrictionType.RemAttach)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is AttachmentPoint restrictedAttachmentPoint && attachmentPoint == restrictedAttachmentPoint))
                .Any();
        }
        private bool CanAttachAttached(AttachmentPoint? attachmentPoint)
        {
            return !_restrictionProvider
                .GetRestrictions(RLVRestrictionType.AddAttach)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is AttachmentPoint restrictedAttachmentPoint && attachmentPoint == restrictedAttachmentPoint))
                .Any();
        }

        public bool CanAttach(InventoryTree.InventoryItem item, bool isShared)
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

        public bool CanDetach(InventoryTree.InventoryItem item, bool isShared)
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
            // @remoutfit[:<part>]=<y/n>
            if (wearableType != null && !CanDetachWearable(wearableType))
            {
                return false;
            }

            // @remattach[:<attach_point_name>]=<y/n>
            if (attachmentPoint != null && !CanDetachAttached(attachmentPoint))
            {
                return false;
            }

            // @detach=<y/n>
            // @detach:<attach_point_name>=<y/n>
            var detachRestrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.Detach);
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
