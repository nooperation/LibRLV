using System;

namespace LibRLV.EventArguments
{
    public class SetGroupEventArgs : EventArgs
    {
        public string GroupName { get; }
        public string Role { get; }
        public Guid GroupId { get; }

        public SetGroupEventArgs(string groupName, string role)
        {
            GroupName = groupName;
            Role = role;
            GroupId = Guid.Empty;
        }
        public SetGroupEventArgs(Guid groupID, string role)
        {
            GroupName = string.Empty;
            Role = role;
            GroupId = groupID;
        }
    }
}
