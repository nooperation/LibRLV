using System;

namespace LibRLV.EventArguments
{
    public class SetGroupEventArgs : EventArgs
    {
        public string? GroupName { get; }
        public string? RoleName { get; }
        public Guid GroupId { get; }

        public SetGroupEventArgs(string groupName, string roleName)
        {
            GroupName = groupName;
            RoleName = roleName;
            GroupId = Guid.Empty;
        }
        public SetGroupEventArgs(Guid groupID, string roleName)
        {
            GroupName = string.Empty;
            RoleName = roleName;
            GroupId = groupID;
        }
    }
}
