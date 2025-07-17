using System;

namespace LibRLV.EventArguments
{
    public class TpToEventArgs : EventArgs
    {
        public TpToEventArgs(double x, double y, double z, string regionName, double? lookat)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.RegionName = regionName;
            this.Lookat = lookat;
        }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public string RegionName { get; }
        public double? Lookat { get; }
    }
}
