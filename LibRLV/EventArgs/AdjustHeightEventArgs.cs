using System;

namespace LibRLV
{
    public class AdjustHeightEventArgs : EventArgs
    {
        public AdjustHeightEventArgs(double distance, double factor, double? delta)
        {
            this.Distance = distance;
            this.Factor = factor;
            this.Delta = delta;
        }

        public double Distance { get; }
        public double Factor { get; }
        public double? Delta { get; }
    }
}
