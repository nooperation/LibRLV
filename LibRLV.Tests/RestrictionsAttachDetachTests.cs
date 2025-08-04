using LibRLV.EventArguments;
using Moq;

namespace LibRLV.Tests
{
    public class RestrictionsAttachDetachTests : RestrictionsBase
    {

        #region @detach=<y/n> |  @detach:<attach_point_name>=<y/n>

        [Fact]
        public void Detach_Default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectId2 = new Guid("11111111-1111-4111-8111-111111111111");

            var folderId1 = new Guid("99999999-9999-4999-8999-999999999999");

            Assert.True(_rlv.Permissions.CanDetach(folderId1, false, null, null));
            Assert.True(_rlv.Permissions.CanDetach(folderId1, false, AttachmentPoint.Chest, null));
            Assert.True(_rlv.Permissions.CanDetach(folderId1, false, null, WearableType.Shirt));

            Assert.True(_rlv.Permissions.CanDetach(folderId1, true, null, null));
            Assert.True(_rlv.Permissions.CanDetach(folderId1, true, AttachmentPoint.Chest, null));
            Assert.True(_rlv.Permissions.CanDetach(folderId1, true, null, WearableType.Shirt));
        }

        [Fact]
        public void Detach()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectId2 = new Guid("11111111-1111-4111-8111-111111111111");

            var folderId1 = new Guid("99999999-9999-4999-8999-999999999999");

            Assert.True(_rlv.ProcessMessage("@detach=n", _sender.Id, _sender.Name));

            Assert.False(_rlv.Permissions.CanDetach(folderId1, false, null, null));
            Assert.False(_rlv.Permissions.CanDetach(folderId1, false, AttachmentPoint.Chest, null));
            Assert.False(_rlv.Permissions.CanDetach(folderId1, false, null, WearableType.Shirt));

