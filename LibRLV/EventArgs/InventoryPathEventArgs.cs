using System;

namespace LibRLV
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
