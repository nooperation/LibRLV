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
            Assert.Equal(foundFolder, clothingFolder);
        }

        #endregion
    }
}
