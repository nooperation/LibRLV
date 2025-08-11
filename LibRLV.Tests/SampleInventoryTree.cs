
namespace LibRLV.Tests
{
    public class SampleInventoryTree
    {
        public RlvSharedFolder Root { get; set; } = null!;
        public RlvInventoryItem Root_Clothing_Hats_FancyHat_AttachChin { get; set; } = null!;
        public RlvInventoryItem Root_Clothing_Hats_PartyHat_AttachGroin { get; set; } = null!;
        public RlvInventoryItem Root_Clothing_BusinessPants_AttachGroin { get; set; } = null!;
        public RlvInventoryItem Root_Clothing_RetroPants_WornPants { get; set; } = null!;
        public RlvInventoryItem Root_Clothing_HappyShirt_AttachChest { get; set; } = null!;
        public RlvInventoryItem Root_Accessories_Glasses_AttachChin { get; set; } = null!;
        public RlvInventoryItem Root_Accessories_Watch_WornTattoo { get; set; } = null!;


        public static List<RlvInventoryItem> BuildCurrentOutfit(RlvSharedFolder sharedFolder)
        {
            var inventoryMap = new InventoryMap(sharedFolder);

            var result = inventoryMap
                .Items
                .Where(n => n.Value.WornOn != null | n.Value.AttachedTo != null)
                .Select(n => n.Value)
                .ToList();

            return result;
        }

        public static SampleInventoryTree BuildInventoryTree()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing
            //  |    |= Business Pants (attached to 'groin')
            //  |    |= Happy Shirt (attached to 'chest')
            //  |    |= Retro Pants (worn on 'pants')
            //  |    \-Hats
            //  |        |
            //  |        |- Sub Hats
            //  |        |    \ (Empty)
            //  |        |
            //  |        |= Fancy Hat (attached to 'chin')
            //  |        \= Party Hat (attached to 'groin')
            //   \-Accessories
            //        |= Watch (worn on 'tattoo')
            //        \= Glasses (attached to 'chin')

            var root = new RlvSharedFolder(new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"), "#RLV");
            var clothingTree = root.AddChild(new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), "Clothing");
            var hatsTree = clothingTree.AddChild(new Guid("dddddddd-dddd-4ddd-8ddd-dddddddddddd"), "Hats");
            var subHatsTree = hatsTree.AddChild(new Guid("ffffffff-0000-4000-8000-000000000000"), "Sub Hats");
            var privateTree = root.AddChild(new Guid("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"), ".private");
            var accessoriesTree = root.AddChild(new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"), "Accessories");

            var watch_tattoo = accessoriesTree.AddItem(new Guid("c0000000-cccc-4ccc-8ccc-cccccccccccc"), "Watch", null, RlvWearableType.Tattoo);
            var glasses_chin = accessoriesTree.AddItem(new Guid("c1111111-cccc-4ccc-8ccc-cccccccccccc"), "Glasses", RlvAttachmentPoint.Chin, null);

            var businessPants_groin = clothingTree.AddItem(new Guid("b0000000-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), "Business Pants", RlvAttachmentPoint.Groin, null);
            var happyShirt_chest = clothingTree.AddItem(new Guid("b1111111-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), "Happy Shirt", RlvAttachmentPoint.Chest, null);
            var retroPants_pants = clothingTree.AddItem(new Guid("b2222222-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), "Retro Pants", null, RlvWearableType.Pants);

            var partyHat_groin = hatsTree.AddItem(new Guid("d0000000-dddd-4ddd-8ddd-dddddddddddd"), "Party Hat", RlvAttachmentPoint.Groin, null);
            var fancyHat_chin = hatsTree.AddItem(new Guid("d1111111-dddd-4ddd-8ddd-dddddddddddd"), "Fancy Hat", RlvAttachmentPoint.Chin, null);

            return new SampleInventoryTree()
            {
                Root = root,
                Root_Clothing_Hats_PartyHat_AttachGroin = partyHat_groin,
                Root_Clothing_Hats_FancyHat_AttachChin = fancyHat_chin,
                Root_Accessories_Glasses_AttachChin = glasses_chin,
                Root_Clothing_BusinessPants_AttachGroin = businessPants_groin,
                Root_Clothing_HappyShirt_AttachChest = happyShirt_chest,
                Root_Clothing_RetroPants_WornPants = retroPants_pants,
                Root_Accessories_Watch_WornTattoo = watch_tattoo
            };
        }
    }
}
