using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibRLV
{
    public interface IRLVCallbacks
    {
        /// <summary>
        /// Sends a message on the given channel
        /// </summary>
        /// <param name="channel">Channel to send on</param>
        /// <param name="message">Message to send</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SendReplyAsync(int channel, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an instant message to a user
        /// </summary>
        /// <param name="targetUser">User to message</param>
        /// <param name="message">Message to send</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SendInstantMessageAsync(Guid targetUser, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if an object exists in the world
        /// </summary>
        /// <param name="objectID">Object ID to check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the object exists</returns>
        Task<bool> ObjectExistsAsync(Guid objectID, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the user is currently sitting
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if sitting</returns>
        Task<bool> IsSittingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets environment info for a setting
        /// </summary>
        /// <param name="settingName">Setting name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success flag and environment info if successful</returns>
        Task<(bool Success, string EnvironmentSettingValue)> TryGetEnvironmentSettingValueAsync(string settingName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets debug info for a setting
        /// </summary>
        /// <param name="settingName">Setting name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success flag and debug info if successful</returns>
        Task<(bool Success, string DebugSettingValue)> TryGetDebugSettingValueAsync(string settingName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the ID of the object the user is sitting on
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success flag and sit ID if successful</returns>
        Task<(bool Success, Guid SitId)> TryGetSitIdAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the RLV shared folder inventory tree
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success flag and inventory tree if successful</returns>
        Task<(bool Success, InventoryTree? SharedFolder)> TryGetSharedFolderAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets current camera settings
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success flag and camera settings if successful</returns>
        Task<(bool Success, CameraSettings? CameraSettings)> TryGetCameraSettingsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current user's active group name
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success flag and active group name if successful</returns>
        Task<(bool Success, string ActiveGroupName)> TryGetActiveGroupNameAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current user's outfit. This will be all worn and attached items and may include
        /// items outside of the shared #RLV folder
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success flag and current outfit if successful</returns>
        Task<(bool Success, IReadOnlyList<InventoryItem>? CurrentOutfit)> TryGetCurrentOutfitAsync(CancellationToken cancellationToken = default);
    }
}
