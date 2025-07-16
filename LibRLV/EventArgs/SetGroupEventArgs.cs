using System;

namespace LibRLV
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
