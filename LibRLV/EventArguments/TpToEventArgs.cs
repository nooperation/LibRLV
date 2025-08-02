using System;

namespace LibRLV.EventArguments
{
    public class TpToEventArgs : EventArgs
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public string RegionName { get; }
        public float? Lookat { get; }

        public TpToEventArgs(float x, float y, float z, string regionName, float? lookat)
        {
            X = x;
            Y = y;
            Z = z;
            RegionName = regionName;
            Lookat = lookat;
        }
    }
}
