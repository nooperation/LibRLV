namespace LibRLV.Tests
{
    public class InventoryMapTests
    {
        // #RLV
        //  |
        //  |- .private
        //  |
        //  |- Clothing
        //  |    |= Business Pants (attached to 'groin')
        //  |    |= Happy Shirt (attached to 'chest')
        //  |    |= Retro Pants (worn on 'pants')
        //  |    \- +Hats
        //  |        |
        //  |        |- Sub Hats
        //  |        |    \ (Empty)
        //  |        |
        //  |        |= Fancy Hat (attached to 'chin')
        //  |        \= Party Hat (attached to 'groin')
        //   \-Accessories
        //        |= Watch (worn on 'tattoo')
        //        \= Glasses (attached to 'chin')

        #region TryGetFolderFromPath
        [Fact]
        public void TryGetFolderFromPath_Normal()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();

            var inventoryMap = new InventoryMap(sharedFolder);

            Assert.True(inventoryMap.TryGetFolderFromPath("Clothing/Hats", true, out var foundFolder));
            Assert.Equal(foundFolder, hatsFolder);
        }

        [Fact]
        public void TryGetFolderFromPath_FolderNameContainsForwardSlash()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            clothingFolder.Name = "Clo/thing";

            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();

            var inventoryMap = new InventoryMap(sharedFolder);

            Assert.True(inventoryMap.TryGetFolderFromPath("Clo/thing/Hats", true, out var foundFolder));
            Assert.Equal(foundFolder, hatsFolder);
        }

        [Fact]
        public void TryGetFolderFromPath_FolderNameContainsForwardSlashes()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            clothingFolder.Name = "/Clo//thing//";

            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = "//h/ats/";

            var inventoryMap = new InventoryMap(sharedFolder);

            Assert.True(inventoryMap.TryGetFolderFromPath($"{clothingFolder.Name}/{hatsFolder.Name}", true, out var foundFolder));
            Assert.Equal(foundFolder, hatsFolder);
        }


        [Fact]
        public void TryGetFolderFromPath_ContendingFoldersWithSlashes()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var contendingTree1 = new InventoryTree
            {
                Id = new Guid("12345678-1ddd-4ddd-8ddd-dddddddddddd"),
                Name = "Clothing",
                Parent = sharedFolder,
                Children = [],
                Items = [],
            };
            var contendingTree3 = new InventoryTree
            {
                Id = new Guid("12345678-123d-4ddd-8ddd-dddddddddddd"),
                Name = "+Clothing///",
                Parent = sharedFolder,
                Children = [],
                Items = [],
            };
            var contendingTree4 = new InventoryTree
            {
                Id = new Guid("12345678-123d-4ddd-8ddd-dddddddddddd"),
                Name = "+Clothing///",
                Parent = sharedFolder,
                Children = [],
                Items = [],
            };

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            clothingFolder.Name = "Clothing///";

            sharedFolder.Children.RemoveAt(0);
            sharedFolder.Children.Add(contendingTree1);
            sharedFolder.Children.Add(contendingTree3);
            sharedFolder.Children.Add(clothingFolder);
            sharedFolder.Children.Add(contendingTree4);

            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = "//h/ats/";

            var inventoryMap = new InventoryMap(sharedFolder);

            // We prefer the exact match of "Clothing///" over the not so exact match of "+Clothing///" since it's exactly what we're searching for
            Assert.True(inventoryMap.TryGetFolderFromPath($"{clothingFolder.Name}/{hatsFolder.Name}", true, out var foundFolder));
            Assert.Equal(foundFolder, hatsFolder);
        }

        [Fact]
        public void TryGetFolderFromPath_InvalidPath()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();

            var inventoryMap = new InventoryMap(sharedFolder);

            Assert.False(inventoryMap.TryGetFolderFromPath("Clothing/Hats123", true, out var foundFolder));
        }

        [Fact]
        public void TryGetFolderFromPath_IgnoreFolderPrefix()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            clothingFolder.Name = "~Clothing";

            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = "+Hats";

            var inventoryMap = new InventoryMap(sharedFolder);

            Assert.True(inventoryMap.TryGetFolderFromPath("Clothing/Hats", true, out var foundFolder));
            Assert.Equal(foundFolder, hatsFolder);
        }

        [Fact]
        public void TryGetFolderFromPath_FailOnHiddenFolder()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            clothingFolder.Name = ".Clothing";

            var inventoryMap = new InventoryMap(sharedFolder);

            Assert.False(inventoryMap.TryGetFolderFromPath(".Clothing", true, out var foundFolder));
        }

        [Fact]
        public void TryGetFolderFromPath_AllowHiddenFolder()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            clothingFolder.Name = ".Clothing";

            var inventoryMap = new InventoryMap(sharedFolder);

            Assert.True(inventoryMap.TryGetFolderFromPath(".Clothing", false, out var foundFolder));
            Assert.Equal(clothingFolder, foundFolder);
        }

        #endregion

        #region FindFoldersContaining
        [Fact]
        public void FindFoldersContaining_ById()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();

            var inventoryMap = new InventoryMap(sharedFolder);

            var actual = inventoryMap.FindFoldersContaining(false, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, null, null);

            var expected = new[] {
                hatsFolder
            };

            Assert.Equal(expected.OrderBy(n => n.Id), actual.OrderBy(n => n.Id));
        }

        [Fact]
        public void FindFoldersContaining_ByAttachmentType()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var accessoriesFolder = sampleTree.Root.Children.Where(n => n.Name == "Accessories").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();

            var inventoryMap = new InventoryMap(sharedFolder);

            var actual = inventoryMap.FindFoldersContaining(false, null, AttachmentPoint.Chin, null);

            var expected = new[] {
                hatsFolder,
                accessoriesFolder
            };

            Assert.Equal(expected.OrderBy(n => n.Id), actual.OrderBy(n => n.Id));
        }

        [Fact]
        public void FindFoldersContaining_ByAttachmentType_SingleResult()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var accessoriesFolder = sampleTree.Root.Children.Where(n => n.Name == "Accessories").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();

            var inventoryMap = new InventoryMap(sharedFolder);

            // What this returns doesn't seem to be really defined. 'return single result' is deprecated since there can be multiple results nowadays
            var actual = inventoryMap.FindFoldersContaining(true, null, AttachmentPoint.Chin, null);

            var expected = new[] {
                accessoriesFolder,
            };

            Assert.Equal(expected.OrderBy(n => n.Id), actual.OrderBy(n => n.Id));
        }

        [Fact]
        public void FindFoldersContaining_ByWearType()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var accessoriesFolder = sampleTree.Root.Children.Where(n => n.Name == "Accessories").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();

            var inventoryMap = new InventoryMap(sharedFolder);

            var actual = inventoryMap.FindFoldersContaining(false, null, null, WearableType.Pants);

            var expected = new[] {
                clothingFolder
            };

            Assert.Equal(expected.OrderBy(n => n.Id), actual.OrderBy(n => n.Id));
        }
        #endregion
    }
}
