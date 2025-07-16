using OpenMetaverse;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LibRLV
{
    public class RLVRule
    {
        public RLVRule(RLVRestriction behavior, bool isException, UUID sender, string senderName, ICollection<object> args)
        {
            this.Behavior = behavior;
            this.IsException = isException;
            this.Sender = sender;
            this.SenderName = senderName;
            this.Args = args.ToImmutableList();
        }

        public RLVRestriction Behavior { get; }
        public bool IsException { get; set; }
        public UUID Sender { get; }
        public string SenderName { get; }
        public ImmutableList<object> Args { get; }

        public bool Validate()
        {
            return Validate(this);
        }

        public static bool Validate(RLVRule newCommand)
        {
            switch (newCommand.Behavior)
            {
                case RLVRestriction.Notify:
                    //[int] || [int, string]
                    return (newCommand.Args.Count == 1 && newCommand.Args[0] is int) ||
                           (newCommand.Args.Count == 2 && newCommand.Args[0] is int && newCommand.Args[1] is string);

                case RLVRestriction.CamZoomMax:
                case RLVRestriction.CamZoomMin:
                case RLVRestriction.SetCamFovMin:
                case RLVRestriction.SetCamFovMax:
                case RLVRestriction.CamDistMax:
                case RLVRestriction.SetCamAvDistMax:
                case RLVRestriction.CamDistMin:
                case RLVRestriction.SetCamAvDistMin:
                case RLVRestriction.CamDrawAlphaMax:
                case RLVRestriction.CamAvDist:
                    // [float]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is float;

                case RLVRestriction.SitTp:
                case RLVRestriction.FarTouch:
                case RLVRestriction.TouchFar:
                case RLVRestriction.TpLocal:
                    // [] || [float]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args[0] is float);

                case RLVRestriction.CamDrawColor:
                    // [float, float, float]
                    return newCommand.Args.Count == 3 && newCommand.Args.All(n => n is float);

                case RLVRestriction.RedirChat:
                case RLVRestriction.RedirEmote:
                case RLVRestriction.SendChannelExcept:
                    // [int]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is int;

                case RLVRestriction.SendChannel:
                case RLVRestriction.SendChannelSec:
                    // [] || [int]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args[0] is int);

                case RLVRestriction.SendImTo:
                case RLVRestriction.RecvImFrom:
                    // [UUID | string]
                    return newCommand.Args.Count == 1 && newCommand.Args.All(n => n is UUID || n is string);

                case RLVRestriction.SendIm:
                case RLVRestriction.RecvIm:
                    // [] || [UUID | string]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args.All(n => n is UUID || n is string));

                case RLVRestriction.Detach:
                    // [AttachmentPoint]
                    return newCommand.Args.Count == 1 && newCommand.Args.All(n => n is AttachmentPoint);

                case RLVRestriction.AddAttach:
                case RLVRestriction.RemAttach:
                    // [] || [AttachmentPoint]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args.All(n => n is AttachmentPoint));

                case RLVRestriction.AddOutfit:
                    // [] || [layer]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args.All(n => n is WearableType));

                case RLVRestriction.DetachThis:
                case RLVRestriction.DetachAllThis:
                case RLVRestriction.AttachAllThis:
                    //[] || [uuid | layer | attachpt | string]
                    return newCommand.Args.Count == 0 || (newCommand.Args.Count == 1 && newCommand.Args.All(n =>
                               n is UUID ||
                               n is WearableType ||
                               n is AttachmentPoint ||
                               n is string));

                case RLVRestriction.AttachThis:
                    // [uuid | layer | attachpt | string]
                    return newCommand.Args.Count == 1 && newCommand.Args.All(n =>
                               n is UUID ||
                               n is WearableType ||
                               n is AttachmentPoint ||
                               n is string);

                case RLVRestriction.DetachThisExcept:
                case RLVRestriction.DetachAllThisExcept:
                case RLVRestriction.AttachThisExcept:
                case RLVRestriction.AttachAllThisExcept:
                    // [string]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is string;

                case RLVRestriction.CamTextures:
                case RLVRestriction.SetCamTextures:
                case RLVRestriction.RecvChat:
                case RLVRestriction.RecvEmote:
                case RLVRestriction.StartIm:
                case RLVRestriction.TpLure:
                case RLVRestriction.AcceptTp:
                case RLVRestriction.AcceptTpRequest:
                case RLVRestriction.TpRequest:
                case RLVRestriction.Edit:
                case RLVRestriction.Share:
                case RLVRestriction.TouchWorld:
                case RLVRestriction.TouchAttachOther:
                case RLVRestriction.TouchHud:
                case RLVRestriction.ShowNames:
                case RLVRestriction.ShowNamesSec:
                case RLVRestriction.ShowNameTags: // RLVA adds optional UUID
                                                  // [] || [UUID]
                    return newCommand.Args.Count == 0 ||
                           (newCommand.Args.Count == 1 && newCommand.Args[0] is UUID);

                case RLVRestriction.RecvChatFrom:
                case RLVRestriction.RecvEmoteFrom:
                case RLVRestriction.StartImTo:
                case RLVRestriction.EditObj:
                case RLVRestriction.TouchThis:
                case RLVRestriction.ShowHoverText:
                    // [UUID]
                    return newCommand.Args.Count == 1 && newCommand.Args[0] is UUID;

                case RLVRestriction.Permissive:
                case RLVRestriction.Fly:
                case RLVRestriction.TempRun:
                case RLVRestriction.AlwaysRun:
                case RLVRestriction.CamUnlock:
                case RLVRestriction.SetCamUnlock:
                case RLVRestriction.SendChat:
                case RLVRestriction.ChatShout:
                case RLVRestriction.ChatNormal:
                case RLVRestriction.ChatWhisper:
                case RLVRestriction.RecvChatSec:
                case RLVRestriction.SendGesture:
                case RLVRestriction.Emote:
                case RLVRestriction.RecvEmoteSec:
                case RLVRestriction.SendImSec:
                case RLVRestriction.RecvImSec:
                case RLVRestriction.TpLm:
                case RLVRestriction.TpLoc:
                case RLVRestriction.TpLureSec:
                case RLVRestriction.StandTp:
                case RLVRestriction.TpRequestSec:
                case RLVRestriction.ShowInv:
                case RLVRestriction.ViewNote:
                case RLVRestriction.ViewScript:
                case RLVRestriction.ViewTexture:
                case RLVRestriction.Rez:
                case RLVRestriction.EditWorld:
                case RLVRestriction.EditAttach:
                case RLVRestriction.ShareSec:
                case RLVRestriction.Unsit:
                case RLVRestriction.Sit:
                case RLVRestriction.DefaultWear:
                case RLVRestriction.AcceptPermission:
                case RLVRestriction.DenyPermission:
                case RLVRestriction.UnsharedWear:
                case RLVRestriction.UnsharedUnwear:
                case RLVRestriction.SharedWear:
                case RLVRestriction.SharedUnwear:
                case RLVRestriction.TouchAll:
                case RLVRestriction.TouchMe:
                case RLVRestriction.TouchAttach:
                case RLVRestriction.TouchAttachSelf:
                case RLVRestriction.Interact:
                case RLVRestriction.ShowWorldMap:
                case RLVRestriction.ShowMiniMap:
                case RLVRestriction.ShowLoc:
                case RLVRestriction.ShowNearby:
                case RLVRestriction.ShowHoverTextAll:
                case RLVRestriction.ShowHoverTextHud:
                case RLVRestriction.ShowHoverTextWorld:
                case RLVRestriction.SetGroup:
                case RLVRestriction.SetDebug:
                case RLVRestriction.SetEnv:
                case RLVRestriction.AllowIdle:
                    // []
                    return newCommand.Args.Count == 0;
            }

            return false;
        }
    }
}
