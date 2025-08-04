using System;
using System.Collections.Generic;

namespace LibRLV
{
    public interface IRestrictionProvider
    {
        IReadOnlyList<RLVRestriction> GetRestrictions(RLVRestrictionType restrictionType);
        IReadOnlyList<RLVRestriction> GetRestrictions(string behaviorNameFilter = "", Guid? senderFilter = null);
        bool TryGetLockedFolder(Guid folderId, out LockedFolderPublic lockedFolder);
        IReadOnlyDictionary<Guid, LockedFolderPublic> GetLockedFolders();
    }
}
