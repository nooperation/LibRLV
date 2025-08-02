using System;
using System.Collections.Immutable;

namespace LibRLV
{
    public interface IRestrictionProvider
    {
        ImmutableList<RLVRestriction> GetRestrictions(RLVRestrictionType restrictionType);
        ImmutableList<RLVRestriction> GetRestrictions(string behaviorNameFilter = "", Guid? senderFilter = null);
        bool TryGetLockedFolder(Guid folderId, out LockedFolderPublic lockedFolder);
        ImmutableDictionary<Guid, LockedFolderPublic> GetLockedFolders();
    }
}
