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
        Task<bool> ObjectExistsAsync(Guid objectID);
        Task<bool> IsSittingAsync();

        Task<(bool Success, string EnvInfo)> TryGetEnvironmentAsync(string settingName);
        Task<(bool Success, string DebugInfo)> TryGetDebugInfoAsync(string settingName);
        Task<(bool Success, Guid SitID)> TryGetSitIdAsync();
        Task<(bool Success, InventoryTree? SharedFolder)> TryGetRlvInventoryTreeAsync();
        Task<(bool Success, float CamAvDistMin)> TryGetCamAvDistMinAsync();
        Task<(bool Success, float CamAvDistMax)> TryGetCamAvDistMaxAsync();
        Task<(bool Success, float CamFovMin)> TryGetCamFovMinAsync();
        Task<(bool Success, float CamFovMax)> TryGetCamFovMaxAsync();
        Task<(bool Success, float CamZoomMin)> TryGetCamZoomMinAsync();
        Task<(bool Success, float CamFov)> TryGetCamFovAsync();
        Task<(bool Success, string ActiveGroupName)> TryGetActiveGroupNameAsync();
        Task<(bool Success, IReadOnlyList<InventoryItem>? CurrentOutfit)> TryGetCurrentOutfitAsync();
    }
}
