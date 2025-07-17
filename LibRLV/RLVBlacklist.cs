using System.Collections.Generic;
using System.Linq;

namespace LibRLV
{
    public class RLVBlacklist : IBlacklistProvider
    {
        private HashSet<string> Blacklist = new HashSet<string>();

        public HashSet<string> GetBlacklist()
        {
            return new HashSet<string>(Blacklist);
        }

        public void BlacklistCommand(string command)
        {
            Blacklist.Add(command);
        }

        public void UnBlacklistCommand(string command)
        {
            Blacklist.Remove(command);
        }

        internal bool IsBlacklisted(string behavior)
        {
            return Blacklist.Contains(behavior);
        }
    }
}
