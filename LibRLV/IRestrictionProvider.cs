using System;
using System.Collections.Generic;

namespace LibRLV
{
    internal interface IRestrictionProvider
    {
        IReadOnlyList<RLVRestriction> GetRestrictionsByType(RLVRestrictionType restrictionType);
        IReadOnlyList<RLVRestriction> FindRestrictions(string behaviorNameFilter = "", Guid? senderFilter = null);
        bool TryGetLockedFolder(Guid folderId, out LockedFolderPublic lockedFolder);
        IReadOnlyDictionary<Guid, LockedFolderPublic> GetLockedFolders();
    }
}
