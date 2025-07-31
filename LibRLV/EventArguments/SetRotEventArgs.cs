using System;

namespace LibRLV.EventArguments
{
    public class SetRotEventArgs : EventArgs
    {
        public float AngleInRadians { get; }

        public SetRotEventArgs(float angleInRadians)
        {
            AngleInRadians = angleInRadians;
        }
    }
}
