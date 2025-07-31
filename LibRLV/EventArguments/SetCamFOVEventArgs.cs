using System;

namespace LibRLV.EventArguments
{
    public class SetCamFOVEventArgs : EventArgs
    {
        public SetCamFOVEventArgs(float fOVInRadians)
        {
            FOVInRadians = fOVInRadians;
        }

        public float FOVInRadians { get; }
    }
}
