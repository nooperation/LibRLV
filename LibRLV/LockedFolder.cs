using System.Collections.Generic;
using System.Linq;

namespace LibRLV
{
    internal class LockedFolder
    {
        internal LockedFolder(InventoryTree folder)
        {
            Folder = folder;
        }

        public InventoryTree Folder { get; }
        public List<RLVRestriction> DetachRestrictions { get; } = new List<RLVRestriction>();
        public List<RLVRestriction> AttachRestrictions { get; } = new List<RLVRestriction>();
        public List<RLVRestriction> DetachExceptions { get; } = new List<RLVRestriction>();
        public List<RLVRestriction> AttachExceptions { get; } = new List<RLVRestriction>();

        public bool CanDetach => DetachExceptions.Any() || !DetachRestrictions.Any();
        public bool CanAttach => AttachExceptions.Any() || !AttachRestrictions.Any();
        public bool IsLocked => DetachRestrictions.Any() || AttachRestrictions.Any();
    }
}
