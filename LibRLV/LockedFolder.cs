using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OpenMetaverse;

namespace LibRLV
{
    internal class LockedFolder
    {
        internal LockedFolder(InventoryTree folder)
        {
            Folder = folder;
        }

        public InventoryTree Folder { get; }
        public bool CanDetach => DetachExceptions.Any() || !DetachRestrictions.Any();
        public bool CanAttach => AttachExceptions.Any() || !AttachRestrictions.Any();
        public bool IsLocked => DetachRestrictions.Any() || AttachRestrictions.Any();

        public List<RLVRestriction> DetachRestrictions { get; set; } = new List<RLVRestriction>();
        public List<RLVRestriction> AttachRestrictions { get; set; } = new List<RLVRestriction>();
        public List<RLVRestriction> DetachExceptions { get; set; } = new List<RLVRestriction>();
        public List<RLVRestriction> AttachExceptions { get; set; } = new List<RLVRestriction>();
    }

    public class LockedFolderPublic
    {
        internal LockedFolderPublic(LockedFolder folder)
        {
            Id = folder.Folder.Id;
            Name = folder.Folder.Name;

            DetachRestrictions = folder.DetachRestrictions.ToImmutableList();
            AttachRestrictions = folder.AttachRestrictions.ToImmutableList();
            DetachExceptions = folder.DetachExceptions.ToImmutableList();
            AttachExceptions = folder.AttachExceptions.ToImmutableList();
        }

        public UUID Id { get; set; }
        public string Name { get; set; }

        public bool CanDetach => DetachExceptions.Any() || !DetachRestrictions.Any();
        public bool CanAttach => AttachExceptions.Any() || !AttachRestrictions.Any();
        public bool IsLocked => DetachRestrictions.Any() || AttachRestrictions.Any();

        public ImmutableList<RLVRestriction> DetachRestrictions { get; }
        public ImmutableList<RLVRestriction> AttachRestrictions { get; }
        public ImmutableList<RLVRestriction> DetachExceptions { get; }
        public ImmutableList<RLVRestriction> AttachExceptions { get; }
    }
}
