using System;

namespace LibRLV.EventArguments
{
    public class AdjustHeightEventArgs : EventArgs
    {
        public AdjustHeightEventArgs(float distance, float factor, float? delta)
        {
            this.Distance = distance;
            this.Factor = factor;
            this.Delta = delta;
        }

        public float Distance { get; }
        public float Factor { get; }
        public float? Delta { get; }
    }
}
