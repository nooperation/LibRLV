using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibRLV
{
    public class RLVCallbacksDefault : IRLVCallbacks
    {
        public virtual Task SendReplyAsync(int channel, string message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual Task<(bool Success, string EnvironmentSettingValue)> TryGetEnvironmentSettingValueAsync(string settingName, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse(settingName, true, out RLVGetEnvType settingType))
            {
                return Task.FromResult((false, string.Empty));
            }

            switch (settingType)
            {
                case RLVGetEnvType.Daytime:
                case RLVGetEnvType.AmbientR:
                case RLVGetEnvType.AmbientG:
                case RLVGetEnvType.AmbientB:
                case RLVGetEnvType.AmbientI:
                case RLVGetEnvType.BlueDensityR:
                case RLVGetEnvType.BlueDensityG:
                case RLVGetEnvType.BlueDensityB:
                case RLVGetEnvType.BlueDensityI:
                case RLVGetEnvType.BlueHorizonR:
                case RLVGetEnvType.BlueHorizonG:
                case RLVGetEnvType.BlueHorizonB:
                case RLVGetEnvType.BlueHorizonI:
                case RLVGetEnvType.CloudColorR:
                case RLVGetEnvType.CloudColorG:
                case RLVGetEnvType.CloudColorB:
                case RLVGetEnvType.CloudColorI:
                case RLVGetEnvType.CloudCoverage:
                case RLVGetEnvType.CloudX:
                case RLVGetEnvType.CloudY:
                case RLVGetEnvType.CloudD:
                case RLVGetEnvType.CloudDetailX:
                case RLVGetEnvType.CloudDetailY:
                case RLVGetEnvType.CloudDetailD:
                case RLVGetEnvType.CloudScale:
                case RLVGetEnvType.CloudScrollX:
                case RLVGetEnvType.CloudScrollY:
                case RLVGetEnvType.CloudVariance:
                case RLVGetEnvType.DensityMultiplier:
                case RLVGetEnvType.DistanceMultiplier:
                case RLVGetEnvType.DropletRadius:
                case RLVGetEnvType.EastAngle:
                case RLVGetEnvType.IceLevel:
                case RLVGetEnvType.HazeDensity:
                case RLVGetEnvType.HazeHorizon:
                case RLVGetEnvType.MaxAltitude:
                case RLVGetEnvType.MoistureLevel:
                case RLVGetEnvType.MoonAzim:
                case RLVGetEnvType.MoonNBrightness:
                case RLVGetEnvType.MoonElev:
                case RLVGetEnvType.MoonScale:
                case RLVGetEnvType.SceneGamma:
                case RLVGetEnvType.StarBrightness:
                case RLVGetEnvType.SunGlowFocus:
                case RLVGetEnvType.SunAzim:
                case RLVGetEnvType.SunElev:
                case RLVGetEnvType.SunScale:
                case RLVGetEnvType.SunMoonPosition:
                case RLVGetEnvType.SunMoonColorR:
                case RLVGetEnvType.SunMoonColorG:
                case RLVGetEnvType.SunMoonColorB:
                case RLVGetEnvType.SunMoonColorI:
                    return Task.FromResult((true, "0"));

                case RLVGetEnvType.Ambient:
                case RLVGetEnvType.BlueDensity:
                case RLVGetEnvType.BlueHorizon:
                case RLVGetEnvType.CloudColor:
                case RLVGetEnvType.Cloud:
                case RLVGetEnvType.CloudDetail:
                case RLVGetEnvType.SunMoonColor:
                    return Task.FromResult((true, "0;0;0"));

                case RLVGetEnvType.CloudScroll:
                    return Task.FromResult((true, "0;0"));

                case RLVGetEnvType.Preset:
                case RLVGetEnvType.Asset:
                    return Task.FromResult((true, ""));

                case RLVGetEnvType.MoonImage:
                case RLVGetEnvType.SunImage:
                case RLVGetEnvType.CloudImage:
                    return Task.FromResult((true, Guid.Empty.ToString()));

                case RLVGetEnvType.SunGlowSize:
                    return Task.FromResult((true, "1"));
            }

            return Task.FromResult((false, string.Empty));
        }

        public virtual Task<(bool Success, string DebugSettingValue)> TryGetDebugSettingValueAsync(string settingName, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse(settingName, true, out RLVGetDebugType settingType))
            {
                return Task.FromResult((false, string.Empty));
            }

            switch (settingType)
            {
                case RLVGetDebugType.AvatarSex:
                case RLVGetDebugType.RestrainedLoveForbidGiveToRLV:
                case RLVGetDebugType.WindLightUseAtmosShaders:
                    return Task.FromResult((true, "0"));

                case RLVGetDebugType.RenderResolutionDivisor:
                case RLVGetDebugType.RestrainedLoveNoSetEnv:
                    return Task.FromResult((true, "1"));
            }

            return Task.FromResult((false, string.Empty));
        }

        public virtual Task SendInstantMessageAsync(Guid targetUser, string message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        // TODO: Replace this, just temp hack to get the data i need right now for testing
        public Task<bool> ObjectExistsAsync(Guid objectID, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }
        public Task<bool> IsSittingAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public virtual Task<(bool Success, Guid SitId)> TryGetSitIdAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult((false, default(Guid)));
        }

        public virtual Task<(bool Success, InventoryTree? SharedFolder)> TryGetSharedFolderAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult((false, (InventoryTree?)null));
        }

        public virtual Task<(bool Success, CameraSettings? CameraSettings)> TryGetCameraSettingsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult((false, (CameraSettings?)null));
        }

        public virtual Task<(bool Success, string ActiveGroupName)> TryGetActiveGroupNameAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult((false, "None"));
        }

        public Task<(bool Success, IReadOnlyList<InventoryItem>? CurrentOutfit)> TryGetCurrentOutfitAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<InventoryItem> currentOutfit = [];
            return Task.FromResult((false, (IReadOnlyList<InventoryItem>?)currentOutfit));
        }
    }
}
