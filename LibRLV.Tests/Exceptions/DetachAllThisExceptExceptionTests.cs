using Moq;

namespace LibRLV.Tests.Exceptions
{
    public class DetachAllThisExceptExceptionTests : RestrictionsBase
    {
        #region @detachallthis_except:<folder>=<rem/add>

        [Fact]
        public async Task DetachAllThis_Recursive_ExceptAll()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@detachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));
            Assert.True(await _rlv.ProcessMessage($"@detachallthis_except:Clothing/Hats=add", sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat () - Parent folder locked recursively, but has exception
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat () - Parent folder locked recursively, but has exception
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public async Task DetachAllThis_Recursive_ExceptAll_Recursive()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@detachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));
            Assert.True(await _rlv.ProcessMessage($"@detachallthis_except:Clothing=add", sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat () - Parent folder locked recursively, but parent has recursive exception
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat () - Parent folder locked recursively, but parent has recursive exception
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()  - Locked, but folder has exception
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt () - Locked, but folder has exception
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants () - Locked, but folder has exception
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        #endregion

    }
}
