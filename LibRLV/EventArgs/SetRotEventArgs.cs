using System;

namespace LibRLV
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
