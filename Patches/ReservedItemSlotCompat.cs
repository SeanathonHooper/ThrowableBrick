using System;
using System.Collections.Generic;
using System.Text;
using ReservedItemSlotCore.Data;

namespace ThrowableBrick.Patches
{
    public static class ReservedItemSlotCompat
    {
        public static void CreateSlotsAddItems()
        {
            ReservedItemData brick = new ReservedItemData("Brick");
            ReservedItemData fracturedBrick = new ReservedItemData("Fractured Brick");
            ReservedItemSlotData brickSlot = ReservedItemSlotData.CreateReservedItemSlotData("brickSlot", 20, 100);
            brickSlot.AddItemToReservedItemSlot(brick);
            brickSlot.AddItemToReservedItemSlot(fracturedBrick);
        }
    }
}
