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

        private const float MinColorValue = 0.0f;
        private const float MaxColorValue = 1.0f;

        internal CameraRestrictions(IRestrictionProvider restrictionProvider)
        {
            ZoomMin = GetRestrictionValueMax(restrictionProvider, RLVRestrictionType.CamZoomMin);
            FovMin = GetRestrictionValueMax(restrictionProvider, RLVRestrictionType.SetCamFovMin);
            AvDistMin = GetRestrictionValueMax(restrictionProvider, RLVRestrictionType.SetCamAvDistMin);
            ZoomMax = GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamZoomMax);
            DrawMin = GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawMin);
            DrawMax = GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawMax);
            DrawAlphaMin = GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawAlphaMin);
            DrawAlphaMax = GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamDrawAlphaMax);
            FovMax = GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.SetCamFovMax);
            AvDistMax = GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.SetCamAvDistMax);
            AvDist = GetRestrictionValueMin(restrictionProvider, RLVRestrictionType.CamAvDist);

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

        private static float? GetRestrictionValueMax(IRestrictionProvider restrictionProvider, RLVRestrictionType restrictionType)
        {
            if (RLVPermissionsService.TryGetRestrictionValueMax(restrictionProvider, restrictionType, out var value))
            {
                return value;
            }

            return null;
        }

        private static float? GetRestrictionValueMin(IRestrictionProvider restrictionProvider, RLVRestrictionType restrictionType)
        {
            if (RLVPermissionsService.TryGetRestrictionValueMin(restrictionProvider, restrictionType, out var value))
            {
                return value;
            }

            return null;
        }

        private static float ClampColorValue(float value)
        {
            return Math.Min(MaxColorValue, Math.Max(MinColorValue, value));
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
                camDrawColorResult.X += ClampColorValue((float)restriction.Args[0]);
                camDrawColorResult.Y += ClampColorValue((float)restriction.Args[1]);
                camDrawColorResult.Z += ClampColorValue((float)restriction.Args[2]);
            }

            camDrawColorResult.X /= restrictions.Count;
            camDrawColorResult.Y /= restrictions.Count;
            camDrawColorResult.Z /= restrictions.Count;

            camDrawColor = camDrawColorResult;
            return true;
        }

        private static bool TryGetCamTexture(IRestrictionProvider restrictionProvider, [NotNullWhen(true)] out Guid? textureUUID)
        {
            var restrictions = restrictionProvider.GetRestrictionsByType(RLVRestrictionType.SetCamTextures);
            if (restrictions.Count == 0)
            {
                textureUUID = null;
                return false;
            }

            var lastRestriction = restrictions[restrictions.Count - 1];
            if (lastRestriction.Args.Count == 0)
            {
                textureUUID = Guid.Empty;
                return true;
            }

            if (lastRestriction.Args[0] is Guid restrictionTexture)
            {
                textureUUID = restrictionTexture;
                return true;
            }

            textureUUID = null;
            return false;
        }
    }
}
