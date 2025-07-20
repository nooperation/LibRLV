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
            Shout = 2,
            StartTyping = 4,
            StopTyping = 5
        }
        public bool CanChat(int channel, string message, ChatType chatType)
        {
            if (channel == 0)
            {
                var canSendChat = _restrictionProvider.GetRestrictions(RLVRestrictionType.SendChat).Count != 0;
                if (!canSendChat)
                {
                    return false;
                }

                var canChatShout = _restrictionProvider.GetRestrictions(RLVRestrictionType.ChatShout).Count != 0;
                var canChatWhisper = _restrictionProvider.GetRestrictions(RLVRestrictionType.ChatWhisper).Count != 0;
                var canChatNormal = _restrictionProvider.GetRestrictions(RLVRestrictionType.ChatNormal).Count != 0;

                if (chatType == ChatType.Whisper && !canChatWhisper)
                {
                    return false;
                }
                else if (chatType == ChatType.Shout && !canChatShout)
                {
                    return false;
                }
                else if (chatType == ChatType.Normal && !canChatNormal)
                {
                    return false;
                }
            }
            else
            {
                var sendChannelExceptRestrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.SendChannelExcept);
                var sendChannelRestrictions = _restrictionProvider.GetRestrictions(RLVRestrictionType.SendChannel);

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

                // @sendchannel
                var sendChannelExceptions = sendChannelRestrictions
                    .Where(n => n.IsException)
                    .ToList();
                foreach (var restriction in sendChannelRestrictions)
                {
                    if (restriction.Args.Count == 0)
                    {
                        return false;
                    }

                    if (restriction.Args[0] is int restrictedChannel)
                    {
                        if (restrictedChannel == channel)
                        {
                            return true;
                        }
                    }
                }
            }

            return true;
        }
    }
}