            Assert.False(_rlv.Permissions.CanDetach(folderId1, true, null, null));
            Assert.False(_rlv.Permissions.CanDetach(folderId1, true, AttachmentPoint.Chest, null));
            Assert.False(_rlv.Permissions.CanDetach(folderId1, true, null, WearableType.Shirt));
        }

        [Fact]
        public void Detach_AttachPoint()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectId2 = new Guid("11111111-1111-4111-8111-111111111111");

            var folderId1 = new Guid("99999999-9999-4999-8999-999999999999");

            Assert.True(_rlv.ProcessMessage("@detach:skull=n", _sender.Id, _sender.Name));

            Assert.True(_rlv.Permissions.CanDetach(folderId1, false, null, null));
            Assert.True(_rlv.Permissions.CanDetach(folderId1, false, AttachmentPoint.Chest, null));
            Assert.False(_rlv.Permissions.CanDetach(folderId1, false, AttachmentPoint.Skull, null));
            Assert.True(_rlv.Permissions.CanDetach(folderId1, false, null, WearableType.Shirt));

            Assert.True(_rlv.Permissions.CanDetach(folderId1, true, null, null));
            Assert.True(_rlv.Permissions.CanDetach(folderId1, true, AttachmentPoint.Chest, null));
            Assert.False(_rlv.Permissions.CanDetach(folderId1, true, AttachmentPoint.Skull, null));
            Assert.True(_rlv.Permissions.CanDetach(folderId1, true, null, WearableType.Shirt));
        }

        #endregion

        #region @addattach[:<attach_point_name>]=<y/n>
        [Fact]
        public void AddAttach()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@addattach=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Hats/Fancy Hat
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));
        }

        [Fact]
        public void AddAttach_Specific()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@addattach:groin=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Hats/Fancy Hat
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));
        }
        #endregion

        #region @remattach[:<attach_point_name>]=<y/n>
        [Fact]
        public void RemAttach()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@remattach=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Hats/Fancy Hat
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));
        }

        [Fact]
        public void RemAttach_Specific()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@remattach:groin=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Hats/Fancy Hat
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));
        }

        #endregion

        #region @defaultwear=<y/n>
        [Fact]
        public void CanDefaultWear()
        {
            CheckSimpleCommand("defaultWear", m => m.CanDefaultWear());
        }
        #endregion

        #region @addoutfit[:<part>]=<y/n>
        [Fact]
        public void AddOutfit()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@addoutfit=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Retro Pants
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void AddOutfit_part()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@addoutfit:pants=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Retro Pants
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }
        #endregion

        #region @remoutfit[:<part>]=<y/n>
        [Fact]
        public void RemOutfit()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@remoutfit=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Retro Pants
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void RemOutfit_part()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@remoutfit:pants=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Retro Pants
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }
        #endregion

        #region @remoutfit[:<folder|layer>]=force
        [Fact]
        public void RemOutfitForce()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);
            var sharedFolder = sampleTree.Root;

            // skin, shape, eyes and hair cannot be removed
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.WornOn = WearableType.Skin;

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<RemOutfitEventArgs>(
                 attach: n => _rlv.Commands.RemOutfit += n,
                 detach: n => _rlv.Commands.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit=force", _sender.Id, _sender.Name)
            );

            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Fact]
        public void RemOutfitForce_ExternalItems()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);

            var externalWearable = new InventoryTree.InventoryItem(
                new Guid("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Tattoo",
                null,
                WearableType.Tattoo,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );
            var externalAttachable = new InventoryTree.InventoryItem(
                new Guid("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Jaw Thing",
                AttachmentPoint.Jaw,
                null,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<RemOutfitEventArgs>(
                 attach: n => _rlv.Commands.RemOutfit += n,
                 detach: n => _rlv.Commands.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit=force", _sender.Id, _sender.Name)
            );

            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                externalWearable.Id
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Fact]
        public void RemOutfitForce_ExternalItems_ByType()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);
            var sharedFolder = sampleTree.Root;

            var externalWearable = new InventoryTree.InventoryItem(
                new Guid("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Tattoo",
                null,
                WearableType.Tattoo,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );
            var externalAttachable = new InventoryTree.InventoryItem(
                new Guid("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Jaw Thing",
                AttachmentPoint.Jaw,
                null,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<RemOutfitEventArgs>(
                 attach: n => _rlv.Commands.RemOutfit += n,
                 detach: n => _rlv.Commands.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit:tattoo=force", _sender.Id, _sender.Name)
            );

            var expected = new List<Guid>()
            {
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                externalWearable.Id
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Fact]
        public void RemOutfitForce_Folder()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.WornOn = WearableType.Tattoo;

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<RemOutfitEventArgs>(
                 attach: n => _rlv.Commands.RemOutfit += n,
                 detach: n => _rlv.Commands.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit:Clothing/Hats=force", _sender.Id, _sender.Name)
            );

            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Fact]
        public void RemOutfitForce_Specific()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<RemOutfitEventArgs>(
                 attach: n => _rlv.Commands.RemOutfit += n,
                 detach: n => _rlv.Commands.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit:tattoo=force", _sender.Id, _sender.Name)
            );

            var expected = new List<Guid>()
            {
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Fact]
        public void RemOutfitForce_BodyPart_Specific()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = WearableType.Skin;

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<RemOutfitEventArgs>(
                 attach: n => _rlv.Commands.RemOutfit += n,
                 detach: n => _rlv.Commands.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit:skin=force", _sender.Id, _sender.Name)
            );

            var expected = new List<Guid>()
            {
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }
        #endregion

        #region @getoutfit[:part]=<channel_number>
        [Fact]
        public void GetOutfit_WearingNothing()
        {
            var actual = _callbacks.RecordReplies();
            var currentOutfit = new List<InventoryTree.InventoryItem>();

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "0000000000000000"),
            };

            Assert.True(_rlv.ProcessMessage("@getoutfit=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOutfit_ExternalItems()
        {
            var actual = _callbacks.RecordReplies();

            var currentOutfit = new List<InventoryTree.InventoryItem>();

            var externalWearable = new InventoryTree.InventoryItem(
                new Guid("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Tattoo",
                null,
                WearableType.Tattoo,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );
            var externalAttachable = new InventoryTree.InventoryItem(
                new Guid("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Jaw Thing",
                AttachmentPoint.Jaw,
                null,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "0000000000000010"),
            };

            Assert.True(_rlv.ProcessMessage("@getoutfit=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOutfit_WearingSomeItems()
        {
            var actual = _callbacks.RecordReplies();
            var currentOutfit = new List<InventoryTree.InventoryItem>()
            {
                new(new Guid($"c0000000-cccc-4ccc-8ccc-cccccccccccc"), "My Socks", null, WearableType.Socks, new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc")),
                new(new Guid($"c0000001-cccc-4ccc-8ccc-cccccccccccc"), "My Hair", null, WearableType.Hair, new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"))
            };

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "0000001000010000"),
            };

            Assert.True(_rlv.ProcessMessage("@getoutfit=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOutfit_WearingEverything()
        {
            var actual = _callbacks.RecordReplies();
            var currentOutfit = new List<InventoryTree.InventoryItem>();
            foreach (var item in Enum.GetValues<WearableType>())
            {
                if (item == WearableType.Invalid)
                {
                    continue;
                }

                currentOutfit.Add(new InventoryTree.InventoryItem(
                    new Guid($"c{(int)item:D7}-cccc-4ccc-8ccc-cccccccccccc"),
                    $"My {item}",
                    null,
                    item,
                    new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc")
                ));
            }

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "1111111111111111"),
            };

            Assert.True(_rlv.ProcessMessage("@getoutfit=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOutfit_Specific_Exists()
        {
            var actual = _callbacks.RecordReplies();
            var currentOutfit = new List<InventoryTree.InventoryItem>()
            {
                new(new Guid($"c0000000-cccc-4ccc-8ccc-cccccccccccc"), "My Socks", null, WearableType.Socks, new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc")),
            };

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "1"),
            };

            Assert.True(_rlv.ProcessMessage("@getoutfit:socks=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOutfit_Specific_NotExists()
        {
            var actual = _callbacks.RecordReplies();
            var currentOutfit = new List<InventoryTree.InventoryItem>()
            {
                new(new Guid($"c0000001-cccc-4ccc-8ccc-cccccccccccc"), "My Hair", null, WearableType.Hair, new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"))
            };

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "0"),
            };

            Assert.True(_rlv.ProcessMessage("@getoutfit:socks=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }
        #endregion

        // TODO: There's a ton of undocumented RLVa stuff we need to implement, not just these

        #region @getattach[:attachpt]=<channel_number>
        [Fact]
        public void GetAttach_WearingNothing()
        {
            var actual = _callbacks.RecordReplies();
            var currentAttach = new List<InventoryTree.InventoryItem>();

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentAttach)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "00000000000000000000000000000000000000000000000000000000"),
            };

            Assert.True(_rlv.ProcessMessage("@getattach=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetAttach_ExternalItems()
        {
            var actual = _callbacks.RecordReplies();

            var currentOutfit = new List<InventoryTree.InventoryItem>();
            var externalWearable = new InventoryTree.InventoryItem(
                new Guid("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Tattoo",
                null,
                WearableType.Tattoo,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );
            var externalAttachable = new InventoryTree.InventoryItem(
                new Guid("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Jaw Thing",
                AttachmentPoint.Jaw,
                null,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "00000000000000000000000000000000000000000000000100000000"),
            };

            Assert.True(_rlv.ProcessMessage("@getattach=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetAttach_WearingSomeItems()
        {
            var actual = _callbacks.RecordReplies();
            var currentAttach = new List<InventoryTree.InventoryItem>()
            {
                new(new Guid($"c0000000-cccc-4ccc-8ccc-cccccccccccc"), "My Socks", AttachmentPoint.LeftFoot, null, new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc")),
                new(new Guid($"c0000001-cccc-4ccc-8ccc-cccccccccccc"), "My Hair", AttachmentPoint.Skull, null, new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"))
            };

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentAttach)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "00100001000000000000000000000000000000000000000000000000"),
            };

            Assert.True(_rlv.ProcessMessage("@getattach=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetAttach_WearingEverything()
        {
            var actual = _callbacks.RecordReplies();
            var currentAttach = new List<InventoryTree.InventoryItem>();
            foreach (var item in Enum.GetValues<AttachmentPoint>())
            {
                currentAttach.Add(new InventoryTree.InventoryItem(
                    new Guid($"c{(int)item:D7}-cccc-4ccc-8ccc-cccccccccccc"),
                    $"My {item}",
                    item,
                    null,
                    new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc")
                ));
            }

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentAttach)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "11111111111111111111111111111111111111111111111111111111"),
            };

            Assert.True(_rlv.ProcessMessage("@getattach=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetAttach_Specific_Exists()
        {
            var actual = _callbacks.RecordReplies();
            var currentAttach = new List<InventoryTree.InventoryItem>()
            {
                new(new Guid($"c0000000-cccc-4ccc-8ccc-cccccccccccc"), "My Socks", AttachmentPoint.LeftFoot, null, new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc")),
            };

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentAttach)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "1"),
            };

            Assert.True(_rlv.ProcessMessage("@getattach:left foot=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetAttach_Specific_NotExists()
        {
            var actual = _callbacks.RecordReplies();
            var currentAttach = new List<InventoryTree.InventoryItem>()
            {
                new(new Guid($"c0000001-cccc-4ccc-8ccc-cccccccccccc"), "My Hair", AttachmentPoint.Skull, null, new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"))
            };

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentAttach)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "0"),
            };

            Assert.True(_rlv.ProcessMessage("@getattach:left foot=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }
        #endregion

        #region @acceptpermission=<rem/add>
        [Fact]
        public void AcceptPermission()
        {
            Assert.True(_rlv.ProcessMessage($"@acceptpermission=add", _sender.Id, _sender.Name));
            Assert.True(_rlv.Permissions.IsAutoAcceptPermissions());

            Assert.True(_rlv.ProcessMessage($"@acceptpermission=rem", _sender.Id, _sender.Name));
            Assert.False(_rlv.Permissions.IsAutoAcceptPermissions());
        }
        #endregion

        #region @denypermission=<rem/add>
        [Fact]
        public void DenyPermission()
        {
            Assert.True(_rlv.ProcessMessage($"@denypermission=add", _sender.Id, _sender.Name));
            Assert.True(_rlv.Permissions.IsAutoDenyPermissions());

            Assert.True(_rlv.ProcessMessage($"@denypermission=rem", _sender.Id, _sender.Name));
            Assert.False(_rlv.Permissions.IsAutoDenyPermissions());
        }
        #endregion

        #region @detachme=force
        [Fact]
        public void DetachMeForce()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachme=force", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name)
            );

            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Fact]
        public void DetachMeForce_IgnoreNoStrip()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "nostrip Party Hat";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachme=force", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name)
            );

            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }
        #endregion

        //
        // Clothing and Attachments (Shared Folders)
        //

        #region @getinv[:folder1/.../folderN]=<channel_number>
        [Fact]
        public void GetInv()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Clothing,Accessories"),
            };

            Assert.True(_rlv.ProcessMessage("@getinv=1234", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetInv_Subfolder()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Sub Hats"),
            };

            Assert.True(_rlv.ProcessMessage("@getinv:Clothing/Hats=1234", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetInv_Empty()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, ""),
            };

            Assert.True(_rlv.ProcessMessage("@getinv:Clothing/Hats/Sub Hats=1234", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetInv_Invalid()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, ""),
            };

            Assert.True(_rlv.ProcessMessage("@getinv:Invalid Folder=1234", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));
            Assert.Equal(expected, actual);
        }
        #endregion

        #region @getinvworn[:folder1/.../folderN]=<channel_number>
        [Fact]
        public void GetInvWorn()
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
            //
            // 0: No item is present in that folder
            // 1: Some items are present in that folder, but none of them is worn
            // 2: Some items are present in that folder, and some of them are worn
            // 3: Some items are present in that folder, and all of them are worn
            //
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "|03,Clothing|33,Accessories|33"),
            };

            Assert.True(_rlv.ProcessMessage("@getinvworn=1234", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetInvWorn_PartialRoot()
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
            //
            // 0: No item is present in that folder
            // 1: Some items are present in that folder, but none of them is worn
            // 2: Some items are present in that folder, and some of them are worn
            // 3: Some items are present in that folder, and all of them are worn
            //
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = AttachmentPoint.Groin;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = WearableType.Tattoo;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "|02,Clothing|22,Accessories|22"),
            };

            Assert.True(_rlv.ProcessMessage("@getinvworn=1234", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetInvWorn_Naked()
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
            //
            // 0: No item is present in that folder
            // 1: Some items are present in that folder, but none of them is worn
            // 2: Some items are present in that folder, and some of them are worn
            // 3: Some items are present in that folder, and all of them are worn
            //
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "|01,Clothing|11,Accessories|11"),
            };

            Assert.True(_rlv.ProcessMessage("@getinvworn=1234", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetInvWorn_EmptyFolder()
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
            //
            // 0: No item is present in that folder
            // 1: Some items are present in that folder, but none of them is worn
            // 2: Some items are present in that folder, and some of them are worn
            // 3: Some items are present in that folder, and all of them are worn
            //
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "|00"),
            };

            Assert.True(_rlv.ProcessMessage("@getinvworn:Clothing/Hats/Sub Hats=1234", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetInvWorn_PartialWorn()
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
            //
            // 0: No item is present in that folder
            // 1: Some items are present in that folder, but none of them is worn
            // 2: Some items are present in that folder, and some of them are worn
            // 3: Some items are present in that folder, and all of them are worn
            //
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "|33,Sub Hats|00"),
            };

            Assert.True(_rlv.ProcessMessage("@getinvworn:Clothing/Hats=1234", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));
            Assert.Equal(expected, actual);
        }
        #endregion

        #region @findfolder:part1[&&...&&partN]=<channel_number>
        [Fact]
        public void FindFolder_MultipleTerms()
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

            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Clothing/Hats/Sub Hats"),
            };

            Assert.True(_rlv.ProcessMessage("@findfolder:at&&ub=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void FindFolder_SearchOrder()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Clothing/Hats"),
            };

            Assert.True(_rlv.ProcessMessage("@findfolder:at=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindFolder_IgnorePrivate()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = ".Hats";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, ""),
            };

            Assert.True(_rlv.ProcessMessage("@findfolder:at=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindFolder_IgnoreTildePrefix()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = "~Hats";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, ""),
            };

            Assert.True(_rlv.ProcessMessage("@findfolder:at=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }
        #endregion

        #region @findfolders:part1[&&...&&partN][;output_separator]=<channel_number>
        [Fact]
        public void FindFolders()
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

            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Clothing/Hats,Clothing/Hats/Sub Hats"),
            };

            Assert.True(_rlv.ProcessMessage("@findfolders:at=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindFolders_Separator()
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

            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Clothing/Hats AND Clothing/Hats/Sub Hats"),
            };

            Assert.True(_rlv.ProcessMessage("@findfolders:at; AND =1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }
        #endregion

        #region @getpath @getpathnew[:<attachpt> or <clothing_layer> or <uuid>]=<channel_number>

        [Fact]
        public void GetPathNew_BySender()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Clothing/Hats"),
            };

            Assert.True(_rlv.ProcessMessage("@getpathnew=1234", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPathNew_ByUUID()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Accessories"),
            };

            Assert.True(_rlv.ProcessMessage($"@getpathnew:{sampleTree.Root_Accessories_Glasses_AttachChin.Id}=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPathNew_ByUUID_Unknown()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, ""),
            };

            Assert.True(_rlv.ProcessMessage($"@getpathnew:BADBADBA-DBAD-4BAD-8BAD-BADBADBADBAD=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPathNew_ByAttach()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = AttachmentPoint.Groin;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = AttachmentPoint.Default;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = AttachmentPoint.Chin;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = AttachmentPoint.Groin;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Accessories,Clothing/Hats"),
            };

            Assert.True(_rlv.ProcessMessage($"@getpathnew:groin=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPathNew_ByWorn()
        {
            var actual = _callbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.WornOn = WearableType.Pants;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = WearableType.Tattoo;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = WearableType.Pants;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Accessories,Clothing/Hats"),
            };

            Assert.True(_rlv.ProcessMessage($"@getpathnew:pants=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        #endregion

        #region @attachover @attachoverorreplace @attach:<folder1/.../folderN>=force
        [Theory]
        [InlineData("attach", true)]
        [InlineData("attachoverorreplace", true)]
        [InlineData("attachover", false)]
        public void AttachForce(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:Clothing/Hats=force", _sender.Id, _sender.Name)
            );

            // Attach everything in the Clothing/Hats folder
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, AttachmentPoint.Spine, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attach", true)]
        [InlineData("attachoverorreplace", true)]
        [InlineData("attachover", false)]
        public void AttachForce_WithClothing(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.Name = "Happy Shirt (chest)";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:Clothing=force", _sender.Id, _sender.Name)
            );

            // Attach everything in the Clothing folder. Make sure clothing types (WearableType) are also included
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Chest, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, AttachmentPoint.Default, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attach")]
        [InlineData("attachoverorreplace")]
        [InlineData("attachover")]
        public void AttachForce_AlreadyAttached(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = AttachmentPoint.Groin;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = AttachmentPoint.Chest;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = WearableType.Pants;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:Clothing=force", _sender.Id, _sender.Name)
            );

            // Attach nothing because everything in this folder is already attached
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attach", true)]
        [InlineData("attachoverorreplace", true)]
        [InlineData("attachover", false)]
        public void AttachForce_PositionFromFolderName(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = "Hats (spine)";

            // Item name overrides folder name
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (skull)";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:{clothingFolder.Name}/{hatsFolder.Name}=force", _sender.Id, _sender.Name)
            );

            // Attach everything under the "Clothing/Hats (spine)" folder, attaching everything to the Spine point unless the item explicitly
            //  specifies a different attachment point such as "Fancy Hat (skull)".
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Skull, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, AttachmentPoint.Spine, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attach")]
        [InlineData("attachoverorreplace")]
        [InlineData("attachover")]
        public void AttachForce_FolderNameSpecifiesToAddInsteadOfReplace(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = "+Hats";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:{clothingFolder.Name}/{hatsFolder.Name}=force", _sender.Id, _sender.Name)
            );

            // Attach everything inside of the Clothing/Hats folder, but force 'add to' logic due to the + prefix on the hats folder
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Chin, false),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, AttachmentPoint.Spine, false),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attach")]
        [InlineData("attachoverorreplace")]
        [InlineData("attachover")]
        public void AttachForce_AttachPrivateParentFolder(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            clothingFolder.Name = ".clothing";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:{clothingFolder.Name}/{hatsFolder.Name}=force", _sender.Id, _sender.Name)
            );

            // Attach nothing because one of the folders in the path is a private (. prefix) folder
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }
        #endregion

        #region @attachallover @attachalloverorreplace @attachall:<folder1/.../folderN>=force

        [Theory]
        [InlineData("attachall", true)]
        [InlineData("attachalloverorreplace", true)]
        [InlineData("attachallover", false)]
        public void AttachForce_Recursive(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business Pants (Pelvis)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:Clothing=force", _sender.Id, _sender.Name)
            );

            // Attach everything inside of of the Clothing folder, and all of its subfolders recursively
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, AttachmentPoint.Pelvis, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, AttachmentPoint.Spine, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attachall", true)]
        [InlineData("attachalloverorreplace", true)]
        [InlineData("attachallover", false)]
        public void AttachForce_Recursive_WithHiddenSubfolder(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business Pants (Pelvis)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = ".hats";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:Clothing=force", _sender.Id, _sender.Name)
            );

            // Attach everything inside of of the Clothing folder, and all of its subfolders recursively. The hats folder has a special . prefix,
            //   which means it will be ignored
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, AttachmentPoint.Pelvis, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attachall", true)]
        [InlineData("attachalloverorreplace", true)]
        [InlineData("attachallover", false)]
        public void AttachForce_Recursive_FolderNameSpecifiesToAddInsteadOfReplace(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business Pants (Pelvis)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = "+hats";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:Clothing=force", _sender.Id, _sender.Name)
            );

            // Attach everything inside of of the Clothing folder, and all of its subfolders recursively. The hats folder has a special + prefix,
            //   which means it will use 'add to' logic instead of 'replace' logic when attaching
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, AttachmentPoint.Pelvis, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Chin, false),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, AttachmentPoint.Spine, false),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        #endregion

        #region @attachthisoverorreplace @attachthisover @attachthis[:<attachpt> or <clothing_layer> or <uuid>]=force
        [Theory]
        [InlineData("attachthis", true)]
        [InlineData("attachthisoverorreplace", true)]
        [InlineData("attachthisover", false)]
        public void AttachThis_Default(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name)
            );

            // Attach everything in #RLV/Clothing/Hats because that's where the source item (fancy hat) is calling @attachthis from
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, AttachmentPoint.Spine, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attachthis")]
        [InlineData("attachthisoverorreplace")]
        [InlineData("attachthisover")]
        public void AttachThis_FolderNameSpecifiesToAddInsteadOfReplace(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business Pants (groin)";
            sampleTree.Root_Clothing_HappyShirt_AttachChest.Name = "Happy Shirt (Spine)";
            sampleTree.Root_Clothing_RetroPants_WornPants.Name = "Worn Pants";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            clothingFolder.Name = "+clothing";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name)
            );

            // Attach everything in #RLV/+clothing because that's where the source item (business pants) is calling @attachthis
            //   from, but use 'add-to' logic instead of 'replace' logic
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, AttachmentPoint.Groin, false),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Spine, false),
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, AttachmentPoint.Default, false),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attachthis", true)]
        [InlineData("attachthisoverorreplace", true)]
        [InlineData("attachthisover", false)]
        public void AttachThis_FolderNameSpecifiesAttachmentPoint(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = "(skull) hats";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name)
            );

            // Attach everything in #RLV/Clothing/+Hats because that's where the source item (fancy hat) is calling @attachthis
            //   from, but attach "party hat" to the skull because it doesn't specify an attachment point but the folder name does
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, AttachmentPoint.Skull, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attachthis")]
        [InlineData("attachthisoverorreplace")]
        [InlineData("attachthisover")]
        public void AttachThis_FromHiddenSubfolder(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = ".hats";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name)
            );

            // Nothing from ./Clothing/.Hats is worn because it's private, even though the sender exists in this folder
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attachthis", true)]
        [InlineData("attachthisoverorreplace", true)]
        [InlineData("attachthisover", false)]
        public void AttachThis_AttachPoint(string command, bool replaceExistingAttachments)
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

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_HappyShirt_AttachChest.Name = "Happy Shirt (chest)";
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business pants (spine)";
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = AttachmentPoint.Spine;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (spine)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = AttachmentPoint.Spine;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:spine=force", sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name)
            );

            // Attach happy shirt because it's in the same folder as our business pants (attached to spine).
            // Attach retro pants because it's in the same folder as our business pants (attached to spine).
            // Attach fancy hat because it's in the same folder as our party hat (attached to spine)
            // Don't attach BusinessPants or PartyHat because they are already attached
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Chest, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Chin, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attachthis", true)]
        [InlineData("attachthisoverorreplace", true)]
        [InlineData("attachthisover", false)]
        public void AttachThis_WearableType(string command, bool replaceExistingAttachments)
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

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;

            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = WearableType.Tattoo;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = WearableType.Tattoo;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:tattoo=force", _sender.Id, _sender.Name)
            );

            // We are currently wearing Tattoo items in "./Clothing" and "./Accessories". Wear everything from these two folders
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Accessories_Glasses_AttachChin.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, AttachmentPoint.Default, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }
        #endregion

        #region @attachallthisover @attachallthisoverorreplace @attachallthis[:<attachpt> or <clothing_layer>]=force
        [Theory]
        [InlineData("attachallthis", true)]
        [InlineData("attachallthisoverorreplace", true)]
        [InlineData("attachallthisover", false)]
        public void AttachAllThisForce_Recursive(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business Pants (Pelvis)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_RetroPants_WornPants.Id, sampleTree.Root_Clothing_RetroPants_WornPants.Name)
            );

            // Attach everything inside of of the Clothing folder (sender exists in the clothing folder), and all of its subfolders recursively
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, AttachmentPoint.Pelvis, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, AttachmentPoint.Spine, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attachallthis", true)]
        [InlineData("attachallthisoverorreplace", true)]
        [InlineData("attachallthisover", false)]
        public void AttachAllThisForce_Recursive_WithHiddenSubfolder(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business Pants (Pelvis)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = ".hats";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_RetroPants_WornPants.Id, sampleTree.Root_Clothing_RetroPants_WornPants.Name)
            );

            // Attach everything inside of of the Clothing folder (sender exists in the clothing folder), and all of its subfolders recursively.
            //   The hats folder has a special . prefix, which means it will be ignored
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, AttachmentPoint.Pelvis, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attachallthis", true)]
        [InlineData("attachallthisoverorreplace", true)]
        [InlineData("attachallthisover", false)]
        public void AttachAllThisForce_Recursive_FolderNameSpecifiesToAddInsteadOfReplace(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business Pants (Pelvis)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = "+hats";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_RetroPants_WornPants.Id, sampleTree.Root_Clothing_RetroPants_WornPants.Name)
            );

            // Attach everything inside of of the Clothing folder (sender exists in the clothing folder), and all of its subfolders recursively.
            //   The hats folder has a special + prefix, which means it will use 'add to' logic instead of 'replace' logic when attaching
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, AttachmentPoint.Pelvis, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Chin, false),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, AttachmentPoint.Spine, false),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attachallthis", true)]
        [InlineData("attachallthisoverorreplace", true)]
        [InlineData("attachallthisover", false)]
        public void AttachAllThisForce_AttachPoint(string command, bool replaceExistingAttachments)
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

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business pants (spine)";
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = AttachmentPoint.Spine;

            sampleTree.Root_Clothing_HappyShirt_AttachChest.Name = "Happy Shirt (chest)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (neck)";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:spine=force", _sender.Id, _sender.Name)
            );

            // Attach happy shirt because it's in the same folder as our business pants (attached to spine).
            // Attach retro pants because it's in the same folder as our business pants (attached to spine).
            // Attach fancy hat because it's in a subfolder of our business pants
            // Attach party hat because it's in a subfolder of our business pants
            // Don't attach BusinessPants because they are already attached
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Chest, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, AttachmentPoint.Neck, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }

        [Theory]
        [InlineData("attachallthis", true)]
        [InlineData("attachallthisoverorreplace", true)]
        [InlineData("attachallthisover", false)]
        public void AttachAllThisForce_WearableType(string command, bool replaceExistingAttachments)
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

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;

            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = WearableType.Tattoo;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business pants";
            sampleTree.Root_Clothing_HappyShirt_AttachChest.Name = "Happy Shirt (chest)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (neck)";

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<AttachmentEventArgs>(
                 attach: n => _rlv.Commands.Attach += n,
                 detach: n => _rlv.Commands.Attach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:tattoo=force", _sender.Id, _sender.Name)
            );

            // Attach happy shirt because it's in the same folder as our retro pants (worn as tattoo).
            // Attach retro pants because it's in the same folder as our retro pants (worn as tattoo).
            // Attach fancy hat because it's in a subfolder of our retro pants
            // Attach party hat because it's in a subfolder of our retro pants
            // Don't attach retro pants because they are already attached
            var expected = new List<AttachmentEventArgs.AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, AttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, AttachmentPoint.Chest, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, AttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, AttachmentPoint.Neck, replaceExistingAttachments),
            }.OrderBy(n => n.ItemId);

            Assert.Equal(expected, raised.Arguments.ItemsToAttach.OrderBy(n => n.ItemId));
        }
        #endregion

        #region @detach @remattach[:<folder|attachpt|uuid>]=force
        [Theory]
        [InlineData("@detach=force")]
        [InlineData("@remattach=force")]
        public void RemAttach_RemoveAllAttachments(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage(command, _sender.Id, _sender.Name)
            );

            // Remove everything except for clothing despite what you would think. Just how things go.
            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id,
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                 sampleTree.Root_Accessories_Glasses_AttachChin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Theory]
        [InlineData("@detach=force")]
        [InlineData("@remattach=force")]
        public void RemAttach_RemoveAllAttachments_ExternalItems(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);

            var externalWearable = new InventoryTree.InventoryItem(
                new Guid("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Tattoo",
                null,
                WearableType.Tattoo,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );
            var externalAttachable = new InventoryTree.InventoryItem(
                new Guid("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Jaw Thing",
                AttachmentPoint.Jaw,
                null,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage(command, _sender.Id, _sender.Name)
            );

            // Remove everything except for clothing despite what you would think. Just how things go.
            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id,
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Accessories_Glasses_AttachChin.Id,
                externalAttachable.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Theory]
        [InlineData("@detach:Clothing/Hats=force")]
        [InlineData("@remattach:Clothing/Hats=force")]
        public void RemAttach_ByFolder(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage(command, _sender.Id, _sender.Name)
            );

            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id,
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Theory]
        [InlineData("@detach:groin=force")]
        [InlineData("@remattach:groin=force")]
        public void RemAttach_RemoveAttachmentPoint(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);

            var externalAttachable = new InventoryTree.InventoryItem(
                new Guid("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Groin Thing",
                AttachmentPoint.Groin,
                null,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
             );

            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage(command, _sender.Id, _sender.Name)
            );

            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                externalAttachable.Id
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Theory]
        [InlineData("@detach:skull=force")]
        [InlineData("@remattach:skull=force")]
        public void RemAttach_RemoveNone(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage(command, _sender.Id, _sender.Name)
            );

            var expected = new List<Guid>()
            {
            };

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Theory]
        [InlineData("detach")]
        [InlineData("remattach")]
        public void RemAttach_RemoveByUUID(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:{sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id}=force", _sender.Id, _sender.Name)
            );

            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id
            };

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Theory]
        [InlineData("detach")]
        [InlineData("remattach")]
        public void RemAttach_RemoveByUUID_External(string command)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);

            var externalWearable = new InventoryTree.InventoryItem(
                new Guid("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Tattoo",
                null,
                WearableType.Tattoo,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );
            var externalAttachable = new InventoryTree.InventoryItem(
                new Guid("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Jaw Thing",
                AttachmentPoint.Jaw,
                null,
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
            );

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:{sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id}=force", _sender.Id, _sender.Name)
            );

            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id
            };

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }
        #endregion

        #region @detachall:<folder1/.../folderN>=force
        [Fact]
        public void DetachAllForce_Recursive()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachall:Clothing=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing folder, and all of its subfolders will be removed
            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id,
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }
        #endregion

        #region @detachthis[:<attachpt> or <clothing_layer> or <uuid>]=force
        [Fact]
        public void DetachThisForce_Default()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachthis=force", sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, sampleTree.Root_Clothing_HappyShirt_AttachChest.Name)
            );

            // Everything under the clothing folder will be detached because happy shirt exists in the clothing folder
            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Fact]
        public void DetachThisForce_ByAttachmentPoint()
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
            //        \= Glasses (attached to 'chin') <--- Modified to be attached to chest

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = AttachmentPoint.Chest;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachthis:chest=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing and accessories folder will be detached, not recursive
            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                sampleTree.Root_Accessories_Glasses_AttachChin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Fact]
        public void DetachThisForce_ByWearableType()
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
            //        |= Watch (worn on 'tattoo')  <--- Modified to be worn on pants
            //        \= Glasses (attached to 'chin')

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = WearableType.Pants;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachthis:pants=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing and accessories folder will be detached, not recursive
            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                sampleTree.Root_Accessories_Glasses_AttachChin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Fact]
        public void DetachThisForce_ByWearableType_PrivateFolder()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing                 <--- Modified to be .Clothing
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
            //        |= Watch (worn on 'tattoo') <--- Modified to be worn on pants
            //        \= Glasses (attached to 'chin')

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            clothingFolder.Name = ".clothing";

            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = WearableType.Pants;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachthis:pants=force", _sender.Id, _sender.Name)
            );

            // Only accessories will be removed even though pants exist in our clothing folder. The clothing folder is private ".clothing"
            var expected = new List<Guid>()
            {
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                sampleTree.Root_Accessories_Glasses_AttachChin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }
        #endregion

        #region @detachallthis[:<attachpt> or <clothing_layer>]=force
        [Fact]
        public void DetachAllThisForce_Default()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachallthis=force", sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, sampleTree.Root_Clothing_HappyShirt_AttachChest.Name)
            );

            // Everything under the clothing folder (and its subfolders recursively) will be detached because happy shirt exists in the clothing folder
            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
                sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id,
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Fact]
        public void DetachThisAllForce_ByAttachmentPoint()
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
            //        \= Glasses (attached to 'chin') <--- Modified to be attached to chest

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = AttachmentPoint.Chest;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachallthis:chest=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing and accessories folder will be detached, and their subfolders recursively
            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
                sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id,
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                sampleTree.Root_Accessories_Glasses_AttachChin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }


        [Fact]
        public void DetachAllThisForce_ByWearableType()
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
            //        |= Watch (worn on 'tattoo')  <--- Modified to be worn on pants
            //        \= Glasses (attached to 'chin')

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = WearableType.Pants;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachallthis:pants=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing and accessories folder will be detached, recursive
            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
                sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id,
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                sampleTree.Root_Accessories_Glasses_AttachChin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }

        [Fact]
        public void DetachAllThisForce_ByWearableType_PrivateFolder()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing
            //  |    |= Business Pants (attached to 'groin')
            //  |    |= Happy Shirt (attached to 'chest')
            //  |    |= Retro Pants (worn on 'pants')
            //  |    \-Hats                     <--- Modified to be .Hats
            //  |        |
            //  |        |- Sub Hats
            //  |        |    \ (Empty)
            //  |        |
            //  |        |= Fancy Hat (attached to 'chin')
            //  |        \= Party Hat (attached to 'groin')
            //   \-Accessories
            //        |= Watch (worn on 'tattoo')  <--- Modified to be worn on pants
            //        \= Glasses (attached to 'chin')

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = ".hats";

            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = WearableType.Pants;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Commands.Detach += n,
                 detach: n => _rlv.Commands.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachallthis:pants=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing and accessories folder will be detached, recursive.
            //   Hats will be excluded because they are in a private folder ".hats"
            var expected = new List<Guid>()
            {
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                sampleTree.Root_Accessories_Glasses_AttachChin.Id,
            }.Order();

            Assert.Equal(expected, raised.Arguments.ItemIds.Order());
        }
        #endregion

        #region @detachthis[:<layer>|<attachpt>|<path_to_folder>]=<y/n>

        [Fact]
        public void DetachThis()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@detachthis=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void DetachThis_NotRecursive()
        {
            // TryGetRlvInventoryTree
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            // This should lock the #RLV/Clothing folder because the Business Pants are issuing the command, which is in the Clothing folder.
            //   Business Pants cannot be detached, but hats are still detachable.
            Assert.True(_rlv.ProcessMessage("@detachthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat ()
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
        public void DetachThis_ByPath()
        {
            // TryGetRlvInventoryTree
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            // This should lock the Hats folder, all hats are no longer detachable
            Assert.True(_rlv.ProcessMessage("@detachthis:Clothing/Hats=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void DetachThis_ByAttachmentPoint()
        {
            // TryGetRlvInventoryTree
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            // This should lock the Hats folder, all hats are no longer detachable
            Assert.True(_rlv.ProcessMessage("@detachthis:groin=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - folder was locked because PartyHat (groin)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED) - folder was locked because BusinessPants (groin)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void DetachThis_ByWornLayer()
        {
            // TryGetRlvInventoryTree
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            // This should lock the Hats folder, all hats are no longer detachable
            Assert.True(_rlv.ProcessMessage("@detachthis:tattoo=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses (LOCKED) - folder was locked from Watch (tattoo)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        #endregion

        #region @detachallthis[:<layer>|<attachpt>|<path_to_folder>]=<y/n>

        [Fact]
        public void DetachAllThis()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@detachallthis=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void DetachAllThis_Recursive()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@detachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

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
        public void DetachAllThis_Recursive_Path()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@detachallthis:Clothing=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

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
        public void DetachAllThis_Recursive_Worn()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@detachallthis:pants=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED) - Folder locked due to RetroPants being worn as 'pants'
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED) - Folder locked due to RetroPants being worn as 'pants'
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void DetachAllThis_Recursive_Attached()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@detachallthis:chest=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED) - Folder locked due to HappyShirt attachment of 'chest'
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED) - Folder locked due to HappyShirt attachment of 'chest'
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        #endregion

        #region @detachthis_except:<folder>=<rem/add>

        [Fact]
        public void DetachAllThis_Recursive_Except()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@detachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));
            Assert.True(_rlv.ProcessMessage($"@detachthis_except:Clothing/Hats=add", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()  - Locked, but folder has exception
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt () - Locked, but folder has exception
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants () - Locked, but folder has exception
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        #endregion

        #region @detachallthis_except:<folder>=<rem/add>

        [Fact]
        public void DetachAllThis_Recursive_ExceptAll()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@detachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));
            Assert.True(_rlv.ProcessMessage($"@detachallthis_except:Clothing/Hats=add", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

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
        public void DetachAllThis_Recursive_ExceptAll_Recursive()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@detachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));
            Assert.True(_rlv.ProcessMessage($"@detachallthis_except:Clothing=add", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

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

        #region @attachthis[:<layer>|<attachpt>|<path_to_folder>]=<y/n>
        [Fact]
        public void AttachThis()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@attachthis=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void AttachThis_NotRecursive()
        {
            // TryGetRlvInventoryTree
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            // This should lock the #RLV/Clothing folder because the Business Pants are issuing the command, which is in the Clothing folder.
            //   Business Pants cannot be attached, but hats are still attachable.
            Assert.True(_rlv.ProcessMessage("@attachthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void AttachThis_ByPath()
        {
            // TryGetRlvInventoryTree
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            // This should lock the Hats folder, all hats are no longer attachable
            Assert.True(_rlv.ProcessMessage("@attachthis:Clothing/Hats=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void AttachThis_ByAttachmentPoint()
        {
            // TryGetRlvInventoryTree
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            // This should lock the Hats folder, all hats are no longer attachable
            Assert.True(_rlv.ProcessMessage("@attachthis:groin=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - folder was locked because PartyHat (groin)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED) - folder was locked because BusinessPants (groin)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void AttachThis_ByWornLayer()
        {
            // TryGetRlvInventoryTree
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            // This should lock the Hats folder, all hats are no longer attachable
            Assert.True(_rlv.ProcessMessage("@attachthis:tattoo=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses (LOCKED) - folder was locked from Watch (tattoo)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }
        #endregion

        #region @attachallthis[:<layer>|<attachpt>|<path_to_folder>]=<y/n>

        [Fact]
        public void AttachAllThis()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@attachallthis=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void AttachAllThis_Recursive()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@attachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void AttachAllThis_Recursive_Path()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@attachallthis:Clothing=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void AttachAllThis_Recursive_Worn()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@attachallthis:pants=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED) - Folder locked due to RetroPants being worn as 'pants'
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED) - Folder locked due to RetroPants being worn as 'pants'
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void AttachAllThis_Recursive_Attached()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@attachallthis:chest=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED) - Folder locked due to HappyShirt attachment of 'chest'
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED) - Folder locked due to HappyShirt attachment of 'chest'
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        #endregion

        #region @attachthis_except:<folder>=<rem/add>

        [Fact]
        public void AttachAllThis_Recursive_Except()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@attachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));
            Assert.True(_rlv.ProcessMessage($"@attachthis_except:Clothing/Hats=add", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()  - Locked, but folder has exception
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt () - Locked, but folder has exception
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants () - Locked, but folder has exception
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        #endregion

        #region @attachallthis_except:<folder>=<rem/add>

        [Fact]
        public void AttachAllThis_Recursive_ExceptAll()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@attachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));
            Assert.True(_rlv.ProcessMessage($"@attachallthis_except:Clothing/Hats=add", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat () - Parent folder locked recursively, but has exception
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat () - Parent folder locked recursively, but has exception
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public void AttachAllThis_Recursive_ExceptAll_Recursive()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            Assert.True(_rlv.ProcessMessage("@attachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));
            Assert.True(_rlv.ProcessMessage($"@attachallthis_except:Clothing=add", sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat () - Parent folder locked recursively, but parent has recursive exception
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat () - Parent folder locked recursively, but parent has recursive exception
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()  - Locked, but folder has exception
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt () - Locked, but folder has exception
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants () - Locked, but folder has exception
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        #endregion
    }
}
