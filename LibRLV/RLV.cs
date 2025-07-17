using OpenMetaverse;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace LibRLV
{
    /*
      TODO:
        - Implement @version and @getblacklist instant message handlers
        - 
    */
    public class RLV
    {
        public bool Enabled { get; set; }
        public bool EnableInstantMessageProcessing { get; set; }

        public RLVBlacklist Blacklist { get; }
        public RLVActionHandler Actions { get; }
        public RLVGetHandler Get { get; }
        public RLVRestrictionHandler Restrictions { get; }

        private readonly Regex RLVRegexPattern = new Regex(@"(?<behavior>[^:=]+)(:(?<option>[^=]*))?=(?<param>\w+)", RegexOptions.Compiled);

        public RLV()
        {
            Blacklist = new RLVBlacklist();
            Actions = new RLVActionHandler();
            Restrictions = new RLVRestrictionHandler();
            Get = new RLVGetHandler(Blacklist, Restrictions);
        }

        private bool ProcessRLVMessage(RLVMessage rlvMessage)
        {
            if (Blacklist.IsBlacklisted(rlvMessage.Behavior))
            {
                if (int.TryParse(rlvMessage.Param, out int channel))
                {
                    Get.SendReplyAsync?.Invoke(channel, "", CancellationToken.None);
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
                return Get.ProcessGetCommand(rlvMessage, channel);
            }

            return false;
        }

        private bool ProcessSingleMessage(string message, UUID senderId, string senderName)
        {
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

        public bool ProcessInstantMessage(string message, UUID senderId, string senderName, Action<UUID, string> instantMessageReply)
        {
            if (!EnableInstantMessageProcessing || !Enabled || !message.StartsWith("@"))
            {
                return false;
            }

            if (Blacklist.IsBlacklisted(message))
            {
                return false;
            }

            return Get.ProcessInstantMessageCommand(message.ToLower(), senderId, senderName, instantMessageReply);
        }
    }
}
