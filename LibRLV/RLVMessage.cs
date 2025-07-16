using OpenMetaverse;

namespace LibRLV
{
    public class RLVMessage
    {
        public string Behavior { get; set; }
        public string Option { get; set; }
        public string Param { get; set; }
        public UUID Sender { get; set; }
        public string SenderName { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}:{2}={3} [{4}]", SenderName, Behavior, Option, Param, Sender);
        }
    }
}
