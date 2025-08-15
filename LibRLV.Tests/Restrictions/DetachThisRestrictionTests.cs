﻿using Moq;

namespace LibRLV.Tests.Restrictions
{
    public class DetachThisRestrictionTests : RestrictionsBase
    {
        #region @detachthis[:<layer>|<attachpt>|<path_to_folder>]=<y/n>

        [Fact]
        public async Task DetachThis()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing
            //  |    |= Business Pants
            //  |    |= Happy Shirt
            //  |    |= Retro Pants
            //  |    \- Hats <-- Expected locked, no-detach
            //  |        |
            //  |        |- Sub Hats
            //  |        |    \ (Empty)
            //  |        |
            //  |        |= Fancy Hat <-- No detach
            //  |        \= Party Hat (Attached to spine) <-- No detach
            //   \-Accessories
            //        |= Watch
            //        \= Glasses
            //

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_Spine.AttachedTo = RlvAttachmentPoint.Spine;
            sampleTree.Root_Clothing_Hats_PartyHat_Spine.AttachedPrimId = new Guid("11111111-0003-4aaa-8aaa-ffffffffffff");

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@detachthis=n", sampleTree.Root_Clothing_Hats_PartyHat_Spine.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_Spine.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_Spine, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_Chin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_Pelvis, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch, true));

            var lockedFolders = _rlv.Restrictions.GetLockedFolders();
            Assert.Single(lockedFolders);

            Assert.True(lockedFolders.TryGetValue(sampleTree.Clothing_Hats_Folder.Id, out var hatsFolderLocked));

            Assert.Empty(hatsFolderLocked.AttachExceptions);
            Assert.Empty(hatsFolderLocked.AttachRestrictions);
            Assert.Empty(hatsFolderLocked.DetachExceptions);
            Assert.Single(hatsFolderLocked.DetachRestrictions);
        }

        [Fact]
        public async Task DetachThis_NotRecursive()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing <-- Expected locked, no-detach
            //  |    |= Business Pants (Attached pelvis) <-- No detach
            //  |    |= Happy Shirt <-- No detach
            //  |    |= Retro Pants <-- No detach
            //  |    \- Hats
            //  |        |
            //  |        |- Sub Hats
            //  |        |    \ (Empty)
            //  |        |
            //  |        |= Fancy Hat
            //  |        \= Party Hat
            //   \-Accessories
            //        |= Watch
            //        \= Glasses
            //

            // TryGetRlvInventoryTree
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_BusinessPants_Pelvis.AttachedTo = RlvAttachmentPoint.Pelvis;
            sampleTree.Root_Clothing_BusinessPants_Pelvis.AttachedPrimId = new Guid("11111111-0003-4aaa-8aaa-ffffffffffff");

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            // This should lock the #RLV/Clothing folder because the Business Pants are issuing the command, which is in the Clothing folder.
            //   Business Pants cannot be detached, but hats are still detachable.
            Assert.True(await _rlv.ProcessMessage("@detachthis=n", sampleTree.Root_Clothing_BusinessPants_Pelvis.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_Pelvis.Name));

            // #RLV/Clothing/Hats/Party Hat ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_Spine, true));

            // #RLV/Clothing/Hats/Fancy Hat ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_Chin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_Pelvis, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch, true));

            var lockedFolders = _rlv.Restrictions.GetLockedFolders();
            Assert.Single(lockedFolders);

            Assert.True(lockedFolders.TryGetValue(sampleTree.Clothing_Folder.Id, out var clothingFolderLocked));

