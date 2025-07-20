using OpenMetaverse;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LibRLV
{
    public interface IRestrictionProvider
    {
        ImmutableList<RLVRestriction> GetRestrictions(string filter = "", UUID? sender = null);
        ImmutableList<RLVRestriction> GetRestrictions(RLVRestrictionType restrictionType, UUID? sender = null);
    }
}
