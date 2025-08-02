using System;

namespace LibRLV.EventArguments
{
    public class SetCamFOVEventArgs : EventArgs
    {
        public float FOVInRadians { get; }

        public SetCamFOVEventArgs(float fOVInRadians)
        {
            FOVInRadians = fOVInRadians;
        }

    }
}
