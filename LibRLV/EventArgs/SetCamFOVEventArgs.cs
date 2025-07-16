using System;

namespace LibRLV
{
    public class SetCamFOVEventArgs : EventArgs
    {
        public SetCamFOVEventArgs(double fOVInRadians)
        {
            this.FOVInRadians = fOVInRadians;
        }

        public double FOVInRadians { get; }
    }
}
