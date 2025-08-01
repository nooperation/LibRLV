using OpenMetaverse;

namespace LibRLV.Tests
{
    public class SampleInventoryTree
    {
        public InventoryTree Root { get; set; } = null!;
        public InventoryTree.InventoryItem Root_Clothing_Hats_FancyHat_AttachChin { get; set; } = null!;
        public InventoryTree.InventoryItem Root_Clothing_Hats_PartyHat_AttachGroin { get; set; } = null!;
        public InventoryTree.InventoryItem Root_Clothing_BusinessPants_AttachGroin { get; set; } = null!;
        public InventoryTree.InventoryItem Root_Clothing_RetroPants_WornPants { get; set; } = null!;
        public InventoryTree.InventoryItem Root_Clothing_HappyShirt_AttachChest { get; set; } = null!;
        public InventoryTree.InventoryItem Root_Accessories_Glasses_AttachChin { get; set; } = null!;
        public InventoryTree.InventoryItem Root_Accessories_Watch_WornTattoo { get; set; } = null!;


        public static List<InventoryTree.InventoryItem> BuildCurrentOutfit(InventoryTree sharedFolder)
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

            var root = new InventoryTree()
            {
                Id = new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                Name = "#RLV",
                Parent = null,
                Children = [],
                Items = [],
            };

            var clothingTree = new InventoryTree()
            {
                Id = new UUID("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                Name = "Clothing",
                Parent = root,
                Children = [],
                Items = [],

            };
            root.Children.Add(clothingTree);

            var hatsTree = new InventoryTree
            {
                Id = new UUID("dddddddd-dddd-4ddd-8ddd-dddddddddddd"),
                Name = "Hats",
                Parent = clothingTree,
                Children = [],
                Items = [],
            };
            clothingTree.Children.Add(hatsTree);

            var subHatsTree = new InventoryTree
            {
                Id = new UUID("ffffffff-0000-4000-8000-000000000000"),
                Name = "Sub Hats",
                Parent = hatsTree,
                Children = [],
                Items = [],
            };
            hatsTree.Children.Add(subHatsTree);

            var privateTree = new InventoryTree
            {
                Id = new UUID("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"),
                Name = ".private",
                Parent = root,
                Children = [],
                Items = [],
            };
            root.Children.Add(privateTree);

            var AccessoriesTree = new InventoryTree
            {
                Id = new UUID("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                Name = "Accessories",
                Parent = root,
                Children = [],
                Items = [],
            };
            root.Children.Add(AccessoriesTree);

            var watch_tattoo = new InventoryTree.InventoryItem()
            {
                Id = new UUID("c0000000-cccc-4ccc-8ccc-cccccccccccc"),
                Name = "Watch",
                Type = InventoryTree.InventoryItem.ItemType.Wearable,
                AttachedTo = null,
                WornOn = WearableType.Tattoo,
                Folder = AccessoriesTree,
                FolderId = AccessoriesTree.Id
            };
            var glasses_chin = new InventoryTree.InventoryItem()
            {
                Id = new UUID("c1111111-cccc-4ccc-8ccc-cccccccccccc"),
                Name = "Glasses",
                Type = InventoryTree.InventoryItem.ItemType.Attachable,
                AttachedTo = AttachmentPoint.Chin,
                WornOn = null,
                Folder = AccessoriesTree,
                FolderId = AccessoriesTree.Id
            };
            var businessPants_groin = new InventoryTree.InventoryItem()
            {
                Id = new UUID("b0000000-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                Name = "Business Pants",
                Type = InventoryTree.InventoryItem.ItemType.Attachable,
                AttachedTo = AttachmentPoint.Groin,
                WornOn = null,
                Folder = clothingTree,
                FolderId = clothingTree.Id
            };
            var happyShirt_chest = new InventoryTree.InventoryItem()
            {
                Id = new UUID("b1111111-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                Name = "Happy Shirt",
                Type = InventoryTree.InventoryItem.ItemType.Attachable,
                AttachedTo = AttachmentPoint.Chest,
                WornOn = null,
                Folder = clothingTree,
                FolderId = clothingTree.Id
            };
            var retroPants_pants = new InventoryTree.InventoryItem()
            {
                Id = new UUID("b2222222-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                Name = "Retro Pants",
                Type = InventoryTree.InventoryItem.ItemType.Wearable,
                AttachedTo = null,
                WornOn = WearableType.Pants,
                Folder = clothingTree,
                FolderId = clothingTree.Id
            };
            var partyHat_groin = new InventoryTree.InventoryItem()
            {
                Id = new UUID("d0000000-dddd-4ddd-8ddd-dddddddddddd"),
                Name = "Party Hat",
                Type = InventoryTree.InventoryItem.ItemType.Attachable,
                AttachedTo = AttachmentPoint.Groin,
                WornOn = null,
                Folder = hatsTree,
                FolderId = hatsTree.Id
            };
            var fancyHat_chin = new InventoryTree.InventoryItem()
            {
                Id = new UUID("d1111111-dddd-4ddd-8ddd-dddddddddddd"),
                Name = "Fancy Hat",
                Type = InventoryTree.InventoryItem.ItemType.Attachable,
                AttachedTo = AttachmentPoint.Chin,
                WornOn = null,
                Folder = hatsTree,
                FolderId = hatsTree.Id
            };

            AccessoriesTree.Items.Add(watch_tattoo);
            AccessoriesTree.Items.Add(glasses_chin);
            clothingTree.Items.Add(businessPants_groin);
            clothingTree.Items.Add(happyShirt_chest);
            clothingTree.Items.Add(retroPants_pants);
            hatsTree.Items.Add(partyHat_groin);
            hatsTree.Items.Add(fancyHat_chin);

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
