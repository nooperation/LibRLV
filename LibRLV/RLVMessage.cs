using System;

namespace LibRLV
{
    internal sealed class RLVMessage
    {
        public string Behavior { get; }
        public Guid Sender { get; }
        public string SenderName { get; }
        public string Option { get; }
        public string Param { get; }

        public RLVMessage(string behavior, Guid sender, string senderName, string option, string param)
        {
            Behavior = behavior;
            Sender = sender;
            SenderName = senderName;
            Option = option;
            Param = param;
        }

        public override string ToString()
        {
            return $"{Behavior} from {SenderName} ({Sender})";
        }
    }
}
