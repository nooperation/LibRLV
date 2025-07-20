using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRLV
{
    public class RLVManager
    {
        IRestrictionProvider _restrictionProvider;

        public RLVManager(IRestrictionProvider restrictionProvider)
        {
            _restrictionProvider = restrictionProvider;
        }

        public bool CanFly()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.Fly).Count == 0;
        }

        public bool CanTempRun()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.TempRun).Count == 0;
        }

        public bool CanAlwaysRun()
        {
            return _restrictionProvider.GetRestrictions(RLVRestrictionType.TempRun).Count == 0;
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
                .Max();

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
                    .Max();
            }

            return true;
        }

        public bool HasCamZoomMax(out float camZoomMax)
        {
            return GetRestrictionValueMin(RLVRestrictionType.CamZoomMax, out camZoomMax);
        }
        public bool HasCamZoomMin(out float camZoomMin)
        {
            return GetRestrictionValueMax(RLVRestrictionType.CamZoomMin, out camZoomMin);
        }
        public bool HasSetCamFovMin(out float setCamFovMin)
        {
            return GetRestrictionValueMax(RLVRestrictionType.SetCamFovMin, out setCamFovMin);
        }
        public bool HasSetCamFovMax(out float setCamFovMax)
        {
            return GetRestrictionValueMin(RLVRestrictionType.SetCamFovMax, out setCamFovMax);
        }
        public bool HasCamDistMax(out float camDistMax)
        {
            return GetRestrictionValueMin(RLVRestrictionType.CamDistMax, out camDistMax);
        }
        public bool HasSetCamAvDistMax(out float setCamAvDistMax)
        {
            return GetRestrictionValueMin(RLVRestrictionType.SetCamAvDistMax, out setCamAvDistMax);
        }
        public bool HasCamDistMin(out float camDistMin)
        {
            return GetRestrictionValueMax(RLVRestrictionType.CamDistMin, out camDistMin);
        }
        public bool HasSetCamAvDistMin(out float setCamAvDistMin)
        {
            return GetRestrictionValueMax(RLVRestrictionType.SetCamAvDistMin, out setCamAvDistMin);
        }
        public bool HasCamDrawAlphaMax(out float camDrawAlphaMax)
        {
            return GetRestrictionValueMin(RLVRestrictionType.CamDrawAlphaMax, out camDrawAlphaMax);
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

        public bool GetCamDrawColor(out Vector3 camDrawColor)
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
                camDrawColor.X += Math.Min(0.0f, Math.Max(1.0f, (float)restriction.Args[0]));
                camDrawColor.Y += Math.Min(0.0f, Math.Max(1.0f, (float)restriction.Args[1]));
                camDrawColor.Z += Math.Min(0.0f, Math.Max(1.0f, (float)restriction.Args[2]));
            }

            camDrawColor.X /= restrictions.Count;
            camDrawColor.Y /= restrictions.Count;
            camDrawColor.Z /= restrictions.Count;

            return true;
        }

        public enum ChatType
        {
            Whisper = 0,
            Normal = 1,
            Shout = 2
        }
        public bool CanChat(int channel, string message, ChatType chatType)
        {
            if (channel == 0)
            {
                // @sendchat=<y/n>
                //      @chatshout=<y/n>
                //      @chatnormal=<y/n>
                //      @chatwhisper=<y/n>
                //      @emote=<rem/add>

                var canEmote = _restrictionProvider.GetRestrictions(RLVRestrictionType.Emote).Count != 0;
                if (message.StartsWith("/me ") && !canEmote)
                {
                    return false;
                }

                var canSendChat = _restrictionProvider.GetRestrictions(RLVRestrictionType.SendChat).Count != 0;
                if (!canSendChat)
                {
                    return false;
                }

                var canChatWhisper = _restrictionProvider.GetRestrictions(RLVRestrictionType.ChatWhisper).Count != 0;
                if (chatType == ChatType.Whisper && !canChatWhisper)
                {
                    return false;
                }

                var canChatShout = _restrictionProvider.GetRestrictions(RLVRestrictionType.ChatShout).Count != 0;
                if (chatType == ChatType.Shout && !canChatShout)
                {
                    return false;
                }

                var canChatNormal = _restrictionProvider.GetRestrictions(RLVRestrictionType.ChatNormal).Count != 0;
                if (chatType == ChatType.Normal && !canChatNormal)
                {
                    return false;
                }
            }
            else
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
                var channelExceptions = sendChannelExceptRestrictions
                    .Where(n =>
                        n.IsException &&
                        n.Args.Count > 0 &&
                        n.Args[0] is int exceptionChannel &&
                        exceptionChannel == channel
                    )
                    .ToList();

                // @sendchannel_sec
                var permissiveMode = false;
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
                foreach (var restriction in sendChannelRestrictions.Where(n => !n.IsException && n.Args.Count == 0))
                {
                    var hasException = channelExceptions
                        .Where(n => !permissiveMode || n.Sender == restriction.Sender)
                        .Any();
                    if (hasException)
                    {
                        continue;
                    }

                    return false;
                }
            }

            return true;
        }

        private bool CheckSecureRestriction(UUID? userId, string groupName, RLVRestrictionType normalType, RLVRestrictionType? secureType, RLVRestrictionType? fromToType)
        {
            // Explicit restrictions
            if (fromToType != null)
            {
                var isRestrictedBySendImTo = _restrictionProvider.GetRestrictions(fromToType.Value)
                    .Where(n => n.Args.Count == 1 &&
                        (userId != null && n.Args[0] is UUID restrictedId && userId == restrictedId) ||
                        (groupName != null && n.Args[0] is string restrictedGroupName && (restrictedGroupName == "allgroups" || restrictedGroupName == groupName))
                    ).Any();
                if (isRestrictedBySendImTo)
                {
                    return false;
                }
            }

            var sendImRestrictions = _restrictionProvider.GetRestrictions(normalType);
            var sendImExceptions = sendImRestrictions
                .Where(n => n.IsException && n.Args.Count == 1 &&
                    (userId != null && n.Args[0] is UUID restrictedId && userId == restrictedId) ||
                    (groupName != null && n.Args[0] is string restrictedGroupName && (restrictedGroupName == "allgroups" || restrictedGroupName == groupName))
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
            var permissiveMode = false;
            foreach (var restriction in sendImRestrictions.Where(n => !n.IsException && n.Args.Count == 0))
            {
                var hasException = sendImExceptions
                    .Where(n => !permissiveMode || n.Sender == restriction.Sender)
                    .Any();
                if (hasException)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public bool StartIM(UUID? userId)
        {
            return CheckSecureRestriction(userId, null, RLVRestrictionType.StartIm, null, RLVRestrictionType.StartImTo);
        }

        public bool CanSendIM(string message, UUID? userId, string groupName)
        {
            return CheckSecureRestriction(userId, groupName, RLVRestrictionType.SendIm, RLVRestrictionType.SendImSec, RLVRestrictionType.SendImTo);
        }

        public bool CanReceiveIM(string message, UUID? userId, string groupName)
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

        public bool IsAutoAcceptTp(UUID userId)
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

        public bool IsAutoAcceptTpRequest(UUID userId)
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
                if (restriction.Args.Count == 1 && restriction.Args[0] is UUID restrictionTexture)
                {
                    textureUUID = restrictionTexture;
                    return true;
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
        public bool CanEdit(UUID objectId, ObjectLocation objectLocation)
        {
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
    }
}
