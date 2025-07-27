using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;
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

        public virtual Task<string> ProvideDataAsync(RLVDataRequest request, List<object> data, CancellationToken cancellationToken)
        {
            return Task.FromResult(string.Empty);
        }

        public virtual Task<string> GetEnvironmentAsync(RLVGetEnvType command)
        {
            switch (command)
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
                    return Task.FromResult("0");

                case RLVGetEnvType.Ambient:
                case RLVGetEnvType.BlueDensity:
                case RLVGetEnvType.BlueHorizon:
                case RLVGetEnvType.CloudColor:
                case RLVGetEnvType.Cloud:
                case RLVGetEnvType.CloudDetail:
                case RLVGetEnvType.SunMoonColor:
                    return Task.FromResult("0;0;0");

                case RLVGetEnvType.CloudScroll:
                    return Task.FromResult("0;0");

                case RLVGetEnvType.Preset:
                case RLVGetEnvType.Asset:
                    return Task.FromResult("");

                case RLVGetEnvType.MoonImage:
                case RLVGetEnvType.SunImage:
                case RLVGetEnvType.CloudImage:
                    return Task.FromResult(UUID.Zero.ToString());

                case RLVGetEnvType.SunGlowSize:
                    return Task.FromResult("1");
            }

            return null;
        }

        public virtual Task<string> GetDebugInfoAsync(RLVGetDebugType command)
        {
            switch (command)
            {
                case RLVGetDebugType.AvatarSex:
                case RLVGetDebugType.RestrainedLoveForbidGiveToRLV:
                case RLVGetDebugType.WindLightUseAtmosShaders:
                    return Task.FromResult("0");

                case RLVGetDebugType.RenderResolutionDivisor:
                case RLVGetDebugType.RestrainedLoveNoSetEnv:
                    return Task.FromResult("1");
            }

            return null;
        }

        public virtual Task<bool> TryGetRlvInventoryTree(out InventoryTree sharedFolder)
        {
            sharedFolder = null;
            return Task.FromResult(false);
        }

        public virtual Task SendInstantMessageAsync(UUID targetUser, string message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        // TODO: Replace this, just temp hack to get the data i need right now for testing
        public Task<bool> TryGetObjectExists(UUID objectID, out bool isCurrentlySitting)
        {
            isCurrentlySitting = false;
            return Task.FromResult(false);
        }

        public virtual Task<bool> TryGetSitId(out UUID sitId)
        {
            sitId = default;
            return Task.FromResult(false);
        }

        public virtual Task<bool> TryGetCamAvDistMin(out float camAvDistMin)
        {
            camAvDistMin = default;
            return Task.FromResult(false);
        }

        public virtual Task<bool> TryGetCamAvDistMax(out float camAvdistmax)
        {
            camAvdistmax = default;
            return Task.FromResult(false);
        }

        public virtual Task<bool> TryGetCamFovMin(out float camFovMin)
        {
            camFovMin = default;
            return Task.FromResult(false);
        }

        public virtual Task<bool> TryGetCamFovMax(out float camFovMax)
        {
            camFovMax = default;
            return Task.FromResult(false);
        }

        public virtual Task<bool> TryGetCamZoomMin(out float camZoomMin)
        {
            camZoomMin = default;
            return Task.FromResult(false);
        }

        public virtual Task<bool> TryGetCamFov(out float camFov)
        {
            camFov = default;
            return Task.FromResult(false);
        }

        public virtual Task<bool> TryGetGroup(out string activeGroupName)
        {
            activeGroupName = "none";
            return Task.FromResult(false);
        }
    }
}
