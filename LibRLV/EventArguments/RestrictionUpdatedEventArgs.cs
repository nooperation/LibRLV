using System;

namespace LibRLV.EventArguments
{
    public class RestrictionUpdatedEventArgs : EventArgs
    {
        public bool IsNew { get; }
        public bool IsDeleted { get; }
        public RLVRestriction Restriction { get; }

        public RestrictionUpdatedEventArgs(RLVRestriction restriction, bool isNew, bool isDeleted)
        {
            IsNew = isNew;
            IsDeleted = isDeleted;
            Restriction = restriction;
        }
    }
}
