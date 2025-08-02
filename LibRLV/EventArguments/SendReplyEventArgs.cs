using System;

namespace LibRLV.EventArguments
{
    public class SendReplyEventArgs : EventArgs
    {
        public int Channel { get; }
        public string Reply { get; }

        public SendReplyEventArgs(int channel, string reply)
        {
            Channel = channel;
            Reply = reply;
        }
    }
}
