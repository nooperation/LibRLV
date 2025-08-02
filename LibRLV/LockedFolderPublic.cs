using System.Collections.Immutable;
using System.Linq;
using OpenMetaverse;

namespace LibRLV
{
    public class LockedFolderPublic
    {
        public UUID Id { get; }
        public string Name { get; }
        public ImmutableList<RLVRestriction> DetachRestrictions { get; }
        public ImmutableList<RLVRestriction> AttachRestrictions { get; }
        public ImmutableList<RLVRestriction> DetachExceptions { get; }
        public ImmutableList<RLVRestriction> AttachExceptions { get; }

        public bool CanDetach => DetachExceptions.Any() || !DetachRestrictions.Any();
        public bool CanAttach => AttachExceptions.Any() || !AttachRestrictions.Any();
        public bool IsLocked => DetachRestrictions.Any() || AttachRestrictions.Any();

        internal LockedFolderPublic(LockedFolder folder)
        {
            Id = folder.Folder.Id;
            Name = folder.Folder.Name;

            DetachRestrictions = folder.DetachRestrictions.ToImmutableList();
            AttachRestrictions = folder.AttachRestrictions.ToImmutableList();
            DetachExceptions = folder.DetachExceptions.ToImmutableList();
            AttachExceptions = folder.AttachExceptions.ToImmutableList();
        }
    }
}
