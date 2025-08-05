using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

namespace LibRLV
{
    public class CameraRestrictions
    {
        public bool IsLocked { get; }

        public float? ZoomMin { get; }
        public float? ZoomMax { get; }

        public float? DrawMin { get; }
        public float? DrawMax { get; }

        public float? DrawAlphaMin { get; }
        public float? DrawAlphaMax { get; }

        public float? FovMin { get; }
        public float? FovMax { get; }

        public float? AvDistMin { get; }
        public float? AvDistMax { get; }
        public float? AvDist { get; }

        public Vector3? DrawColor { get; }

        public Guid? Texture { get; }

        internal CameraRestrictions(IRestrictionProvider restrictionProvider)
        {
            if (RLVPermissionsService.TryGetRestrictionValueMax(restrictionProvider, RLVRestrictionType.CamZoomMin, out var camZoomMin))
            {
                ZoomMin = camZoomMin;
            }
            if (RLVPermissionsService.TryGetRestrictionValueMax(restrictionProvider, RLVRestrictionType.SetCamFovMin, out var setCamFovMin))
            {
                FovMin = setCamFovMin;
            }
            if (RLVPermissionsService.TryGetRestrictionValueMax(restrictionProvider, RLVRestrictionType.SetCamAvDistMin, out var setCamAvDistMin))
            {
                AvDistMin = setCamAvDistMin;
            }
            if (RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamZoomMax, out var camZoomMax))
            {
                ZoomMax = camZoomMax;
            }
            if (RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawMin, out var camDrawMin))
            {
                DrawMin = camDrawMin;
            }
            if (RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawMax, out var camDrawMax))
            {
                DrawMax = camDrawMax;
            }
            if (RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawAlphaMin, out var camDrawAlphaMin))
            {
                DrawAlphaMin = camDrawAlphaMin;
            }
            if (RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawAlphaMax, out var camDrawAlphaMax))
            {
                DrawAlphaMax = camDrawAlphaMax;
            }
            if (RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.SetCamFovMax, out var setCamFovMax))
            {
                FovMax = setCamFovMax;
            }
            if (RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.SetCamAvDistMax, out var setCamAvDistMax))
            {
                AvDistMax = setCamAvDistMax;
            }
            if (RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamAvDist, out var camAvDist))
            {
                AvDist = camAvDist;
            }

            if (TryGetCamDrawColor(restrictionProvider, out var camDrawColor))
            {
                DrawColor = camDrawColor;
            }
            if (TryGetCamTexture(restrictionProvider, out var camtextures))
            {
                Texture = camtextures;
            }

            IsLocked = restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SetCamUnlock).Count != 0;
        }

        private static bool TryGetCamDrawColor(IRestrictionProvider restrictionProvider, out Vector3? camDrawColor)
        {
            camDrawColor = default;

            var restrictions = restrictionProvider
                .GetRestrictionsByType(RLVRestrictionType.CamDrawColor)
                .Where(n => n.Args.Count == 3 && n.Args.All(arg => arg is float))
                .ToList();
            if (restrictions.Count == 0)
            {
                camDrawColor = default;
                return false;
            }

            var camDrawColorResult = new Vector3();
            foreach (var restriction in restrictions)
            {
                camDrawColorResult.X += Math.Min(1.0f, Math.Max(0.0f, (float)restriction.Args[0]));
                camDrawColorResult.Y += Math.Min(1.0f, Math.Max(0.0f, (float)restriction.Args[1]));
                camDrawColorResult.Z += Math.Min(1.0f, Math.Max(0.0f, (float)restriction.Args[2]));
            }

            camDrawColorResult.X /= restrictions.Count;
            camDrawColorResult.Y /= restrictions.Count;
            camDrawColorResult.Z /= restrictions.Count;

            camDrawColor = camDrawColorResult;
            return true;
        }

        private static bool TryGetCamTexture(IRestrictionProvider restrictionProvider, [NotNullWhen(true)] out Guid? textureUUID)
        {
            textureUUID = default;

            var restrictions = restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SetCamTextures);
            if (restrictions.Count == 0)
            {
                return false;
            }

            textureUUID = Guid.Empty;
            foreach (var restriction in restrictions)
            {
                if (restriction.Args.Count == 0)
                {
                    textureUUID = Guid.Empty;
                }
                else if (restriction.Args.Count == 1 && restriction.Args[0] is Guid restrictionTexture)
                {
                    textureUUID = restrictionTexture;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
