using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace LibRLV
{
    public static class RLVCommon
    {
        internal static readonly ImmutableDictionary<string, WearableType> RLVWearableTypeMap = new Dictionary<string, WearableType>()
        {
            {"gloves", WearableType.Gloves},
            {"jacket", WearableType.Jacket},
            {"pants", WearableType.Pants},
            {"shirt", WearableType.Shirt},
            {"shoes", WearableType.Shoes},
            {"skirt", WearableType.Skirt},
            {"socks", WearableType.Socks},
            {"underpants", WearableType.Underpants},
            {"undershirt", WearableType.Undershirt},
            {"skin", WearableType.Skin},
            {"eyes", WearableType.Eyes},
            {"hair", WearableType.Hair},
            {"shape", WearableType.Shape},
            {"alpha", WearableType.Alpha},
            {"tattoo", WearableType.Tattoo},
            {"physics", WearableType.Physics },
        }.ToImmutableDictionary();

        internal static readonly ImmutableDictionary<string, AttachmentPoint> RLVAttachmentPointMap = new Dictionary<string, AttachmentPoint>()
        {
            {"none", AttachmentPoint.Default},
            {"chest", AttachmentPoint.Chest },
            {"skull", AttachmentPoint.Skull},
            {"left shoulder", AttachmentPoint.LeftShoulder},
            {"right shoulder", AttachmentPoint.RightShoulder},
            {"left hand", AttachmentPoint.LeftHand},
            {"right hand", AttachmentPoint.RightHand},
            {"left foot", AttachmentPoint.LeftFoot},
            {"right foot", AttachmentPoint.RightFoot},
            {"spine", AttachmentPoint.Spine},
            {"pelvis", AttachmentPoint.Pelvis},
            {"mouth", AttachmentPoint.Mouth},
            {"chin", AttachmentPoint.Chin},
            {"left ear", AttachmentPoint.LeftEar},
            {"right ear", AttachmentPoint.RightEar},
            {"left eyeball", AttachmentPoint.LeftEyeball},
            {"right eyeball", AttachmentPoint.RightEyeball},
            {"nose", AttachmentPoint.Nose},
            {"r upper arm", AttachmentPoint.RightUpperArm},
            {"r forearm", AttachmentPoint.RightForearm},
            {"l upper arm", AttachmentPoint.LeftUpperArm},
            {"l forearm", AttachmentPoint.LeftForearm},
            {"right hip", AttachmentPoint.RightHip},
            {"r upper leg", AttachmentPoint.RightUpperLeg},
            {"r lower leg", AttachmentPoint.RightLowerLeg},
            {"left hip", AttachmentPoint.LeftHip},
            {"l upper leg", AttachmentPoint.LeftUpperLeg},
            {"l lower leg", AttachmentPoint.LeftLowerLeg},
            {"stomach", AttachmentPoint.Stomach},
            {"left pec", AttachmentPoint.LeftPec},
            {"right pec", AttachmentPoint.RightPec},
            {"center 2", AttachmentPoint.HUDCenter2},
            {"top right", AttachmentPoint.HUDTopRight},
            {"top", AttachmentPoint.HUDTop},
            {"top left", AttachmentPoint.HUDTopLeft},
            {"center", AttachmentPoint.HUDCenter},
            {"bottom left", AttachmentPoint.HUDBottomLeft},
            {"bottom", AttachmentPoint.HUDBottom},
            {"bottom right", AttachmentPoint.HUDBottomRight},
            {"neck", AttachmentPoint.Neck},
            {"root", AttachmentPoint.Root},
            {"left ring finger", AttachmentPoint.LeftHandRing},
            {"right ring finger", AttachmentPoint.RightHandRing},
            {"tail base", AttachmentPoint.TailBase},
            {"tail tip", AttachmentPoint.TailTip},
            {"left wing", AttachmentPoint.LeftWing},
            {"right wing", AttachmentPoint.RightWing},
            {"jaw", AttachmentPoint.Jaw},
            {"alt left ear", AttachmentPoint.AltLeftEar},
            {"alt right ear", AttachmentPoint.AltRightEar},
            {"alt left eye", AttachmentPoint.AltLeftEye},
            {"alt right eye", AttachmentPoint.AltRightEye},
            {"tongue", AttachmentPoint.Tongue},
            {"groin", AttachmentPoint.Groin},
            {"left hind foot", AttachmentPoint.LeftHindFoot},
            {"right hind foot", AttachmentPoint.RightHindFoot},
        }.ToImmutableDictionary();

        public static List<object> ParseOptions(string options)
        {
            var result = new List<object>();
            var args = options.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var arg in args)
            {
                if (UUID.TryParse(arg, out var id))
                {
                    result.Add(id);
                    continue;
                }
                else if (int.TryParse(arg, out var intValue))
                {
                    result.Add(intValue);
                    continue;
                }
                else if (float.TryParse(arg, out var floatValue))
                {
                    result.Add(floatValue);
                    continue;
                }
                else if (RLVWearableTypeMap.TryGetValue(arg, out WearableType part) && part != WearableType.Invalid)
                {
                    result.Add(part);
                    continue;
                }
                else if (RLVAttachmentPointMap.TryGetValue(arg, out AttachmentPoint attachmentPoint))
                {
                    result.Add(attachmentPoint);
                    continue;
                }
                else
                {
                    result.Add(arg);
                }
            }

            return result;
        }
    }
}
