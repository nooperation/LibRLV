using OpenMetaverse;
using System;

namespace LibRLV.EventArguments
{
    public class SetGroupEventArgs : EventArgs
    {
        public SetGroupEventArgs(string groupName)
        {
            this.GroupName = groupName;
            this.GroupId = UUID.Zero;
        }
        public SetGroupEventArgs(UUID groupID)
        {
            this.GroupName = string.Empty;
            this.GroupId = groupID;
        }

        public string GroupName { get; }
        public UUID GroupId { get; set; }
    }
}
