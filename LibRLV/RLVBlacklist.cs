using System.Collections.Generic;

namespace LibRLV
{
    public class RLVBlacklist : IBlacklistProvider
    {
        private readonly HashSet<string> _blacklist = new HashSet<string>();

        public HashSet<string> GetBlacklist()
        {
            return new HashSet<string>(_blacklist);
        }

        public void BlacklistCommand(string command)
        {
            _blacklist.Add(command);
        }

        public void UnBlacklistCommand(string command)
        {
            _blacklist.Remove(command);
        }

        internal bool IsBlacklisted(string behavior)
        {
            return _blacklist.Contains(behavior);
        }
    }
}
