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

        public bool Validate()
        {
            return Validate(this);
        }

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

        public static bool Validate(RLVRestriction newCommand)
        {
            switch (newCommand.Behavior)
            {
                case RLVRestrictionType.Notify:             // INTERNAL
                    //[int] || [int, string]
                    return (newCommand.Args.Count == 1 && newCommand.Args[0] is int) ||
                           (newCommand.Args.Count == 2 && newCommand.Args[0] is int && newCommand.Args[1] is string);

                case RLVRestrictionType.CamZoomMax:         // HasCamZoomMax
                case RLVRestrictionType.CamZoomMin:         // HasCamZoomMin
                case RLVRestrictionType.SetCamFovMin:       // HasSetCamFovMin
                case RLVRestrictionType.SetCamFovMax:       // HasSetCamFovMax
                case RLVRestrictionType.CamDistMax:         // HasCamDistMax
                case RLVRestrictionType.SetCamAvDistMax:    // HasSetCamAvDistMax
                case RLVRestrictionType.CamDistMin:         // HasCamDistMin
                case RLVRestrictionType.SetCamAvDistMin:    // HasSetCamAvDistMin
                case RLVRestrictionType.CamDrawAlphaMax:    // HasCamDrawAlphaMax
                case RLVRestrictionType.CamAvDist:          // HasCamAvDist
                    // [float]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is float;

                case RLVRestrictionType.SitTp:              // CanSitTp
                case RLVRestrictionType.FarTouch:           // CanFarTouch, CanTouch
                case RLVRestrictionType.TouchFar:           // CanTouchFar, CanTouch
                case RLVRestrictionType.TpLocal:            // CanTpLocal
                    // [] || [float]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args[0] is float);

                case RLVRestrictionType.CamDrawColor:       // HasCamDrawColor
                    // [float, float, float]
                    return newCommand.Args.Count == 3 && newCommand.Args.All(n => n is float);

                case RLVRestrictionType.RedirChat:          // IsRedirChat        // TODO: Handle internally automatically
                case RLVRestrictionType.RedirEmote:         // IsRedirEmote       // TODO: Handle internally automatically
                case RLVRestrictionType.SendChannelExcept:  // CanChat
                    // [int]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is int;

                case RLVRestrictionType.SendChannel:        // CanChat
                case RLVRestrictionType.SendChannelSec:     // CanChat
                    // [] || [int]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args[0] is int);

                case RLVRestrictionType.SendImTo:           // CanSendIM
                case RLVRestrictionType.RecvImFrom:         // CanReceiveIM
                    // [UUID | string]
                    return newCommand.Args.Count == 1 && newCommand.Args.All(n => n is UUID || n is string);

                case RLVRestrictionType.SendIm:             // CanSendIM
                case RLVRestrictionType.RecvIm:             // CanReceiveIM
                    // [] || [UUID | string]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args.All(n => n is UUID || n is string));

                case RLVRestrictionType.Detach:             // CanDetach
                    // [AttachmentPoint]
                    return newCommand.Args.Count == 1 && newCommand.Args.All(n => n is AttachmentPoint);

                case RLVRestrictionType.AddAttach:
                case RLVRestrictionType.RemAttach:          // CanDetach
                    // [] || [AttachmentPoint]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args.All(n => n is AttachmentPoint));

                case RLVRestrictionType.AddOutfit:
                case RLVRestrictionType.RemOutfit:
                    // [] || [layer]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args.All(n => n is WearableType));

                case RLVRestrictionType.DetachThis:         // CanDetach
                case RLVRestrictionType.DetachAllThis:      // CanDetach
                case RLVRestrictionType.AttachAllThis:
                    //[] || [uuid | layer | attachpt | string]
                    return newCommand.Args.Count == 0 || (newCommand.Args.Count == 1 && newCommand.Args.All(n =>
                               n is UUID ||
                               n is WearableType ||
                               n is AttachmentPoint ||
                               n is string));

                case RLVRestrictionType.AttachThis:
                    // [uuid | layer | attachpt | string]
                    return newCommand.Args.Count == 1 && newCommand.Args.All(n =>
                               n is UUID ||
                               n is WearableType ||
                               n is AttachmentPoint ||
                               n is string);

                case RLVRestrictionType.DetachThisExcept:   // CanDetach
                case RLVRestrictionType.DetachAllThisExcept:// CanDetach
                case RLVRestrictionType.AttachThisExcept:   // 
                case RLVRestrictionType.AttachAllThisExcept:// 
                    // [string]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is string;

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
                case RLVRestrictionType.ShowNameTags: // RLVA adds optional UUID
                                                      // [] || [UUID]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args[0] is UUID);

                case RLVRestrictionType.RecvChatFrom:       // CanReceiveChat
                case RLVRestrictionType.RecvEmoteFrom:      // CanReceiveChat
                case RLVRestrictionType.StartImTo:          // CanStartIM
                case RLVRestrictionType.EditObj:            // CanEdit
                case RLVRestrictionType.TouchThis:          // CanTouch
                case RLVRestrictionType.ShowHoverText:      // ShowHoverText
                    // [UUID]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is UUID;

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
                    return newCommand.Args.Count == 0;
                default:
                    throw new NotImplementedException();
            }

            return false;
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
