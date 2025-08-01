using System.Collections.Immutable;
using OpenMetaverse;

namespace LibRLV
{
    public interface IRestrictionProvider
    {
        ImmutableList<RLVRestriction> GetRestrictions(RLVRestrictionType restrictionType);
        ImmutableList<RLVRestriction> GetRestrictions(string behaviorNameFilter = "", UUID? senderFilter = null);
        bool TryGetLockedFolder(UUID folderId, out LockedFolderPublic lockedFolder);
        ImmutableDictionary<UUID, LockedFolderPublic> GetLockedFolders();
    }
}
