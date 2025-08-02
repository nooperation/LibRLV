using System;

namespace LibRLV.EventArguments
{
    public class AdjustHeightEventArgs : EventArgs
    {
        public float Distance { get; }
        public float Factor { get; }
        public float Delta { get; }

        public AdjustHeightEventArgs(float distance, float factor, float delta)
        {
            Distance = distance;
            Factor = factor;
            Delta = delta;
        }
    }
}
