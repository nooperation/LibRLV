using System.Collections.Generic;

namespace LibRLV
{
    internal interface IBlacklistProvider
    {
        HashSet<string> GetBlacklist();
    }
}
