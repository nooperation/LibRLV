using System;
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

        public CameraRestrictions(IRestrictionProvider restrictionProvider)
        {
            RLVPermissionsService.GetRestrictionValueMax(restrictionProvider, RLVRestrictionType.CamZoomMin, out float? camZoomMin);
            RLVPermissionsService.GetRestrictionValueMax(restrictionProvider, RLVRestrictionType.SetCamFovMin, out float? setCamFovMin);
            RLVPermissionsService.GetRestrictionValueMax(restrictionProvider, RLVRestrictionType.SetCamAvDistMin, out float? setCamAvDistMin);
            RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamZoomMax, out float? camZoomMax);
            RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawMin, out float? camDrawMin);
            RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawMax, out float? camDrawMax);
            RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawAlphaMin, out float? camDrawAlphaMin);
            RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawAlphaMax, out float? camDrawAlphaMax);
            RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.SetCamFovMax, out float? setCamFovMax);
            RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.SetCamAvDistMax, out float? setCamAvDistMax);
            RLVPermissionsService.GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamAvDist, out float? camAvDist);

            GetCamDrawColor(restrictionProvider, out var camDrawColor);
            GetCamTexture(restrictionProvider, out var camtextures);

            IsLocked = restrictionProvider.GetRestrictions(RLVRestrictionType.SetCamUnlock).Count != 0;
            ZoomMin = camZoomMin;
            FovMin = setCamFovMin;
            AvDistMin = setCamAvDistMin;
            ZoomMax = camZoomMax;
            DrawMin = camDrawMin;
            DrawMax = camDrawMax;
            DrawAlphaMin = camDrawAlphaMin;
            DrawAlphaMax = camDrawAlphaMax;
            FovMax = setCamFovMax;
            AvDistMax = setCamAvDistMax;
            AvDist = camAvDist;
            DrawColor = camDrawColor;
            Texture = camtextures;
        }

        private static bool GetCamDrawColor(IRestrictionProvider restrictionProvider, out Vector3? camDrawColor)
        {
            camDrawColor = default;

            var restrictions = restrictionProvider
                .GetRestrictions(RLVRestrictionType.CamDrawColor)
                .Where(n => n.Args.Count == 3)
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

        private static bool GetCamTexture(IRestrictionProvider restrictionProvider, out Guid? textureUUID)
        {
            textureUUID = default;

            var restrictions = restrictionProvider.GetRestrictions(RLVRestrictionType.SetCamTextures);
            if (restrictions.Count == 0)
            {
                return false;
            }

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
