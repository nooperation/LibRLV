using OpenMetaverse;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace LibRLV
{
    public class RLV
    {
        public const string RLVVersion = "RestrainedLove viewer v3.4.3 (RLVa 2.4.2)";
        public const string RLVVersionNum = "2040213";

        public bool Enabled { get; set; }
        public bool EnableInstantMessageProcessing { get; set; }

        public RLVBlacklist Blacklist { get; }
        public RLVActionHandler Actions { get; }
        public RLVGetHandler Get { get; }
        public RLVRestrictionHandler Restrictions { get; }
        public IRLVCallbacks Callbacks { get; }
        public RLVManager RLVManager { get; }

        private readonly Regex RLVRegexPattern = new Regex(@"(?<behavior>[^:=]+)(:(?<option>[^=]*))?=(?<param>.+)", RegexOptions.Compiled);

        public RLV(IRLVCallbacks callbacks, bool enabled)
        {
            Callbacks = callbacks;
            Blacklist = new RLVBlacklist();
            Actions = new RLVActionHandler();
            Restrictions = new RLVRestrictionHandler(Callbacks);
            Get = new RLVGetHandler(Blacklist, Restrictions, Callbacks);
            RLVManager = new RLVManager(Restrictions, Callbacks);
            Enabled = enabled;
        }

        private bool ProcessRLVMessage(RLVMessage rlvMessage)
        {
            if (Blacklist.IsBlacklisted(rlvMessage.Behavior))
            {
                if (int.TryParse(rlvMessage.Param, out int channel))
                {
                    Callbacks.SendReplyAsync(channel, "", CancellationToken.None);
                }

                return false;
            }

            if (rlvMessage.Behavior == "clear")
            {
                return Restrictions.ProcessClearCommand(rlvMessage);
            }
            else if (rlvMessage.Param == "force")
            {
                return Actions.ProcessActionCommand(rlvMessage);
            }
            else if (rlvMessage.Param == "y" || rlvMessage.Param == "n" || rlvMessage.Param == "add" || rlvMessage.Param == "rem")
            {
                return Restrictions.ProcessRestrictionCommand(rlvMessage, rlvMessage.Option, rlvMessage.Param == "n" || rlvMessage.Param == "add");
            }
            else if (int.TryParse(rlvMessage.Param, out int channel))
            {
                if(channel == 0)
                {
                    return false;
                }

                return Get.ProcessGetCommand(rlvMessage, channel);
            }

            return false;
        }

        private bool ProcessSingleMessage(string message, UUID senderId, string senderName)
        {
            // Special hack for @clear, which doesn't match the standard pattern of @behavior=param
            if(message == "clear")
            {
                return ProcessRLVMessage(new RLVMessage()
                {
                    Behavior = message,
                    Option = "",
                    Param = "",
                    Sender = senderId,
                    SenderName = senderName
                });
            }

            var match = RLVRegexPattern.Match(message);
            if (!match.Success)
            {
                return false;
            }

            var rlvMessage = new RLVMessage
            {
                Behavior = match.Groups["behavior"].ToString().ToLower(),
                Option = match.Groups["option"].ToString(),
                Param = match.Groups["param"].ToString().ToLower(),
                Sender = senderId,
                SenderName = senderName
            };

            return ProcessRLVMessage(rlvMessage);
        }

        public bool ProcessMessage(string message, UUID senderId, string senderName)
        {
            if (!Enabled || !message.StartsWith("@"))
            {
                return false;
            }

            var result = true;
            foreach (var singleMessage in message.Substring(1).Split(','))
            {
                var isSuccessful = ProcessSingleMessage(singleMessage, senderId, senderName);
                if (!isSuccessful)
                {
                    result = false;
                }
            }

            return result;
        }

        public bool ProcessInstantMessage(string message, UUID senderId, string senderName)
        {
            if (!EnableInstantMessageProcessing || !Enabled || !message.StartsWith("@"))
            {
                return false;
            }

            if (Blacklist.IsBlacklisted(message))
            {
                return false;
            }

            return Get.ProcessInstantMessageCommand(message.ToLower(), senderId, senderName);
        }
    }
}
