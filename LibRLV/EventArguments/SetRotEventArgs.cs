using System;

namespace LibRLV.EventArguments
{
    public class SetRotEventArgs : EventArgs
    {
        public double AngleInRadians { get; }

        public SetRotEventArgs(double angleInRadians)
        {
            this.AngleInRadians = angleInRadians;
        }
    }
}
