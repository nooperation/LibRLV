using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LibRLV
{
    public class RLVRestriction
    {
        public RLVRestriction(RLVRestrictionType behavior, UUID sender, string senderName, ICollection<object> args)
        {
            this.Behavior = GetRealRestriction(behavior);
            this.OriginalBehavior = behavior;
            this.Sender = sender;
            this.SenderName = senderName;
            this.Args = args.ToImmutableList();
        }

        public RLVRestrictionType Behavior { get; }
        public RLVRestrictionType OriginalBehavior { get; }
        public bool IsException => IsRestrictionAnException(this);
        public UUID Sender { get; }
        public string SenderName { get; }
        public ImmutableList<object> Args { get; }

        public static RLVRestrictionType GetRealRestriction(RLVRestrictionType restrictionType)
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

        public static bool IsRestrictionAnException(RLVRestriction restriction)
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

        public static bool ParseOptions(RLVRestrictionType behavior, string options, out List<object> parsedArgs)
        {
            parsedArgs = new List<object>();
            var args = options.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            switch (behavior)
            {
                case RLVRestrictionType.Notify:             // INTERNAL
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

                case RLVRestrictionType.CamDrawMin:         // HasCamDrawMin
                case RLVRestrictionType.CamDrawMax:         // HasCamDrawMax
                {
                    if (args.Length < 1 || !float.TryParse(args[0], out var val))
                    {
                        return false;
                    }

                    if(val < 0.40f)
                    {
                        return false;
                    }

                    parsedArgs.Add(val);

                    return true;
                }

                case RLVRestrictionType.CamZoomMax:         // HasCamZoomMax
                case RLVRestrictionType.CamZoomMin:         // HasCamZoomMin
                case RLVRestrictionType.SetCamFovMin:       // HasSetCamFovMin
                case RLVRestrictionType.SetCamFovMax:       // HasSetCamFovMax
                case RLVRestrictionType.CamDistMax:         // HasCamDistMax
                case RLVRestrictionType.SetCamAvDistMax:    // HasSetCamAvDistMax
                case RLVRestrictionType.CamDistMin:         // HasCamDistMin
                case RLVRestrictionType.SetCamAvDistMin:    // HasSetCamAvDistMin
                case RLVRestrictionType.CamDrawAlphaMin:    // HasCamDrawAlphaMin
                case RLVRestrictionType.CamDrawAlphaMax:    // HasCamDrawAlphaMax
                case RLVRestrictionType.CamAvDist:          // HasCamAvDist
                {
                    if (args.Length < 1 || !float.TryParse(args[0], out var val))
                    {
                        return false;
                    }
                    parsedArgs.Add(val);

                    return true;
                }

                case RLVRestrictionType.SitTp:              // CanSitTp
                case RLVRestrictionType.FarTouch:           // CanFarTouch, CanTouch
                case RLVRestrictionType.TouchFar:           // CanTouchFar, CanTouch
                case RLVRestrictionType.TpLocal:            // CanTpLocal
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

                case RLVRestrictionType.CamDrawColor:       // HasCamDrawColor
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

                case RLVRestrictionType.RedirChat:          // IsRedirChat        // TODO: Handle internally automatically
                case RLVRestrictionType.RedirEmote:         // IsRedirEmote       // TODO: Handle internally automatically
                case RLVRestrictionType.SendChannelExcept:  // HasSendChannelExceptions - CanChat
                {
                    if (args.Length != 1 || !int.TryParse(args[0], out var val))
                    {
                        return false;
                    }

                    parsedArgs.Add(val);
                    return true;
                }

                case RLVRestrictionType.SendChannel:        // CanChat
                case RLVRestrictionType.SendChannelSec:     // CanChat
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

                case RLVRestrictionType.SendImTo:           // CanSendIM
                case RLVRestrictionType.RecvImFrom:         // CanReceiveIM
                {
                    // [UUID | string]
                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (UUID.TryParse(args[0], out var val))
                    {
                        parsedArgs.Add(val);
                    }
                    else
                    {
                        parsedArgs.Add(args[0]);
                    }

                    return true;
                }

                case RLVRestrictionType.SendIm:             // CanSendIM
                case RLVRestrictionType.RecvIm:             // CanReceiveIM
                {
                    // [] | [UUID | string]
                    if (args.Length == 0)
                    {
                        return true;
                    }

                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (UUID.TryParse(args[0], out var val))
                    {
                        parsedArgs.Add(val);
                    }
                    else
                    {
                        parsedArgs.Add(args[0]);
                    }

                    return true;
                }

                case RLVRestrictionType.Detach:             // CanDetach
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

                    if (!Enum.TryParse<AttachmentPoint>(args[0], true, out var val))
                    {
                        return false;
                    }

                    parsedArgs.Add(val);
                    return true;
                }

                case RLVRestrictionType.AddAttach:
                case RLVRestrictionType.RemAttach:          // CanDetach
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

                    if (!Enum.TryParse<AttachmentPoint>(args[0], true, out var val))
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

                    if (!Enum.TryParse<WearableType>(args[0], true, out var val))
                    {
                        return false;
                    }

                    parsedArgs.Add(val);
                    return true;
                }


                case RLVRestrictionType.DetachThis:         // CanDetach
                case RLVRestrictionType.DetachAllThis:      // CanDetach
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

                    if (Enum.TryParse<WearableType>(args[0], true, out var wearableType))
                    {
                        parsedArgs.Add(wearableType);
                        return true;
                    }
                    else if (Enum.TryParse<AttachmentPoint>(args[0], true, out var attachmentPoint))
                    {
                        parsedArgs.Add(attachmentPoint);
                        return true;
                    }

                    parsedArgs.Add(args[0]);
                    return true;
                }

                case RLVRestrictionType.DetachThisExcept:   // CanDetach
                case RLVRestrictionType.DetachAllThisExcept:// CanDetach
                case RLVRestrictionType.AttachThisExcept:   // 
                case RLVRestrictionType.AttachAllThisExcept:// 
                {
                    // [string]
                    if (args.Length != 1)
                    {
                        return false;
                    }

                    parsedArgs.Add(args[0]);
                    return true;
                }

                case RLVRestrictionType.CamTextures:        // HasSetCamtextures
                case RLVRestrictionType.SetCamTextures:     // HasSetCamtextures
                case RLVRestrictionType.RecvChat:           // CanReceiveChat
                case RLVRestrictionType.RecvEmote:          // CanReceiveChat
                case RLVRestrictionType.StartIm:            // CanStartIM
                case RLVRestrictionType.TpLure:             // CanTPLure
                case RLVRestrictionType.AcceptTp:           // IsAutoAcceptTp
                case RLVRestrictionType.AcceptTpRequest:    // IsAutoAcceptTpRequest
                case RLVRestrictionType.TpRequest:          // CanTpRequest
                case RLVRestrictionType.Edit:               // CanEdit
                case RLVRestrictionType.Share:              // CanShare
                case RLVRestrictionType.TouchWorld:         // CanTouch
                case RLVRestrictionType.TouchAttachOther:   // CanTouch
                case RLVRestrictionType.TouchHud:           // CanTouchHud
                case RLVRestrictionType.ShowNames:          // CanShowNames
                case RLVRestrictionType.ShowNamesSec:       // CanShowNames
                case RLVRestrictionType.ShowNameTags:       // CanShowNameTags
                {
                    // [] [UUID]
                    if (args.Length == 0)
                    {
                        return true;
                    }

                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (!UUID.TryParse(args[0], out UUID uuid))
                    {
                        return false;
                    }

                    parsedArgs.Add(uuid);
                    return true;
                }

                case RLVRestrictionType.RecvChatFrom:       // CanReceiveChat
                case RLVRestrictionType.RecvEmoteFrom:      // CanReceiveChat
                case RLVRestrictionType.StartImTo:          // CanStartIM
                case RLVRestrictionType.EditObj:            // CanEdit
                case RLVRestrictionType.TouchThis:          // CanTouch
                case RLVRestrictionType.ShowHoverText:      // ShowHoverText
                {
                    // [UUID]
                    if (args.Length != 1)
                    {
                        return false;
                    }

                    if (!UUID.TryParse(args[0], out UUID uuid))
                    {
                        return false;
                    }

                    parsedArgs.Add(uuid);
                    return true;
                }

                case RLVRestrictionType.Permissive:         // IsPermissive
                case RLVRestrictionType.SendChat:           // CanChat
                case RLVRestrictionType.ChatShout:          // CanChat
                case RLVRestrictionType.ChatNormal:         // CanChat
                case RLVRestrictionType.ChatWhisper:        // CanChat
                case RLVRestrictionType.Emote:              // CanChat
                case RLVRestrictionType.RecvChatSec:        // CanReceiveChat
                case RLVRestrictionType.RecvEmoteSec:       // CanReceiveChat
                case RLVRestrictionType.SendGesture:        // CanSendGesture
                case RLVRestrictionType.SendImSec:          // CanSendIM
                case RLVRestrictionType.RecvImSec:          // CanReceiveIM
                case RLVRestrictionType.TpLureSec:          // CanTPLure
                case RLVRestrictionType.TpRequestSec:       // CanTpRequest
                case RLVRestrictionType.ShareSec:           // CanShare
                case RLVRestrictionType.Fly:                // CanFly
                case RLVRestrictionType.TempRun:            // CanTempRun
                case RLVRestrictionType.AlwaysRun:          // CanAlwaysRun
                case RLVRestrictionType.CamUnlock:          // CanCamUnlock
                case RLVRestrictionType.SetCamUnlock:       // CanCamUnlock
                case RLVRestrictionType.TpLm:               // CanTpLm
                case RLVRestrictionType.TpLoc:              // CanTpLoc
                case RLVRestrictionType.StandTp:            // CanStandTp
                case RLVRestrictionType.ShowInv:            // CanShowInv
                case RLVRestrictionType.ViewNote:           // CanViewNote
                case RLVRestrictionType.ViewScript:         // CanViewScript
                case RLVRestrictionType.ViewTexture:        // CanViewTexture
                case RLVRestrictionType.Unsit:              // CanUnsit
                case RLVRestrictionType.Sit:                // CanSit
                case RLVRestrictionType.DefaultWear:        // CanDefaultWear
                case RLVRestrictionType.SetGroup:           // CanSetGroup
                case RLVRestrictionType.SetDebug:           // CanSetDebug
                case RLVRestrictionType.SetEnv:             // CanSetEnv
                case RLVRestrictionType.AllowIdle:          // CanAllowIdle
                case RLVRestrictionType.ShowWorldMap:       // CanShowWorldMap
                case RLVRestrictionType.ShowMiniMap:        // CanShowMiniMap
                case RLVRestrictionType.ShowLoc:            // CanShowLoc
                case RLVRestrictionType.ShowNearby:         // CanShowNearby
                case RLVRestrictionType.EditWorld:          // CanEdit
                case RLVRestrictionType.EditAttach:         // CanEdit
                case RLVRestrictionType.Rez:                // CanRez
                case RLVRestrictionType.DenyPermission:     // IsAutoDenyPermissions, IsAutoAcceptPermissions
                case RLVRestrictionType.AcceptPermission:   // IsAutoAcceptPermissions
                case RLVRestrictionType.UnsharedWear:       // CanUnsharedWear, CanAttach?
                case RLVRestrictionType.UnsharedUnwear:     // CanUnsharedUnwear, CanDetach?
                case RLVRestrictionType.SharedWear:         // CanSharedWear, CanAttach?
                case RLVRestrictionType.SharedUnwear:       // CanSharedUnwear, CanDetach?
                case RLVRestrictionType.TouchAll:           // CanTouch
                case RLVRestrictionType.TouchMe:            // CanTouch
                case RLVRestrictionType.TouchAttach:        // CanTouch
                case RLVRestrictionType.TouchAttachSelf:    // CanTouch
                case RLVRestrictionType.Interact:           // MULTIPLE - CanTouch, CanEdit, CanRez
                case RLVRestrictionType.ShowHoverTextAll:   // ShowHoverText
                case RLVRestrictionType.ShowHoverTextHud:   // ShowHoverText
                case RLVRestrictionType.ShowHoverTextWorld: // ShowHoverText
                    // []
                    return args.Length == 0;
                default:
                    throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return $"[{SenderName}] {Behavior}{(Args.Count == 0 ? "" : ":")}{string.Join(",", Args)}";
        }

        public override bool Equals(object obj)
        {
            return obj is RLVRestriction restriction &&
                   this.Behavior == restriction.Behavior &&
                   this.Sender.Equals(restriction.Sender) &&
                   Args.SequenceEqual(restriction.Args);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.Behavior);
            hashCode.Add(this.Sender);
            foreach (var item in Args)
            {
                hashCode.Add(item);
            }

            return hashCode.ToHashCode();
        }
    }
}
