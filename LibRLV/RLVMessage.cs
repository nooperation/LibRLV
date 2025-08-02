using OpenMetaverse;

namespace LibRLV
{
    internal class RLVMessage
    {
        public string Behavior { get; set; }
        public string Option { get; set; }
        public string Param { get; set; }
        public UUID Sender { get; set; }
        public string SenderName { get; set; }
    }
}
