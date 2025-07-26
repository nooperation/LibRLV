using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibRLV
{
    public interface IRLVCallbacks
    {
        Task SendReplyAsync(int channel, string message, CancellationToken cancellationToken);
        Task SendInstantMessageAsync(UUID targetUser, string message, CancellationToken cancellationToken);
        Task<string> ProvideDataAsync(RLVDataRequest request, List<object> data, CancellationToken cancellationToken);
        Task<string> GetEnvironmentAsync(RLVGetEnvType envType);
        Task<string> GetDebugInfoAsync(RLVGetDebugType debugType);
        Task<bool> TryGetSitId(out UUID sitId);
        Task<bool> TryGetObjectExists(UUID objectID, out bool isCurrentlySitting);
        Task<bool> TryGetRlvInventoryTree(out InventoryTree sharedFolder);

    }
}
