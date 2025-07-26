using System;

namespace LibRLV.EventArguments
{
    public class SetCamFOVEventArgs : EventArgs
    {
        public SetCamFOVEventArgs(float fOVInRadians)
        {
            this.FOVInRadians = fOVInRadians;
        }

        public float FOVInRadians { get; }
    }
}
