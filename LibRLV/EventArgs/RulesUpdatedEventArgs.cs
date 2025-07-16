using System;

namespace LibRLV
{
    public class RulesUpdatedEventArgs : EventArgs
    {
        public bool IsNewRule { get; set; }
        public bool IsDeleted { get; set; }
        public RLVRule Rule { get; set; }

        public RulesUpdatedEventArgs()
        {
        }
    }
}
