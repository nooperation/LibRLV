using System;

namespace LibRLV.EventArguments
{
    public class TpToEventArgs : EventArgs
    {
        public TpToEventArgs(float x, float y, float z, string regionName, float? lookat)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.RegionName = regionName;
            this.Lookat = lookat;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public string RegionName { get; }
        public float? Lookat { get; }
    }
}
