using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibRLV
{
    public interface IRLVCallbacks
    {
        Task SendReplyAsync(int channel, string message, CancellationToken cancellationToken);
        Task SendInstantMessageAsync(Guid targetUser, string message, CancellationToken cancellationToken);
        Task<string> GetEnvironmentAsync(string settingName);
        Task<string> GetDebugInfoAsync(string settingName);
        Task<bool> TryGetSitId(out Guid sitId);
        Task<bool> TryGetObjectExists(Guid objectID, out bool isCurrentlySitting);
        Task<bool> TryGetRlvInventoryTree(out InventoryTree sharedFolder);
        Task<bool> TryGetCamAvDistMin(out float camAvDistMin);
        Task<bool> TryGetCamAvDistMax(out float camAvdistmax);
        Task<bool> TryGetCamFovMin(out float camFovMin);
        Task<bool> TryGetCamFovMax(out float camFovMax);
        Task<bool> TryGetCamZoomMin(out float camZoomMin);
        Task<bool> TryGetCamFov(out float camFov);
        Task<bool> TryGetGroup(out string activeGroupName);
        Task<bool> TryGetCurrentOutfit(out List<InventoryTree.InventoryItem> currentOutfit);
    }
}
