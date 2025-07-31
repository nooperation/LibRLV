using System;
using System.Collections.Generic;
using System.Linq;
using OpenMetaverse;

namespace LibRLV
{
    public class RLVManager
    {
        private readonly IRestrictionProvider _restrictionProvider;
        private readonly IRLVCallbacks _callbacks;

        public RLVManager(IRestrictionProvider restrictionProvider, IRLVCallbacks callbacks)
        {
            _restrictionProvider = restrictionProvider;
            _callbacks = callbacks;
        }



        private bool GetRestrictionValueMax<T>(RLVRestrictionType restrictionType, out T val)
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

        private bool GetRestrictionValueMin<T>(RLVRestrictionType restrictionType, out T val)
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

        private bool GetOptionalRestrictionValueMin<T>(RLVRestrictionType restrictionType, T defaultVal, out T val)
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

        public bool CanFly()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.Fly).Any();
        }

        public bool CanJump()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.Jump).Any();
        }
        public bool CanTempRun()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.TempRun).Any();
        }
        public bool CanAlwaysRun()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.AlwaysRun).Any();
        }
        public bool CanChatShout()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.ChatShout).Any();
        }
        public bool CanChatWhisper()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.ChatWhisper).Any();
        }
        public bool CanChatNormal()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.ChatNormal).Any();
        }
        public bool CanSendChat()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.SendChat).Any();
        }
        public bool CanEmote()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.Emote).Any();
        }
        public bool CanSendGesture()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.SendGesture).Any();
        }
        public bool IsCamLocked()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.SetCamUnlock).Any();
        }
        public bool CanTpLm()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.TpLm).Any();
        }
        public bool CanTpLoc()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.TpLoc).Any();
        }
        public bool CanStandTp()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.StandTp).Any();
        }
        public bool CanShowInv()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.ShowInv).Any();
        }
        public bool CanViewNote()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.ViewNote).Any();
        }
        public bool CanViewScript()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.ViewScript).Any();
        }
        public bool CanViewTexture()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.ViewTexture).Any();
        }
        public bool CanUnsit()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.Unsit).Any();
        }
        public bool CanSit()
        {
            if (!CanInteract())
            {
                return false;
            }

            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.Sit).Any();
        }
        public bool CanDefaultWear()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.DefaultWear).Any();
        }
        public bool CanSetGroup()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.SetGroup).Any();
        }
        public bool CanSetDebug()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.SetDebug).Any();
        }
        public bool CanSetEnv()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.SetEnv).Any();
        }
        public bool CanAllowIdle()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.AllowIdle).Any();
        }
        public bool CanInteract()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.Interact).Any();
        }
        public bool CanShowWorldMap()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.ShowWorldMap).Any();
        }
        public bool CanShowMiniMap()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.ShowMiniMap).Any();
        }
        public bool CanShowLoc()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.ShowLoc).Any();
        }
        public bool CanShowNearby()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.ShowNearby).Any();
        }
        public bool CanUnsharedWear()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.UnsharedWear).Any();
        }
        public bool CanUnsharedUnwear()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.UnsharedUnwear).Any();
        }
        public bool CanSharedWear()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.SharedWear).Any();
        }
        public bool CanSharedUnwear()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.SharedUnwear).Any();
        }
        public bool IsAutoDenyPermissions()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.DenyPermission).Any();
        }

        public bool HasCamZoomMin(out float camZoomMin)
        {
            return GetRestrictionValueMax(RLVRestrictionType.CamZoomMin, out camZoomMin);
        }
        public bool HasSetCamFovMin(out float setCamFovMin)
        {
            return GetRestrictionValueMax(RLVRestrictionType.SetCamFovMin, out setCamFovMin);
        }

        public bool HasSetCamAvDistMin(out float setCamAvDistMin)
        {
            return GetRestrictionValueMax(RLVRestrictionType.SetCamAvDistMin, out setCamAvDistMin);
        }
        public bool HasCamZoomMax(out float camZoomMax)
        {
            return GetRestrictionValueMin(RLVRestrictionType.CamZoomMax, out camZoomMax);
        }
        public bool HasCamDrawMin(out float camDrawMin)
        {
            return GetRestrictionValueMin(RLVRestrictionType.CamDrawMin, out camDrawMin);
        }
        public bool HasCamDrawMax(out float camDrawMax)
        {
            return GetRestrictionValueMin(RLVRestrictionType.CamDrawMax, out camDrawMax);
        }
        public bool HasCamDrawAlphaMin(out float camDrawAlphaMin)
        {
            return GetRestrictionValueMin(RLVRestrictionType.CamDrawAlphaMin, out camDrawAlphaMin);
        }
        public bool HasCamDrawAlphaMax(out float camDrawAlphaMax)
        {
            return GetRestrictionValueMin(RLVRestrictionType.CamDrawAlphaMax, out camDrawAlphaMax);
        }
        public bool HasSetCamFovMax(out float setCamFovMax)
        {
            return GetRestrictionValueMin(RLVRestrictionType.SetCamFovMax, out setCamFovMax);
        }

        public bool HasSetCamAvDistMax(out float setCamAvDistMax)
        {
            return GetRestrictionValueMin(RLVRestrictionType.SetCamAvDistMax, out setCamAvDistMax);
        }
        public bool HasCamAvDist(out float camAvDist)
        {
            return GetRestrictionValueMin(RLVRestrictionType.CamAvDist, out camAvDist);
        }

        public bool CanSitTp(out float sitTpDist)
        {
            return GetOptionalRestrictionValueMin(RLVRestrictionType.SitTp, 1.5f, out sitTpDist);
        }
        public bool CanFarTouch(out float farTouchDist)
        {
            return GetOptionalRestrictionValueMin(RLVRestrictionType.FarTouch, 1.5f, out farTouchDist);
        }
        public bool CanTpLocal(out float tpLocalDist)
        {
            return GetOptionalRestrictionValueMin(RLVRestrictionType.TpLocal, 0.0f, out tpLocalDist);
        }

        public bool HasCamDrawColor(out Vector3 camDrawColor)
        {
            camDrawColor.X = 0;
            camDrawColor.Y = 0;
            camDrawColor.Z = 0;

            var restrictions = _restrictionProvider
                .GetRestrictions(RLVRestrictionType.CamDrawColor)
                .Where(n => n.Args.Count == 3)
                .ToList();
            if (restrictions.Count == 0)
            {
                return false;
            }

            foreach (var restriction in restrictions)
            {
                camDrawColor.X += Math.Min(1.0f, Math.Max(0.0f, (float)restriction.Args[0]));
                camDrawColor.Y += Math.Min(1.0f, Math.Max(0.0f, (float)restriction.Args[1]));
                camDrawColor.Z += Math.Min(1.0f, Math.Max(0.0f, (float)restriction.Args[2]));
            }

            camDrawColor.X /= restrictions.Count;
            camDrawColor.Y /= restrictions.Count;
            camDrawColor.Z /= restrictions.Count;

            return true;
        }

        #region Chat
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
                    .Where(n => n.Sender == restriction.Sender).Any();
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

                var canEmote = !_restrictionProvider.GetRestrictions(RLVRestrictionType.Emote).Any();
                if (message.StartsWith("/me ") && !canEmote)
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

                    if (message.IndexOfAny(new char[] { '(', ')', '"', '-', '*', '=', '_', '^' }) != -1)
                    {
                        return false;
                    }

                    if (!message.StartsWith("/"))
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

        private bool CheckSecureRestriction(UUID? userId, string groupName, RLVRestrictionType normalType, RLVRestrictionType? secureType, RLVRestrictionType? fromToType)
        {
            // Explicit restrictions
            if (fromToType != null)
            {
                var isRestrictedBySendImTo = _restrictionProvider.GetRestrictions(fromToType.Value)
                    .Where(n => n.Args.Count == 1 &&
                        ((userId != null && n.Args[0] is UUID restrictedId && userId == restrictedId) ||
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
                    ((userId != null && n.Args[0] is UUID restrictedId && userId == restrictedId) ||
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

        public bool CanStartIM(UUID? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.StartIm, null, RLVRestrictionType.StartImTo);
        }

        public bool CanSendIM(string message, UUID? userId, string groupName = null)
        {
            return CheckSecureRestriction(userId, groupName, RLVRestrictionType.SendIm, RLVRestrictionType.SendImSec, RLVRestrictionType.SendImTo);
        }

        public bool CanReceiveIM(string message, UUID? userId, string groupName = null)
        {
            return CheckSecureRestriction(userId, groupName, RLVRestrictionType.RecvIm, RLVRestrictionType.RecvImSec, RLVRestrictionType.RecvImFrom);
        }

        public bool CanReceiveChat(string message, UUID? userId)
        {
            if (message.StartsWith("/me "))
            {
                return CheckSecureRestriction(userId, null, RLVRestrictionType.RecvEmote, RLVRestrictionType.RecvEmoteSec, RLVRestrictionType.RecvEmoteFrom);
            }
            else
            {
                return CheckSecureRestriction(userId, null, RLVRestrictionType.RecvChat, RLVRestrictionType.RecvChatSec, RLVRestrictionType.RecvChatFrom);
            }
        }

        public bool CanShowNames(UUID? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.ShowNames, RLVRestrictionType.ShowNamesSec, null);
        }

        public bool CanShowNameTags(UUID? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.ShowNameTags, null, null);
        }

        public bool CanTPLure(UUID? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.TpLure, RLVRestrictionType.TpLureSec, null);
        }

        public bool CanTpRequest(UUID? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.TpRequest, RLVRestrictionType.TpRequestSec, null);
        }

        public bool CanShare(UUID? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.Share, RLVRestrictionType.ShareSec, null);
        }



        public bool IsAutoAcceptPermissions()
        {
            if (IsAutoDenyPermissions())
            {
                return false;
            }

            return _restrictionProvider.GetRestrictions(RLVRestrictionType.AcceptPermission).Any();
        }

        public bool IsAutoAcceptTp(UUID? userId = null)
        {
            var restrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.AcceptTp);
            foreach (var restriction in restrictions)
            {
                if (restriction.Args.Count == 0)
                {
                    return true;
                }

                if (restriction.Args[0] is UUID allowedUserID && allowedUserID == userId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAutoAcceptTpRequest(UUID? userId = null)
        {
            var restrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.AcceptTpRequest);
            foreach (var restriction in restrictions)
            {
                if (restriction.Args.Count == 0)
                {
                    return true;
                }

                if (restriction.Args[0] is UUID allowedUserID && allowedUserID == userId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasSetCamtextures(out UUID? textureUUID)
        {
            textureUUID = null;

            var restrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.SetCamTextures);
            if (restrictions.Count == 0)
            {
                return false;
            }

            foreach (var restriction in restrictions)
            {
                if (restriction.Args.Count == 0)
                {
                    textureUUID = UUID.Zero;
                }
                else if (restriction.Args.Count == 1 && restriction.Args[0] is UUID restrictionTexture)
                {
                    textureUUID = restrictionTexture;
                }
                else
                {
                    textureUUID = UUID.Zero;
                    return false;
                }
            }

            return true;
        }

        public enum ObjectLocation
        {
            Hud,
            Attached,
            RezzedInWorld
        }
        public bool CanEdit(ObjectLocation objectLocation, UUID? objectId)
        {
            if (!CanInteract())
            {
                return false;
            }

            // @edit=<y/n>
            // @edit:<UUID>=<rem/add>
            // @editobj:<UUID>=<y/n>
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
                    .Any();
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
                    .Any();
                if (hasEditAttachRestriction)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanAttachWearable(WearableType? typeToRemove)
        {
            return !_restrictionProvider
                .GetRestrictions(RLVRestrictionType.AddOutfit)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is WearableType restrictedType && typeToRemove == restrictedType))
                .Any();
        }

        public bool CanDetachWearable(WearableType? typeToRemove)
        {
            return !_restrictionProvider
                .GetRestrictions(RLVRestrictionType.RemOutfit)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is WearableType restrictedType && typeToRemove == restrictedType))
                .Any();
        }

        public bool CanDetachAttached(AttachmentPoint? attachmentPoint)
        {
            return !_restrictionProvider
                .GetRestrictions(RLVRestrictionType.RemAttach)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is AttachmentPoint restrictedAttachmentPoint && attachmentPoint == restrictedAttachmentPoint))
                .Any();
        }

        public bool CanAttachAttached(AttachmentPoint? attachmentPoint)
        {
            return !_restrictionProvider
                .GetRestrictions(RLVRestrictionType.AddAttach)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is AttachmentPoint restrictedAttachmentPoint && attachmentPoint == restrictedAttachmentPoint))
                .Any();
        }

        public bool IsPermissive()
        {
            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.Permissive).Any();
        }

        public bool CanRez()
        {
            if (!CanInteract())
            {
                return false;
            }

            return !_restrictionProvider.GetRestrictions(RLVRestrictionType.Rez).Any();
        }

        public bool CanTouchHud(UUID objectId)
        {
            if (!CanInteract())
            {
                return false;
            }

            return !_restrictionProvider
                .GetRestrictions(RLVRestrictionType.TouchHud)
                .Where(n => n.Args.Count == 0 || (n.Args[0] is UUID restrictedObjectId && restrictedObjectId == objectId))
                .Any();
        }

        private bool CanTouchAttachment(bool isAttachedToSelf, UUID? otherUserId)
        {
            // @touchattach
            if (_restrictionProvider.GetRestrictions(RLVRestrictionType.TouchAttach).Any())
            {
                return false;
            }

            if (isAttachedToSelf)
            {
                // @touchattachself
                if (_restrictionProvider.GetRestrictions(RLVRestrictionType.TouchAttachSelf).Any())
                {
                    return false;
                }
            }
            else
            {
                // @touchattachother
                var isForbiddenFromTouchingOthers = _restrictionProvider.GetRestrictions(RLVRestrictionType.TouchAttachOther)
                    .Where(n => n.Args.Count == 0 || (n.Args[0] is UUID restrictedUserId && restrictedUserId == otherUserId))
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
        public bool CanTouch(TouchLocation location, UUID objectId, UUID? userId, float? distance)
        {
            // @FarTouch | TouchFar ?
            if (distance != null)
            {
                if (GetRestrictionValueMin(RLVRestrictionType.CamZoomMax, out float? maxTouchDistance))
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
                .Where(n => n.Args.Count == 1 && n.Args[0] is UUID restrictedItemId && restrictedItemId == objectId)
                .Any())
            {
                return false;
            }

            if (location != TouchLocation.Hud)
            {
                // @TouchAll
                if (_restrictionProvider.GetRestrictions(RLVRestrictionType.TouchAll).Any())
                {
                    return false;
                }
            }

            if (location == TouchLocation.RezzedInWorld)
            {
                // @touchworld
                var touchWorldRestrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.TouchWorld);
                var hasException = touchWorldRestrictions
                    .Where(n => n.IsException && n.Args.Count == 1 && n.Args[0] is UUID allowedObjectId && allowedObjectId == objectId)
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

        public enum HoverTextLocation
        {
            World,
            Hud
        }
        public bool ShowHoverText(HoverTextLocation location, UUID? objectId)
        {
            // @showhovertextall
            if (_restrictionProvider.GetRestrictions(RLVRestrictionType.ShowHoverTextAll).Any())
            {
                return false;
            }

            // @showhovertext:<UUID>
            if (_restrictionProvider
                .GetRestrictions(RLVRestrictionType.ShowHoverText)
                .Where(n => n.Args.Count == 1 && n.Args[0] is UUID restrictedObjectId && restrictedObjectId == objectId)
                .Any())
            {
                return false;
            }

            if (location == HoverTextLocation.Hud)
            {
                // @showhovertexthud
                if (_restrictionProvider.GetRestrictions(RLVRestrictionType.ShowHoverTextHud).Any())
                {
                    return false;
                }
            }
            else if (location == HoverTextLocation.World)
            {
                // @showhovertexthud
                if (_restrictionProvider.GetRestrictions(RLVRestrictionType.ShowHoverTextWorld).Any())
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanAttach(InventoryTree.InventoryItem item, bool isShared)
        {
            return CanAttach(
                item.Id,
                item.FolderId,
                isShared,
                item.AttachedTo,
                item.WornOn
            );
        }
        public bool CanAttach(UUID objectId, UUID objectFolderId, bool isShared, AttachmentPoint? attachmentPoint, WearableType? wearableType)
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

                if (_restrictionProvider.TryGetLockedFolder(objectFolderId, out var lockedFolder))
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
                item.Id,
                item.FolderId,
                isShared,
                item.AttachedTo,
                item.WornOn
            );
        }
        public bool CanDetach(UUID itemId, UUID folderId, bool isShared, AttachmentPoint? attachmentPoint, WearableType? wearableType)
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

                if (_restrictionProvider.TryGetLockedFolder(folderId, out var lockedFolder))
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

        public void ReportSendPublicMessage(string message)
        {
            if (message.StartsWith("/me"))
            {
                if (!IsRedirEmote(out var channels))
                {
                    return;
                }

                foreach (var channel in channels)
                {
                    _callbacks.SendReplyAsync(channel, message, System.Threading.CancellationToken.None);
                }
            }
            else
            {
                if (!IsRedirChat(out var channels))
                {
                    return;
                }

                foreach (var channel in channels)
                {
                    _callbacks.SendReplyAsync(channel, message, System.Threading.CancellationToken.None);
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
        public void ReportWornItemChange(UUID objectId, UUID objectFolderId, bool isShared, WearableType wearableType, WornItemChange changeType)
        {
            var notificationText = "";

            if (changeType == WornItemChange.Attached)
            {
                var isLegal = CanAttach(objectId, objectFolderId, isShared, null, wearableType);

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
                var isLegal = CanDetach(objectId, objectFolderId, isShared, null, wearableType);

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
        public void ReportAttachedItemChange(UUID objectId, UUID objectFolderId, bool isShared, AttachmentPoint attachmentPoint, AttachedItemChange changeType)
        {
            var notificationText = "";

            if (changeType == AttachedItemChange.Attached)
            {
                var isLegal = CanAttach(objectId, objectFolderId, isShared, attachmentPoint, null);

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
                var isLegal = CanDetach(objectId, objectFolderId, isShared, attachmentPoint, null);

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
                var isLegal = CanInteract() && CanSit();

                if (CanSitTp(out var maxObjectDistance))
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
                var isLegal = CanInteract() && CanUnsit();

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
                var isLegal = CanSit();

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
                var isLegal = CanUnsit();

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
            var notificationRestrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.Notify);

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
                    _callbacks.SendReplyAsync(channel, notificationText, System.Threading.CancellationToken.None);
                }
            }
        }
    }
}
