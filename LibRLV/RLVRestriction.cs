using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LibRLV
{
    public class RLVRestriction
    {
        public RLVRestrictionType Behavior { get; }
        public RLVRestrictionType OriginalBehavior { get; }
        public bool IsException => IsRestrictionAnException(this);
        public Guid Sender { get; }
        public string SenderName { get; }
        public ImmutableList<object> Args { get; }

        public RLVRestriction(RLVRestrictionType behavior, Guid sender, string senderName, ICollection<object> args)
        {
            Behavior = GetRealRestriction(behavior);
            OriginalBehavior = behavior;
            Sender = sender;
            SenderName = senderName;
            Args = args.ToImmutableList();
        }

        internal static RLVRestrictionType GetRealRestriction(RLVRestrictionType restrictionType)
        {
            switch (restrictionType)
            {
                case RLVRestrictionType.CamDistMax:
                    return RLVRestrictionType.SetCamAvDistMax;
                case RLVRestrictionType.CamDistMin:
                    return RLVRestrictionType.SetCamAvDistMin;
                case RLVRestrictionType.CamUnlock:
                    return RLVRestrictionType.SetCamUnlock;
                case RLVRestrictionType.CamTextures:
                    return RLVRestrictionType.SetCamTextures;
                case RLVRestrictionType.FarTouch:
                    return RLVRestrictionType.TouchFar;
            }

            return restrictionType;
        }

        private static bool IsRestrictionAnException(RLVRestriction restriction)
        {
            switch (restriction.Behavior)
            {
                case RLVRestrictionType.RecvEmote:
                case RLVRestrictionType.RecvChat:
                case RLVRestrictionType.SendIm:
                case RLVRestrictionType.StartIm:
                case RLVRestrictionType.RecvIm:
                case RLVRestrictionType.SendChannel:
                case RLVRestrictionType.TpRequest:
                case RLVRestrictionType.TpLure:
                case RLVRestrictionType.Edit:
                case RLVRestrictionType.Share:
                case RLVRestrictionType.TouchWorld:
                case RLVRestrictionType.ShowNamesSec:
                case RLVRestrictionType.ShowNames:
                case RLVRestrictionType.ShowNameTags:
                case RLVRestrictionType.AcceptTp:
                case RLVRestrictionType.AcceptTpRequest:
                    return restriction.Args.Count > 0;

                case RLVRestrictionType.DetachThisExcept:
                case RLVRestrictionType.DetachAllThisExcept:
                case RLVRestrictionType.AttachThisExcept:
                case RLVRestrictionType.AttachAllThisExcept:
                    return true;
            }

            return false;
        }

        internal static bool ParseOptions(RLVRestrictionType behavior, string options, out List<object> parsedArgs)
        {
            parsedArgs = new List<object>();
            var args = options.Split([';'], StringSplitOptions.RemoveEmptyEntries);

            switch (behavior)
            {
                case RLVRestrictionType.Notify:
                {
                    if (args.Length < 1 || !int.TryParse(args[0], out var channel))
                    {
                        return false;
                    }
                    parsedArgs.Add(channel);

                    if (args.Length == 2)
                    {
                        parsedArgs.Add(args[1]);
                    }

                    return true;
                }

                case RLVRestrictionType.CamDrawMin:
                case RLVRestrictionType.CamDrawMax:
                {
                    if (args.Length < 1 || !float.TryParse(args[0], out var val))
                    {
                        return false;
                    }

                    if (val < 0.40f)
                    {
                        return false;
                    }

                    parsedArgs.Add(val);

                    return true;
                }

                case RLVRestrictionType.CamZoomMax:
                case RLVRestrictionType.CamZoomMin:
                case RLVRestrictionType.SetCamFovMin:
                case RLVRestrictionType.SetCamFovMax:
                case RLVRestrictionType.CamDistMax:
                case RLVRestrictionType.SetCamAvDistMax:
                case RLVRestrictionType.CamDistMin:
                case RLVRestrictionType.SetCamAvDistMin:
                case RLVRestrictionType.CamDrawAlphaMin:
                case RLVRestrictionType.CamDrawAlphaMax:
                case RLVRestrictionType.CamAvDist:
                {
                    if (args.Length < 1 || !float.TryParse(args[0], out var val))
                    {
                        return false;
                    }
                    parsedArgs.Add(val);

                    return true;
                }

                case RLVRestrictionType.SitTp:
                case RLVRestrictionType.FarTouch:
                case RLVRestrictionType.TouchFar:
                case RLVRestrictionType.TpLocal:
                {
                    if (args.Length == 0)
                    {
                        return true;
                    }

                    if (args.Length != 1 || !float.TryParse(args[0], out var val))
                    {
                        return false;
                    }

                    parsedArgs.Add(val);
                    return true;
                }

                case RLVRestrictionType.CamDrawColor:
                {
                    if (args.Length != 3)
                    {
                        return false;
                    }

                    foreach (var arg in args)
                    {
                        if (!float.TryParse(arg, out var val))
                        {
                            return false;
                        }

                        parsedArgs.Add(val);
                    }
                    return true;
                }

                case RLVRestrictionType.RedirChat:
                case RLVRestrictionType.RedirEmote:
                case RLVRestrictionType.SendChannelExcept:
                {
                    if (args.Length != 1 || !int.TryParse(args[0], out var val))
                    {
                        return false;
                    }

                    parsedArgs.Add(val);
                    return true;
                }

                case RLVRestrictionType.SendChannel:
                case RLVRestrictionType.SendChannelSec:
                {
                    if (args.Length == 0)
                    {
                        return true;
                    }

                    if (args.Length != 1 || !int.TryParse(args[0], out var val))
                    {
                        return false;
                    }

                    parsedArgs.Add(val);
                    return true;
                }

                case RLVRestrictionType.SendImTo:
                case RLVRestrictionType.RecvImFrom:
                {
                    // [Guid | string]
                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (Guid.TryParse(args[0], out var val))
                    {
                        parsedArgs.Add(val);
                    }
                    else
                    {
                        parsedArgs.Add(args[0]);
                    }

                    return true;
                }

                case RLVRestrictionType.SendIm:
                case RLVRestrictionType.RecvIm:
                {
                    // [] | [Guid | string]
                    if (args.Length == 0)
                    {
                        return true;
                    }

                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (Guid.TryParse(args[0], out var val))
                    {
                        parsedArgs.Add(val);
                    }
                    else
                    {
                        parsedArgs.Add(args[0]);
                    }

                    return true;
                }

                case RLVRestrictionType.Detach:
                {
                    // [] | [AttachmentPoint]
                    if (args.Length == 0)
                    {
                        return true;
                    }

                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (!RLVCommon.RLVAttachmentPointMap.TryGetValue(args[0], out var val))
                    {
                        return false;
                    }

                    parsedArgs.Add(val);
                    return true;
                }

                case RLVRestrictionType.AddAttach:
                case RLVRestrictionType.RemAttach:
                {
                    // [] | [AttachmentPoint]
                    if (args.Length == 0)
                    {
                        return true;
                    }

                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (!RLVCommon.RLVAttachmentPointMap.TryGetValue(args[0], out var val))
                    {
                        return false;
                    }

                    parsedArgs.Add(val);
                    return true;
                }

                case RLVRestrictionType.AddOutfit:
                case RLVRestrictionType.RemOutfit:
                {
                    // [] || [layer]
                    if (args.Length == 0)
                    {
                        return true;
                    }

                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (!RLVCommon.RLVWearableTypeMap.TryGetValue(args[0], out var val))
                    {
                        return false;
                    }

                    parsedArgs.Add(val);
                    return true;
                }


                case RLVRestrictionType.DetachThis:
                case RLVRestrictionType.DetachAllThis:
                case RLVRestrictionType.AttachThis:
                case RLVRestrictionType.AttachAllThis:
                {
                    // [] || [layer | attachpt | string]
                    if (args.Length == 0)
                    {
                        return true;
                    }

                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (RLVCommon.RLVWearableTypeMap.TryGetValue(args[0], out var wearableType))
                    {
                        parsedArgs.Add(wearableType);
                        return true;
                    }
                    else if (RLVCommon.RLVAttachmentPointMap.TryGetValue(args[0], out var attachmentPoint))
                    {
                        parsedArgs.Add(attachmentPoint);
                        return true;
                    }

                    parsedArgs.Add(args[0]);
                    return true;
                }

                case RLVRestrictionType.DetachThisExcept:
                case RLVRestrictionType.DetachAllThisExcept:
                case RLVRestrictionType.AttachThisExcept:
                case RLVRestrictionType.AttachAllThisExcept:
                {
                    // [string]
                    if (args.Length != 1)
                    {
                        return false;
                    }

                    parsedArgs.Add(args[0]);
                    return true;
                }

                case RLVRestrictionType.CamTextures:
                case RLVRestrictionType.SetCamTextures:
                case RLVRestrictionType.RecvChat:
                case RLVRestrictionType.RecvEmote:
                case RLVRestrictionType.StartIm:
                case RLVRestrictionType.TpLure:
                case RLVRestrictionType.AcceptTp:
                case RLVRestrictionType.AcceptTpRequest:
                case RLVRestrictionType.TpRequest:
                case RLVRestrictionType.Edit:
                case RLVRestrictionType.Share:
                case RLVRestrictionType.TouchWorld:
                case RLVRestrictionType.TouchAttachOther:
                case RLVRestrictionType.TouchHud:
                case RLVRestrictionType.ShowNames:
                case RLVRestrictionType.ShowNamesSec:
                case RLVRestrictionType.ShowNameTags:
                {
                    // [] [Guid]
                    if (args.Length == 0)
                    {
                        return true;
                    }

                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (!Guid.TryParse(args[0], out var uuid))
                    {
                        return false;
                    }

                    parsedArgs.Add(uuid);
                    return true;
                }

                case RLVRestrictionType.RecvChatFrom:
                case RLVRestrictionType.RecvEmoteFrom:
                case RLVRestrictionType.StartImTo:
                case RLVRestrictionType.EditObj:
                case RLVRestrictionType.TouchThis:
                case RLVRestrictionType.ShowHoverText:
                {
                    // [Guid]
                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (!Guid.TryParse(args[0], out var uuid))
                    {
                        return false;
                    }

                    parsedArgs.Add(uuid);
                    return true;
                }

                case RLVRestrictionType.Permissive:
                case RLVRestrictionType.SendChat:
                case RLVRestrictionType.ChatShout:
                case RLVRestrictionType.ChatNormal:
                case RLVRestrictionType.ChatWhisper:
                case RLVRestrictionType.Emote:
                case RLVRestrictionType.RecvChatSec:
                case RLVRestrictionType.RecvEmoteSec:
                case RLVRestrictionType.SendGesture:
                case RLVRestrictionType.SendImSec:
                case RLVRestrictionType.RecvImSec:
                case RLVRestrictionType.TpLureSec:
                case RLVRestrictionType.TpRequestSec:
                case RLVRestrictionType.ShareSec:
                case RLVRestrictionType.Fly:
                case RLVRestrictionType.Jump:
                case RLVRestrictionType.TempRun:
                case RLVRestrictionType.AlwaysRun:
                case RLVRestrictionType.CamUnlock:
                case RLVRestrictionType.SetCamUnlock:
                case RLVRestrictionType.TpLm:
                case RLVRestrictionType.TpLoc:
                case RLVRestrictionType.StandTp:
                case RLVRestrictionType.ShowInv:
                case RLVRestrictionType.ViewNote:
                case RLVRestrictionType.ViewScript:
                case RLVRestrictionType.ViewTexture:
                case RLVRestrictionType.Unsit:
                case RLVRestrictionType.Sit:
                case RLVRestrictionType.DefaultWear:
                case RLVRestrictionType.SetGroup:
                case RLVRestrictionType.SetDebug:
                case RLVRestrictionType.SetEnv:
                case RLVRestrictionType.AllowIdle:
                case RLVRestrictionType.ShowWorldMap:
                case RLVRestrictionType.ShowMiniMap:
                case RLVRestrictionType.ShowLoc:
                case RLVRestrictionType.ShowNearby:
                case RLVRestrictionType.EditWorld:
                case RLVRestrictionType.EditAttach:
                case RLVRestrictionType.Rez:
                case RLVRestrictionType.DenyPermission:
                case RLVRestrictionType.AcceptPermission:
                case RLVRestrictionType.UnsharedWear:
                case RLVRestrictionType.UnsharedUnwear:
                case RLVRestrictionType.SharedWear:
                case RLVRestrictionType.SharedUnwear:
                case RLVRestrictionType.TouchAll:
                case RLVRestrictionType.TouchMe:
                case RLVRestrictionType.TouchAttach:
                case RLVRestrictionType.TouchAttachSelf:
                case RLVRestrictionType.Interact:
                case RLVRestrictionType.ShowHoverTextAll:
                case RLVRestrictionType.ShowHoverTextHud:
                case RLVRestrictionType.ShowHoverTextWorld:
                    // []
                    return args.Length == 0;
                default:
                    throw new NotImplementedException();
            }
        }

        public override bool Equals(object obj)
        {
            return obj is RLVRestriction restriction &&
                   Behavior == restriction.Behavior &&
                   Sender.Equals(restriction.Sender) &&
                   Args.SequenceEqual(restriction.Args);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Behavior);
            hashCode.Add(Sender);
            foreach (var item in Args)
            {
                hashCode.Add(item);
            }

            return hashCode.ToHashCode();
        }
    }
}
