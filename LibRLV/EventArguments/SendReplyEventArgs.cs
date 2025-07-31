using System;

namespace LibRLV.EventArguments
{
    public class SendReplyEventArgs : EventArgs
    {
        public SendReplyEventArgs(int channel, string reply)
        {
            Channel = channel;
            Reply = reply;
        }

        public int Channel { get; set; }
        public string Reply { get; set; }
    }
}
