using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LibRLV
{
    public class RLVBlacklist : IBlacklistProvider
    {
        private readonly HashSet<string> _blacklist = new();
        private readonly object _blacklistLock = new();

        internal RLVBlacklist()
        {

        }

        public IReadOnlyCollection<string> GetBlacklist()
        {
            lock (_blacklistLock)
            {
                return _blacklist
                    .OrderBy(n => n)
                    .ToImmutableList();
            }
        }

        public void BlacklistCommand(string command)
        {
            lock (_blacklistLock)
            {
                _blacklist.Add(command.ToLowerInvariant());
            }
        }

        public void UnBlacklistCommand(string command)
        {
            lock (_blacklistLock)
            {
                _blacklist.Remove(command.ToLowerInvariant());
            }
        }

        public bool IsBlacklisted(string behavior)
        {
            lock (_blacklistLock)
            {
                return _blacklist.Contains(behavior.ToLowerInvariant());
            }
        }
    }
}
