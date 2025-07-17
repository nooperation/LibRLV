using OpenMetaverse;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LibRLV
{
    public class RLVRestriction
    {
        public RLVRestriction(RLVRestrictionType behavior, bool isException, UUID sender, string senderName, ICollection<object> args)
        {
            this.Behavior = behavior;
            this.IsException = isException;
            this.Sender = sender;
            this.SenderName = senderName;
            this.Args = args.ToImmutableList();
        }

        public RLVRestrictionType Behavior { get; }
        public bool IsException { get; set; }
        public UUID Sender { get; }
        public string SenderName { get; }
        public ImmutableList<object> Args { get; }

        public bool Validate()
        {
            return Validate(this);
        }

        public static bool Validate(RLVRestriction newCommand)
        {
            switch (newCommand.Behavior)
            {
                case RLVRestrictionType.Notify:
                    //[int] || [int, string]
                    return (newCommand.Args.Count == 1 && newCommand.Args[0] is int) ||
                           (newCommand.Args.Count == 2 && newCommand.Args[0] is int && newCommand.Args[1] is string);

                case RLVRestrictionType.CamZoomMax:
                case RLVRestrictionType.CamZoomMin:
                case RLVRestrictionType.SetCamFovMin:
                case RLVRestrictionType.SetCamFovMax:
                case RLVRestrictionType.CamDistMax:
                case RLVRestrictionType.SetCamAvDistMax:
                case RLVRestrictionType.CamDistMin:
                case RLVRestrictionType.SetCamAvDistMin:
                case RLVRestrictionType.CamDrawAlphaMax:
                case RLVRestrictionType.CamAvDist:
                    // [float]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is float;

                case RLVRestrictionType.SitTp:
                case RLVRestrictionType.FarTouch:
                case RLVRestrictionType.TouchFar:
                case RLVRestrictionType.TpLocal:
                    // [] || [float]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args[0] is float);

                case RLVRestrictionType.CamDrawColor:
                    // [float, float, float]
                    return newCommand.Args.Count == 3 && newCommand.Args.All(n => n is float);

                case RLVRestrictionType.RedirChat:
                case RLVRestrictionType.RedirEmote:
                case RLVRestrictionType.SendChannelExcept:
                    // [int]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is int;

                case RLVRestrictionType.SendChannel:
                case RLVRestrictionType.SendChannelSec:
                    // [] || [int]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args[0] is int);

                case RLVRestrictionType.SendImTo:
                case RLVRestrictionType.RecvImFrom:
                    // [UUID | string]
                    return newCommand.Args.Count == 1 && newCommand.Args.All(n => n is UUID || n is string);

                case RLVRestrictionType.SendIm:
                case RLVRestrictionType.RecvIm:
                    // [] || [UUID | string]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args.All(n => n is UUID || n is string));

                case RLVRestrictionType.Detach:
                    // [AttachmentPoint]
                    return newCommand.Args.Count == 1 && newCommand.Args.All(n => n is AttachmentPoint);

                case RLVRestrictionType.AddAttach:
                case RLVRestrictionType.RemAttach:
                    // [] || [AttachmentPoint]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args.All(n => n is AttachmentPoint));

                case RLVRestrictionType.AddOutfit:
                    // [] || [layer]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args.All(n => n is WearableType));

                case RLVRestrictionType.DetachThis:
                case RLVRestrictionType.DetachAllThis:
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

                case RLVRestrictionType.DetachThisExcept:
                case RLVRestrictionType.DetachAllThisExcept:
                case RLVRestrictionType.AttachThisExcept:
                case RLVRestrictionType.AttachAllThisExcept:
                    // [string]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is string;

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
                case RLVRestrictionType.ShowNameTags: // RLVA adds optional UUID
                                                  // [] || [UUID]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args[0] is UUID);

                case RLVRestrictionType.RecvChatFrom:
                case RLVRestrictionType.RecvEmoteFrom:
                case RLVRestrictionType.StartImTo:
                case RLVRestrictionType.EditObj:
                case RLVRestrictionType.TouchThis:
                case RLVRestrictionType.ShowHoverText:
                    // [UUID]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is UUID;

                case RLVRestrictionType.Permissive:
                case RLVRestrictionType.Fly:
                case RLVRestrictionType.TempRun:
                case RLVRestrictionType.AlwaysRun:
                case RLVRestrictionType.CamUnlock:
                case RLVRestrictionType.SetCamUnlock:
                case RLVRestrictionType.SendChat:
                case RLVRestrictionType.ChatShout:
                case RLVRestrictionType.ChatNormal:
                case RLVRestrictionType.ChatWhisper:
                case RLVRestrictionType.RecvChatSec:
                case RLVRestrictionType.SendGesture:
                case RLVRestrictionType.Emote:
                case RLVRestrictionType.RecvEmoteSec:
                case RLVRestrictionType.SendImSec:
                case RLVRestrictionType.RecvImSec:
                case RLVRestrictionType.TpLm:
                case RLVRestrictionType.TpLoc:
                case RLVRestrictionType.TpLureSec:
                case RLVRestrictionType.StandTp:
                case RLVRestrictionType.TpRequestSec:
                case RLVRestrictionType.ShowInv:
                case RLVRestrictionType.ViewNote:
                case RLVRestrictionType.ViewScript:
                case RLVRestrictionType.ViewTexture:
                case RLVRestrictionType.Rez:
                case RLVRestrictionType.EditWorld:
                case RLVRestrictionType.EditAttach:
                case RLVRestrictionType.ShareSec:
                case RLVRestrictionType.Unsit:
                case RLVRestrictionType.Sit:
                case RLVRestrictionType.DefaultWear:
                case RLVRestrictionType.AcceptPermission:
                case RLVRestrictionType.DenyPermission:
                case RLVRestrictionType.UnsharedWear:
                case RLVRestrictionType.UnsharedUnwear:
                case RLVRestrictionType.SharedWear:
                case RLVRestrictionType.SharedUnwear:
                case RLVRestrictionType.TouchAll:
                case RLVRestrictionType.TouchMe:
                case RLVRestrictionType.TouchAttach:
                case RLVRestrictionType.TouchAttachSelf:
                case RLVRestrictionType.Interact:
                case RLVRestrictionType.ShowWorldMap:
                case RLVRestrictionType.ShowMiniMap:
                case RLVRestrictionType.ShowLoc:
                case RLVRestrictionType.ShowNearby:
                case RLVRestrictionType.ShowHoverTextAll:
                case RLVRestrictionType.ShowHoverTextHud:
                case RLVRestrictionType.ShowHoverTextWorld:
                case RLVRestrictionType.SetGroup:
                case RLVRestrictionType.SetDebug:
                case RLVRestrictionType.SetEnv:
                case RLVRestrictionType.AllowIdle:
                    // []
                    return newCommand.Args.Count == 0;
            }

            return false;
        }
    }
}
