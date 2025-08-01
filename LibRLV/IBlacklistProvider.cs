using System.Collections.Generic;

namespace LibRLV
{
    internal interface IBlacklistProvider
    {
        IReadOnlyCollection<string> GetBlacklist();
    }
}
