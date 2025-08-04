using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LibRLV
{
    public class LockedFolderPublic
    {
        public Guid Id { get; }
        public string Name { get; }
        public IReadOnlyList<RLVRestriction> DetachRestrictions { get; }
        public IReadOnlyList<RLVRestriction> AttachRestrictions { get; }
        public IReadOnlyList<RLVRestriction> DetachExceptions { get; }
        public IReadOnlyList<RLVRestriction> AttachExceptions { get; }

        public bool CanDetach => DetachExceptions.Count != 0 || DetachRestrictions.Count == 0;
        public bool CanAttach => AttachExceptions.Count != 0 || AttachRestrictions.Count == 0;
        public bool IsLocked => DetachRestrictions.Count != 0 || AttachRestrictions.Count != 0;

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
