using System.Collections.Generic;

namespace LibRLV
{
    internal sealed class LockedFolder
    {
        internal LockedFolder(InventoryTree folder)
        {
            Folder = folder;
        }

        public InventoryTree Folder { get; }
        public IList<RLVRestriction> DetachRestrictions { get; } = new List<RLVRestriction>();
        public IList<RLVRestriction> AttachRestrictions { get; } = new List<RLVRestriction>();
        public IList<RLVRestriction> DetachExceptions { get; } = new List<RLVRestriction>();
        public IList<RLVRestriction> AttachExceptions { get; } = new List<RLVRestriction>();

        public bool CanDetach => DetachExceptions.Count != 0 || DetachRestrictions.Count == 0;
        public bool CanAttach => AttachExceptions.Count != 0 || AttachRestrictions.Count == 0;
        public bool IsLocked => DetachRestrictions.Count != 0 || AttachRestrictions.Count != 0;
    }
}
