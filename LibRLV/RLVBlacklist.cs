using System.Collections.Generic;
using System.Collections.Immutable;

namespace LibRLV
{
    public class RLVBlacklist : IBlacklistProvider
    {
        private readonly HashSet<string> _blacklist = new HashSet<string>();
        private readonly object _blacklistLock = new object();

        public IReadOnlyCollection<string> GetBlacklist()
        {
            lock (_blacklistLock)
            {
                return _blacklist.ToImmutableHashSet();
            }
        }

        public void BlacklistCommand(string command)
        {
            lock (_blacklistLock)
            {
                _blacklist.Add(command);
            }
        }

        public void UnBlacklistCommand(string command)
        {
            lock (_blacklistLock)
            {
                _blacklist.Remove(command);
            }
        }

        public bool IsBlacklisted(string behavior)
        {
            lock (_blacklistLock)
            {
                return _blacklist.Contains(behavior);
            }
        }
    }
}
