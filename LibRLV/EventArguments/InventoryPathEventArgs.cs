using System;

namespace LibRLV.EventArguments
{
    public class InventoryPathEventArgs : EventArgs
    {
        public string InventoryPath { get; }

        public InventoryPathEventArgs(string inventoryPath)
        {
            InventoryPath = inventoryPath;
        }
    }
}
