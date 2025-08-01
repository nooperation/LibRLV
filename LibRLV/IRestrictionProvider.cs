using System.Collections.Immutable;
using OpenMetaverse;

namespace LibRLV
{
    public interface IRestrictionProvider
    {
        ImmutableList<RLVRestriction> GetRestrictions(string filter = "", UUID? sender = null);
        ImmutableList<RLVRestriction> GetRestrictions(RLVRestrictionType restrictionType, UUID? sender = null);
        bool TryGetLockedFolder(UUID folderId, out LockedFolderPublic lockedFolder);
        ImmutableDictionary<UUID, LockedFolderPublic> GetLockedFolders();
    }
}
