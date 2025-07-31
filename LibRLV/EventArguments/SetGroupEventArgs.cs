using System;
using OpenMetaverse;

namespace LibRLV.EventArguments
{
    public class SetGroupEventArgs : EventArgs
    {
        public SetGroupEventArgs(string groupName, string role)
        {
            GroupName = groupName;
            Role = role;
            GroupId = UUID.Zero;
        }
        public SetGroupEventArgs(UUID groupID, string role)
        {
            GroupName = string.Empty;
            Role = role;
            GroupId = groupID;
        }

        public string GroupName { get; }
        public string Role { get; }
        public UUID GroupId { get; set; }
    }
}
