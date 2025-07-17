using System;

namespace LibRLV.EventArguments
{
    public class InventoryPathEventArgs : EventArgs
    {
        public InventoryPathEventArgs(string inventoryPath)
        {
            this.InventoryPath = inventoryPath;
        }

        public string InventoryPath { get; }
    }
}
