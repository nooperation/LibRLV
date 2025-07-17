using System;

namespace LibRLV.EventArguments
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
