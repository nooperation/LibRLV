using System;
using System.Collections.Generic;

namespace LibRLV
{
    internal sealed class LockedFolder
    {
        internal LockedFolder(InventoryTree folder)
        {
            Folder = folder ?? throw new ArgumentException("Folder cannot be null", nameof(folder));
        }

        /// <summary>
        /// The locked folder
        /// </summary>
        public InventoryTree Folder { get; }

        /// <summary>
        /// All Detach restrictions for this folder
        /// </summary>
        public ICollection<RLVRestriction> DetachRestrictions { get; } = new List<RLVRestriction>();

        /// <summary>
        /// All Attach restrictions for this folder
        /// </summary>
        public ICollection<RLVRestriction> AttachRestrictions { get; } = new List<RLVRestriction>();

        /// <summary>
        /// All Detach exceptions for this folder
        /// </summary>
        public ICollection<RLVRestriction> DetachExceptions { get; } = new List<RLVRestriction>();

        /// <summary>
        /// All Attach exceptions for this folder
        /// </summary>
        public ICollection<RLVRestriction> AttachExceptions { get; } = new List<RLVRestriction>();


        /// <summary>
        /// Determines if items in this folder can be detached/unworn
        /// </summary>
        public bool CanDetach => DetachExceptions.Count != 0 || DetachRestrictions.Count == 0;

        /// <summary>
        /// Determines if items from this folder can be attached/worn
        /// </summary>
        public bool CanAttach => AttachExceptions.Count != 0 || AttachRestrictions.Count == 0;

        /// <summary>
        /// Determines if this folder is locked and cannot be modified
        /// </summary>
        public bool IsLocked => DetachRestrictions.Count != 0 || AttachRestrictions.Count != 0;
    }
}
