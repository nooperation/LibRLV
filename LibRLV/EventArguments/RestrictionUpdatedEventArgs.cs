using System;

namespace LibRLV.EventArguments
{
    public class RestrictionUpdatedEventArgs : EventArgs
    {
        public bool IsNew { get; set; }
        public bool IsDeleted { get; set; }
        public RLVRestriction Restriction { get; set; }

        public RestrictionUpdatedEventArgs()
        {
        }
    }
}
