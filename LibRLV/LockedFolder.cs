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
        public bool CanDetach => DetachExceptions.Any() || !DetachRestrictions.Any();
        public bool CanAttach => AttachExceptions.Any() || !AttachRestrictions.Any();
        public bool IsLocked => DetachRestrictions.Any() || AttachRestrictions.Any();

        public List<RLVRestriction> DetachRestrictions { get; set; } = new List<RLVRestriction>();
        public List<RLVRestriction> AttachRestrictions { get; set; } = new List<RLVRestriction>();
        public List<RLVRestriction> DetachExceptions { get; set; } = new List<RLVRestriction>();
        public List<RLVRestriction> AttachExceptions { get; set; } = new List<RLVRestriction>();
    }
}
