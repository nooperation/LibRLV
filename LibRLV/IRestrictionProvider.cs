using OpenMetaverse;
using System.Collections.Generic;

namespace LibRLV
{
    internal interface IRestrictionProvider
    {
        List<RLVRestriction> GetRestrictions(string filter = "", UUID? sender = null);
    }
}
