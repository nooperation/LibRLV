using System;
using System.Collections.Generic;

namespace LibRLV
{
    internal sealed class LockedFolder
    {
        internal LockedFolder(InventoryFolder folder)
        {
            Folder = folder ?? throw new ArgumentException("Folder cannot be null", nameof(folder));
        }

        public InventoryFolder Folder { get; }

        public ICollection<RLVRestriction> DetachRestrictions { get; } = new List<RLVRestriction>();
        public ICollection<RLVRestriction> AttachRestrictions { get; } = new List<RLVRestriction>();
        public ICollection<RLVRestriction> DetachExceptions { get; } = new List<RLVRestriction>();
        public ICollection<RLVRestriction> AttachExceptions { get; } = new List<RLVRestriction>();

        public bool CanDetach => DetachExceptions.Count != 0 || DetachRestrictions.Count == 0;
        public bool CanAttach => AttachExceptions.Count != 0 || AttachRestrictions.Count == 0;
        public bool IsLocked => DetachRestrictions.Count != 0 || AttachRestrictions.Count != 0;
    }
}