            Assert.Empty(clothingFolderLocked.AttachExceptions);
            Assert.Empty(clothingFolderLocked.AttachRestrictions);
            Assert.Empty(clothingFolderLocked.DetachExceptions);
            Assert.Single(clothingFolderLocked.DetachRestrictions);
        }

        [Fact]
        public async Task DetachThis_ByPath()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing <-- Expected locked, no-detach
            //  |    |= Business Pants <-- No detach
            //  |    |= Happy Shirt <-- No detach
            //  |    |= Retro Pants <-- No detach
            //  |    \- Hats
            //  |        |
            //  |        |- Sub Hats
            //  |        |    \ (Empty)
            //  |        |
            //  |        |= Fancy Hat
            //  |        \= Party Hat
            //   \-Accessories
            //        |= Watch
            //        \= Glasses
            //

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            // This should lock the Hats folder, all hats are no longer detachable
            Assert.True(await _rlv.ProcessMessage("@detachthis:Clothing=n", _sender.Id, _sender.Name));

            // #RLV/Clothing/Hats/Party Hat ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_Spine, true));

            // #RLV/Clothing/Hats/Fancy Hat ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_Chin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_Pelvis, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch, true));

            var lockedFolders = _rlv.Restrictions.GetLockedFolders();
            Assert.Single(lockedFolders);

            Assert.True(lockedFolders.TryGetValue(sampleTree.Clothing_Folder.Id, out var clothingFolderLocked));

            Assert.Empty(clothingFolderLocked.AttachExceptions);
            Assert.Empty(clothingFolderLocked.AttachRestrictions);
            Assert.Empty(clothingFolderLocked.DetachExceptions);
            Assert.Single(clothingFolderLocked.DetachRestrictions);
        }

        [Fact]
        public async Task DetachThis_ByRlvAttachmentPoint()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing <-- Expected locked, no-detach
            //  |    |= Business Pants (Attached pelvis) <-- No detach
            //  |    |= Happy Shirt <-- No detach
            //  |    |= Retro Pants <-- No detach
            //  |    \- Hats
            //  |        |
            //  |        |- Sub Hats
            //  |        |    \ (Empty)
            //  |        |
            //  |        |= Fancy Hat <-- No detach
            //  |        \= Party Hat (Attached pelvis) <-- No detach
            //   \-Accessories
            //        |= Watch
            //        \= Glasses
            //

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_Spine.AttachedTo = RlvAttachmentPoint.Pelvis;
            sampleTree.Root_Clothing_Hats_PartyHat_Spine.AttachedPrimId = new Guid("11111111-0001-4aaa-8aaa-ffffffffffff");

            sampleTree.Root_Clothing_BusinessPants_Pelvis.AttachedTo = RlvAttachmentPoint.Pelvis;
            sampleTree.Root_Clothing_BusinessPants_Pelvis.AttachedPrimId = new Guid("11111111-0003-4aaa-8aaa-ffffffffffff");

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            // This should lock the Hats folder, all hats are no longer detachable
            Assert.True(await _rlv.ProcessMessage("@detachthis:pelvis=n", _sender.Id, _sender.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_Spine, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - folder was locked because PartyHat 
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_Chin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_Pelvis, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt, true));

            // #RLV/Clothing/Retro Pants (LOCKED) - folder was locked because BusinessPants
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch, true));

            var lockedFolders = _rlv.Restrictions.GetLockedFolders();
            Assert.Equal(2, lockedFolders.Count);

            Assert.True(lockedFolders.TryGetValue(sampleTree.Clothing_Folder.Id, out var clothingFolderLocked));
            Assert.True(lockedFolders.TryGetValue(sampleTree.Clothing_Hats_Folder.Id, out var hatsFolderLocked));

            Assert.Empty(clothingFolderLocked.AttachExceptions);
            Assert.Empty(clothingFolderLocked.AttachRestrictions);
            Assert.Empty(clothingFolderLocked.DetachExceptions);
            Assert.Single(clothingFolderLocked.DetachRestrictions);

            Assert.Empty(hatsFolderLocked.AttachExceptions);
            Assert.Empty(hatsFolderLocked.AttachRestrictions);
            Assert.Empty(hatsFolderLocked.DetachExceptions);
            Assert.Single(hatsFolderLocked.DetachRestrictions);
        }

        [Fact]
        public async Task DetachThis_ByWornLayer()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing
            //  |    |= Business Pants
            //  |    |= Happy Shirt
            //  |    |= Retro Pants
            //  |    \- Hats
            //  |        |
            //  |        |- Sub Hats
            //  |        |    \ (Empty)
            //  |        |
            //  |        |= Fancy Hat
            //  |        \= Party Hat
            //   \-Accessories
            //        |= Watch (Worn as tattoo) <-- no detach
            //        \= Glasses <-- No detach
            //

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Accessories_Watch.WornOn = RlvWearableType.Tattoo;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            // This should lock the Hats folder, all hats are no longer detachable
            Assert.True(await _rlv.ProcessMessage("@detachthis:tattoo=n", _sender.Id, _sender.Name));

            // #RLV/Clothing/Hats/Party Hat ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_Spine, true));

            // #RLV/Clothing/Hats/Fancy Hat ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_Chin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_Pelvis, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants, true));

            // #RLV/Accessories/Glasses (LOCKED) - folder was locked from Watch (tattoo)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses, true));

            // #RLV/Accessories/Watch (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch, true));

            var lockedFolders = _rlv.Restrictions.GetLockedFolders();
            Assert.Single(lockedFolders);

            Assert.True(lockedFolders.TryGetValue(sampleTree.Accessories_Folder.Id, out var accessoriesFolderLocked));

            Assert.Equal(sampleTree.Accessories_Folder.Name, accessoriesFolderLocked.Name);
            Assert.Equal(sampleTree.Accessories_Folder.Id, accessoriesFolderLocked.Id);
            Assert.False(accessoriesFolderLocked.CanDetach);
            Assert.True(accessoriesFolderLocked.CanAttach);
            Assert.True(accessoriesFolderLocked.IsLocked);

            Assert.Empty(accessoriesFolderLocked.AttachExceptions);
            Assert.Empty(accessoriesFolderLocked.AttachRestrictions);
            Assert.Empty(accessoriesFolderLocked.DetachExceptions);
            Assert.Single(accessoriesFolderLocked.DetachRestrictions);
        }

        [Fact]
        public async Task DetachThis_ByWornLayer_AddRem()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing
            //  |    |= Business Pants
            //  |    |= Happy Shirt
            //  |    |= Retro Pants
            //  |    \- Hats
            //  |        |
            //  |        |- Sub Hats
            //  |        |    \ (Empty)
            //  |        |
            //  |        |= Fancy Hat
            //  |        \= Party Hat
            //   \-Accessories
            //        |= Watch (Worn as tattoo)
            //        \= Glasses
            //

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Accessories_Watch.WornOn = RlvWearableType.Tattoo;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@detachthis:tattoo=n", _sender.Id, _sender.Name));
            Assert.True(await _rlv.ProcessMessage("@detachthis:tattoo=y", _sender.Id, _sender.Name));

            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_Spine, true));
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_Chin, true));
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_Pelvis, true));
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt, true));
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants, true));
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses, true));
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch, true));

            var lockedFolders = _rlv.Restrictions.GetLockedFolders();
            Assert.Empty(lockedFolders);
        }

        #endregion

    }
}
