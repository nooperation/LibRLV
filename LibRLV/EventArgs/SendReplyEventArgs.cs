using System;

namespace LibRLV
{
    public class SendReplyEventArgs : EventArgs
    {
        public SendReplyEventArgs(int channel, string reply)
        {
            this.Channel = channel;
            this.Reply = reply;
        }

        public int Channel { get; set; }
        public string Reply { get; set; }
    }
}
