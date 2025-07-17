using System;

namespace LibRLV.EventArguments
{
    public class SetGroupEventArgs : EventArgs
    {
        public SetGroupEventArgs(string groupName)
        {
            this.GroupName = groupName;
        }

        public string GroupName { get; }
    }
}
