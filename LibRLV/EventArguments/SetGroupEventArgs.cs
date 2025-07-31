using System;
using OpenMetaverse;

namespace LibRLV.EventArguments
{
    public class SetGroupEventArgs : EventArgs
    {
        public SetGroupEventArgs(string groupName)
        {
            GroupName = groupName;
            GroupId = UUID.Zero;
        }
        public SetGroupEventArgs(UUID groupID)
        {
            GroupName = string.Empty;
            GroupId = groupID;
        }

        public string GroupName { get; }
        public UUID GroupId { get; set; }
    }
}
