using LibRLV.EventArguments;
using Moq;

namespace LibRLV.Tests
{
    public class RestrictionsMiscTests : RestrictionsBase
    {


        #region General
        [Theory]
        [InlineData(1234, RLV.RLVVersion)]
        [InlineData(-1234, RLV.RLVVersion)]
        public void CheckChannelResponseGood(int channel, string expectedReply)
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage($"@versionnew={channel}", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (channel, expectedReply),
            };

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("@versionnew=1234a")]
        [InlineData("@versionnew=0")]
        [InlineData("@versionnew=2147483648")]
        [InlineData("@versionnew=-2147483649")]
        public void CheckChannelResponseBad(string command)
        {
            Assert.False(_rlv.ProcessMessage(command, _sender.Id, _sender.Name));
            _callbacks.VerifyNoOtherCalls();
        }
        #endregion

        //
        // Version Checking
        //

        #region @version Manual
        [Fact]
        public void ManualVersion()
        {
            _rlv.EnableInstantMessageProcessing = true;

            _rlv.ProcessInstantMessage("@version", _sender.Id, _sender.Name);

            _callbacks.Verify(c =>
                c.SendInstantMessageAsync(_sender.Id, RLV.RLVVersion, It.IsAny<CancellationToken>()),
                Times.Once);

            _callbacks.VerifyNoOtherCalls();
        }
        #endregion

        #region @version @versionnew @versionnum

        [Theory]
        [InlineData("@version", 1234, RLV.RLVVersion)]
        [InlineData("@versionnew", 1234, RLV.RLVVersion)]
        [InlineData("@versionnum", 1234, RLV.RLVVersionNum)]
        public void CheckVersions(string command, int channel, string expectedResponse)
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage($"{command}={channel}", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, expectedResponse),
            };

            Assert.Equal(expected, actual);
        }
        #endregion

        //
        // Blacklist handling
        //

        #region @versionnumbl=<channel_number>

        [Theory]
        [InlineData("", RLV.RLVVersionNum)]
        [InlineData("sendim,recvim", RLV.RLVVersionNum + ",recvim,sendim")]
        public void VersionNumBL(string seed, string expectedResponse)
        {
            var actual = _callbacks.RecordReplies();
            SeedBlacklist(seed);

            _rlv.ProcessMessage("@versionnumbl=1234", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, expectedResponse),
            };

            Assert.Equal(expected, actual);
        }
        #endregion

        #region @getblacklist[:filter]=<channel_number>
        [Theory]
        [InlineData("@getblacklist", 1234, "sendim,recvim", "recvim,sendim")]
        [InlineData("@getblacklist:im", 1234, "sendim,recvim", "recvim,sendim")]
        [InlineData("@getblacklist:send", 1234, "sendim,recvim", "sendim")]
        [InlineData("@getblacklist:tpto", 1234, "sendim,recvim", "")]
        [InlineData("@getblacklist", 1234, "", "")]
        public void GetBlacklist(string command, int channel, string seed, string expectedResponse)
        {
            var actual = _callbacks.RecordReplies();
            SeedBlacklist(seed);

            _rlv.ProcessMessage($"{command}={channel}", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (channel, expectedResponse),
            };

            Assert.Equal(expected, actual);
        }
        #endregion

        #region @getblacklist Manual
        private void SeedBlacklist(string seed)
        {
            var blacklistEntries = seed.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in blacklistEntries)
            {
                _rlv.Blacklist.BlacklistCommand(item);
            }
        }

        [Theory]
        [InlineData("@getblacklist", "sendim,recvim", "recvim,sendim")]
        [InlineData("@getblacklist", "", "")]
        public void ManualBlacklist(string command, string seed, string expected)
        {
            _rlv.EnableInstantMessageProcessing = true;

            SeedBlacklist(seed);

            _rlv.ProcessInstantMessage(command, _sender.Id, _sender.Name);

            _callbacks.Verify(c =>
                c.SendInstantMessageAsync(_sender.Id, expected, It.IsAny<CancellationToken>()),
                Times.Once);

            _callbacks.VerifyNoOtherCalls();
        }
        #endregion


        //
        // Miscellaneous
        //

        #region @Notify
        [Fact]
        public void Notify()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@alwaysrun=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendim:group_name=add", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/notify:1234=n"),
                (1234, "/sendim=n"),
                (1234, "/alwaysrun=n"),
                (1234, "/sendim:group_name=n"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifyFiltered()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@notify:1234;run=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@alwaysrun=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendim:group_name=add", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/alwaysrun=n"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifyMultiCommand()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendim=n,sendim:group_name=add,alwaysrun=n", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/notify:1234=n"),
                (1234, "/sendim=n"),
                (1234, "/sendim:group_name=n"),
                (1234, "/alwaysrun=n"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifyMultiChannels()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:12345=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234,  "/notify:1234=n"),
                (1234,  "/notify:12345=n"),
                (12345, "/notify:12345=n"),
                (1234,  "/sendim=n"),
                (12345, "/sendim=n"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifyMultiChannelsFiltered()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:12345;im=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234,  "/notify:1234=n"),
                (1234,  "/notify:12345;im=n"),
                (1234,  "/sendim=n"),
                (12345, "/sendim=n"),
            };

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("@camdistmax:123=n", "/camdistmax:123=n")]
        [InlineData("@setcam_avdistmax:123=n", "/setcam_avdistmax:123=n")]
        [InlineData("@camdistmin:123=n", "/camdistmin:123=n")]
        [InlineData("@setcam_avdistmin:123=n", "/setcam_avdistmin:123=n")]
        [InlineData("@camunlock=n", "/camunlock=n")]
        [InlineData("@setcam_unlock=n", "/setcam_unlock=n")]
        [InlineData("@camtextures:1cdbc6a2-ae6b-3130-9348-3d3b1ca84c53=n", "/camtextures:1cdbc6a2-ae6b-3130-9348-3d3b1ca84c53=n")]
        [InlineData("@touchfar:5=n", "/touchfar:5=n")]
        [InlineData("@fartouch:5=n", "/fartouch:5=n")]
        public void NotifySynonyms(string command, string expectedReply)
        {
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage(command, _sender.Id, _sender.Name);

            _callbacks.Verify(c => c.SendReplyAsync(1234, "/notify:1234=n", It.IsAny<CancellationToken>()), Times.Once);
            _callbacks.Verify(c => c.SendReplyAsync(1234, expectedReply, It.IsAny<CancellationToken>()), Times.Once);

            _callbacks.VerifyNoOtherCalls();
        }

        [Fact]
        public void NotifyClear_Filtered()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@fly=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@clear=fly", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/notify:1234=n"),
                (1234, "/fly=n"),
                // Begin processing clear()...
                (1234, "/fly=y"),
                (1234, "/clear:fly"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifyClear()
        {
            var actual = _callbacks.RecordReplies();

            var sender2Id = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@notify:1234=add", sender2Id, "Main");
            _rlv.ProcessMessage("@fly=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@clear", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/notify:1234=n"),
                (1234, "/fly=n"),
                // Begin processing clear()...
                (1234, "/fly=y"),
                (1234, "/clear"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifyInventoryOffer()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportInventoryOffer("#RLV/~MyCuffs", RLV.InventoryOfferAction.Accepted);
            _rlv.ReportInventoryOffer("Objects/New Folder (3)", RLV.InventoryOfferAction.Accepted);
            _rlv.ReportInventoryOffer("#RLV/Foo/Bar", RLV.InventoryOfferAction.Denied);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/notify:1234=n"),
                (1234, "/accepted_in_rlv inv_offer ~MyCuffs"),
                (1234, "/accepted_in_inv inv_offer Objects/New Folder (3)"),
                (1234, "/declined inv_offer Foo/Bar"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifySitStandLegal()
        {
            var actual = _callbacks.RecordReplies();

            var sitTarget = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportSit(RLV.SitType.Sit, sitTarget, 1.0f);
            _rlv.ReportSit(RLV.SitType.Stand, sitTarget, 0);
            _rlv.ReportSit(RLV.SitType.Sit, null, null);
            _rlv.ReportSit(RLV.SitType.Stand, null, null);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/notify:1234=n"),
                (1234, $"/sat object legally {sitTarget}"),
                (1234, $"/unsat object legally {sitTarget}"),
                (1234, $"/sat ground legally"),
                (1234, $"/unsat ground legally"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifySitStandWithRestrictions()
        {
            var actual = _callbacks.RecordReplies();

            var sitTarget = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@sit=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);

            _rlv.ReportSit(RLV.SitType.Sit, sitTarget, 1.0f);
            _rlv.ReportSit(RLV.SitType.Stand, sitTarget, 1.0f);
            _rlv.ReportSit(RLV.SitType.Sit, null, null);
            _rlv.ReportSit(RLV.SitType.Stand, null, null);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/notify:1234=n"),
                (1234, $"/sat object illegally {sitTarget}"),
                (1234, $"/unsat object illegally {sitTarget}"),
                (1234, $"/sat ground illegally"),
                (1234, $"/unsat ground illegally"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifySitStandWithDistanceRestrictions()
        {
            var actual = _callbacks.RecordReplies();

            var sitTarget = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@sittp=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);

            _rlv.ReportSit(RLV.SitType.Sit, sitTarget, 100.0f);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/notify:1234=n"),
                (1234, $"/sat object illegally {sitTarget}"),
            };

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void NotifyWear()
        {
            var actual = _callbacks.RecordReplies();
            var wornItem = new RlvObject("TargetItem", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var folderId1 = new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb");
            var folderId2 = new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc");

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportWornItemChange(folderId1, false, WearableType.Skin, RLV.WornItemChange.Attached);
            _rlv.ReportWornItemChange(folderId2, true, WearableType.Tattoo, RLV.WornItemChange.Attached);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/notify:1234=n"),
                (1234, $"/worn legally skin"),
                (1234, $"/worn legally tattoo"),
            };

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void NotifyWear_Illegal()
        {
            var actual = _callbacks.RecordReplies();
            var wornItem = new RlvObject("TargetItem", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var itemId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            _rlv.ProcessMessage("@addoutfit:skin=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportWornItemChange(itemId1, false, WearableType.Skin, RLV.WornItemChange.Attached);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/notify:1234=n"),
                (1234, $"/worn illegally skin"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifyUnWear()
        {
            var actual = _callbacks.RecordReplies();
            var wornItem = new RlvObject("TargetItem", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var folderId1 = new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb");
            var folderId2 = new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc");

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportWornItemChange(folderId1, false, WearableType.Skin, RLV.WornItemChange.Detached);
            _rlv.ReportWornItemChange(folderId2, true, WearableType.Tattoo, RLV.WornItemChange.Detached);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/notify:1234=n"),
                (1234, $"/unworn legally skin"),
                (1234, $"/unworn legally tattoo"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifyUnWear_illegal()
        {
            var actual = _callbacks.RecordReplies();
            var wornItem = new RlvObject("TargetItem", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var itemId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            _rlv.ProcessMessage("@remoutfit:skin=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);

            _rlv.ReportWornItemChange(itemId1, false, WearableType.Skin, RLV.WornItemChange.Detached);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/notify:1234=n"),
                (1234, $"/unworn illegally skin"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifyAttached()
        {
            var actual = _callbacks.RecordReplies();
            var wornItem = new RlvObject("TargetItem", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var itemId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");
            var itemId2 = new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb");

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportAttachedItemChange(itemId1, false, AttachmentPoint.Chest, RLV.AttachedItemChange.Attached);
            _rlv.ReportAttachedItemChange(itemId2, true, AttachmentPoint.Skull, RLV.AttachedItemChange.Attached);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/notify:1234=n"),
                (1234, $"/attached legally chest"),
                (1234, $"/attached legally skull"),
            };

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void NotifyAttached_Illegal()
        {
            var actual = _callbacks.RecordReplies();
            var wornItem = new RlvObject("TargetItem", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var itemId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            _rlv.ProcessMessage("@addattach:chest=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportAttachedItemChange(itemId1, false, AttachmentPoint.Chest, RLV.AttachedItemChange.Attached);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/notify:1234=n"),
                (1234, $"/attached illegally chest"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifyDetached()
        {
            var actual = _callbacks.RecordReplies();
            var wornItem = new RlvObject("TargetItem", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var folderId1 = new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb");
            var folderId2 = new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc");

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportAttachedItemChange(folderId1, false, AttachmentPoint.Chest, RLV.AttachedItemChange.Detached);
            _rlv.ReportAttachedItemChange(folderId2, true, AttachmentPoint.Skull, RLV.AttachedItemChange.Detached);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/notify:1234=n"),
                (1234, $"/detached legally chest"),
                (1234, $"/detached legally skull"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotifyDetached_Illegal()
        {
            var actual = _callbacks.RecordReplies();
            var wornItem = new RlvObject("TargetItem", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var itemId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            _rlv.ProcessMessage("@remattach:chest=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportAttachedItemChange(itemId1, false, AttachmentPoint.Chest, RLV.AttachedItemChange.Detached);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/notify:1234=n"),
                (1234, $"/detached illegally chest"),
            };

            Assert.Equal(expected, actual);
        }
        #endregion

        #region @Permissive
        [Fact]
        public void Permissive_On()
        {
            _rlv.ProcessMessage("@permissive=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.IsPermissive());
        }

        [Fact]
        public void Permissive_Off()
        {
            _rlv.ProcessMessage("@permissive=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@permissive=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsPermissive());
        }
        #endregion

        #region @Clear

        [Fact]
        public void Clear()
        {
            _rlv.ProcessMessage("@tploc=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplm=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@fly=n", _sender.Id, _sender.Name);

            _rlv.ProcessMessage("@clear", _sender.Id, _sender.Name);

            var restrictions = _rlv.RestrictionsHandler.GetRestrictions();
            Assert.Empty(restrictions);
        }

        [Fact]
        public void Clear_SenderBased()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@tploc=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplm=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@unsit=n", sender2.Id, sender2.Name);
            _rlv.ProcessMessage("@fly=n", sender2.Id, sender2.Name);

            _rlv.ProcessMessage("@clear", sender2.Id, sender2.Name);

            Assert.False(_rlv.Restrictions.CanTpLoc());
            Assert.False(_rlv.Restrictions.CanTpLm());
            Assert.True(_rlv.Restrictions.CanUnsit());
            Assert.True(_rlv.Restrictions.CanFly());
        }

        [Fact]
        public void Clear_Filtered()
        {
            _rlv.ProcessMessage("@tploc=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplm=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@fly=n", _sender.Id, _sender.Name);

            _rlv.ProcessMessage("@clear=tp", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanTpLoc());
            Assert.True(_rlv.Restrictions.CanTpLm());
            Assert.False(_rlv.Restrictions.CanUnsit());
            Assert.False(_rlv.Restrictions.CanFly());
        }
        #endregion

        #region @getstatus

        [Fact]
        public void GetStatus()
        {
            var actual = _callbacks.RecordReplies();
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@fly=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplure=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplure:3d6181b0-6a4b-97ef-18d8-722652995cf1=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplocal=n", sender2.Id, sender2.Name);

            _rlv.ProcessMessage("@getstatus=1234", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/fly/tplure/tplure:3d6181b0-6a4b-97ef-18d8-722652995cf1"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStatus_filtered()
        {
            var actual = _callbacks.RecordReplies();
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@fly=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplure=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplure:3d6181b0-6a4b-97ef-18d8-722652995cf1=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplocal=n", sender2.Id, sender2.Name);

            _rlv.ProcessMessage("@getstatus:tp=1234", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/tplure/tplure:3d6181b0-6a4b-97ef-18d8-722652995cf1"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStatus_customSeparator()
        {
            var actual = _callbacks.RecordReplies();
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@fly=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplure=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplure:3d6181b0-6a4b-97ef-18d8-722652995cf1=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplocal=n", sender2.Id, sender2.Name);

            _rlv.ProcessMessage("@getstatus:; ! =1234", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $" ! fly ! tplure ! tplure:3d6181b0-6a4b-97ef-18d8-722652995cf1"),
            };

            Assert.Equal(expected, actual);
        }

        #endregion

        #region @getstatusall

        [Fact]
        public void GetStatusAll()
        {
            var actual = _callbacks.RecordReplies();
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@fly=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplure=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplure:3d6181b0-6a4b-97ef-18d8-722652995cf1=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplocal=n", sender2.Id, sender2.Name);

            _rlv.ProcessMessage("@getstatusall=1234", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, $"/fly/tplure/tplure:3d6181b0-6a4b-97ef-18d8-722652995cf1/tplocal"),
            };

            Assert.Equal(expected, actual);
        }

        #endregion

        //
        // Movement
        //

        //
        // Camera and view
        //

        //
        // Chat, Emotes and Instant Messages
        //

        //
        // Teleportation
        //


        //
        // Inventory, Editing and Rezzing
        //

        #region @showinv
        [Fact]
        public void CanShowInv()
        {
            CheckSimpleCommand("showInv", m => m.CanShowInv());
        }

        #endregion

        #region @viewNote
        [Fact]
        public void CanViewNote()
        {
            CheckSimpleCommand("viewNote", m => m.CanViewNote());
        }
        #endregion

        #region @viewscript
        [Fact]
        public void CanViewScript()
        {
            CheckSimpleCommand("viewScript", m => m.CanViewScript());
        }
        #endregion

        #region @viewtexture
        [Fact]
        public void CanViewTexture()
        {
            CheckSimpleCommand("viewTexture", m => m.CanViewTexture());
        }
        #endregion

        #region @edit @editobj @editworld @editattach

        [Fact]
        public void CanEdit_Default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));
        }

        [Fact]
        public void CanEditFolderNameSpecifiesToAddInsteadOfReplace()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@edit=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));
        }

        [Fact]
        public void CanEdit_Exception()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@edit=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@edit:{objectId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));

            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, objectId2));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, objectId2));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId2));
        }

        [Fact]
        public void CanEdit_Specific()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@editobj:{objectId1}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));

            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, objectId2));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, objectId2));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId2));
        }

        [Fact]
        public void CanEdit_World()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage($"@editworld=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));
        }

        [Fact]
        public void CanEdit_Attachment()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage($"@editattach=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.True(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));
        }

        #endregion

        #region @canrez
        [Fact]
        public void CanRez()
        {
            CheckSimpleCommand("rez", m => m.CanRez());
        }

        #endregion

        #region @share @share_sec

        [Fact]
        public void CanShare_Default()
        {
            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            Assert.True(_rlv.Restrictions.CanShare(null));
            Assert.True(_rlv.Restrictions.CanShare(userId1));
        }

        [Fact]
        public void CanShare()
        {
            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            _rlv.ProcessMessage("@share=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShare(null));
            Assert.False(_rlv.Restrictions.CanShare(userId1));
        }

        [Fact]
        public void CanShare_Except()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@share=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@share:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShare(null));
            Assert.True(_rlv.Restrictions.CanShare(userId1));
            Assert.False(_rlv.Restrictions.CanShare(userId2));
        }

        [Fact]
        public void CanShare_Secure_Default()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@share_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShare(null));
            Assert.False(_rlv.Restrictions.CanShare(userId1));
            Assert.False(_rlv.Restrictions.CanShare(userId2));
        }

        [Fact]
        public void CanShare_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@share_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@share:{userId1}=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@share:{userId2}=add", sender2.Id, sender2.Name);

            Assert.False(_rlv.Restrictions.CanShare(null));
            Assert.True(_rlv.Restrictions.CanShare(userId1));
            Assert.False(_rlv.Restrictions.CanShare(userId2));
        }

        #endregion

        //
        // Sitting
        //


        //
        // Clothing and Attachments
        //

        //
        // Touch
        //

        #region @interact=<y/n>

        [Fact]
        public void CanInteract()
        {
            CheckSimpleCommand("interact", m => m.CanInteract());
        }

        [Fact]
        public void CanInteract_default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId1 = new Guid("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage($"@interact=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));

            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));
            Assert.False(_rlv.Restrictions.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));

            Assert.False(_rlv.Restrictions.CanRez());

            Assert.False(_rlv.Restrictions.CanSit());
        }

        #endregion

        //
        // Location
        //

        #region  @showworldmap=<y/n>
        [Fact]
        public void CanShowWorldMap()
        {
            CheckSimpleCommand("showWorldMap", m => m.CanShowWorldMap());
        }
        #endregion

        #region @showminimap=<y/n>
        [Fact]
        public void CanShowMiniMap()
        {
            CheckSimpleCommand("showMiniMap", m => m.CanShowMiniMap());
        }
        #endregion

        #region @showloc=<y/n>
        [Fact]
        public void CanShowLoc()
        {
            CheckSimpleCommand("showLoc", m => m.CanShowLoc());
        }
        #endregion

        //
        // Name Tags and Hovertext
        //

        #region @shownames[:except_uuid]=<y/n> @shownames_sec[:except_uuid]=<y/n>

        [Fact]
        public void CanShowNames_Default()
        {
            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            Assert.True(_rlv.Restrictions.CanShowNames(null));
            Assert.True(_rlv.Restrictions.CanShowNames(userId1));
        }

        [Fact]
        public void CanShowNames()
        {
            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            _rlv.ProcessMessage("@shownames=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowNames(null));
            Assert.False(_rlv.Restrictions.CanShowNames(userId1));
        }

        [Fact]
        public void CanShowNames_Except()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@shownames=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@shownames:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowNames(null));
            Assert.True(_rlv.Restrictions.CanShowNames(userId1));
            Assert.False(_rlv.Restrictions.CanShowNames(userId2));
        }

        [Fact]
        public void CanShowNames_Secure_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@shownames_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowNames(null));
            Assert.False(_rlv.Restrictions.CanShowNames(userId1));
            Assert.False(_rlv.Restrictions.CanShowNames(userId2));
        }

        [Fact]
        public void CanShowNames_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("22222222-2222-4222-8222-222222222222"));
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@shownames_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@shownames:{userId1}=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@shownames:{userId2}=add", sender2.Id, sender2.Name);

            Assert.False(_rlv.Restrictions.CanShowNames(null));
            Assert.True(_rlv.Restrictions.CanShowNames(userId1));
            Assert.False(_rlv.Restrictions.CanShowNames(userId2));
        }

        #endregion

        #region @shownametags[:uuid]=<y/n>
        // TODO: Add distance to option
        [Fact]
        public void CanShowNameTags_Default()
        {
            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            Assert.True(_rlv.Restrictions.CanShowNameTags(null));
            Assert.True(_rlv.Restrictions.CanShowNameTags(userId1));
        }

        [Fact]
        public void CanShowNameTags()
        {
            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            _rlv.ProcessMessage("@shownametags=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowNameTags(null));
            Assert.False(_rlv.Restrictions.CanShowNameTags(userId1));
        }

        [Fact]
        public void CanShowNameTags_Except()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@shownametags=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@shownametags:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowNameTags(null));
            Assert.True(_rlv.Restrictions.CanShowNameTags(userId1));
            Assert.False(_rlv.Restrictions.CanShowNameTags(userId2));
        }

        #endregion

        #region @shownearby=<y/n>
        [Fact]
        public void CanShowNearby()
        {
            CheckSimpleCommand("showNearby", m => m.CanShowNearby());
        }
        #endregion

        #region @showhovertextall=<y/n>

        [Fact]
        public void CanShowHoverTextAll_Default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverTextAll()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@showhovertextall=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.False(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        #endregion

        #region @showhovertext:<Guid>=<y/n>

        [Fact]
        public void CanShowHoverText_Default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverText()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@showhovertext:{objectId1}=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.False(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));

            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId2));
            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId2));
        }

        #endregion

        #region @showhovertexthud=<y/n>

        [Fact]
        public void CanShowHoverTextHud_Default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverTextHud()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@showhovertexthud=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.False(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));

            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId2));
            Assert.False(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId2));
        }

        #endregion

        #region @showhovertextworld=<y/n>

        [Fact]
        public void CanShowHoverTextWorld_Default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverTextWorld()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@showhovertextworld=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));

            Assert.False(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId2));
            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId2));
        }

        #endregion

        //
        // Group
        //

        #region @setgroup:<uuid|group_name>[;<role>]=force

        [Fact]
        public void SetGroup_ByName()
        {
            var raised = Assert.Raises<SetGroupEventArgs>(
                 attach: n => _rlv.Actions.SetGroup += n,
                 detach: n => _rlv.Actions.SetGroup -= n,
                 testCode: () => _rlv.ProcessMessage("@setgroup:Group Name=force", _sender.Id, _sender.Name)
            );

            Assert.Equal("Group Name", raised.Arguments.GroupName);
            Assert.Equal(Guid.Empty, raised.Arguments.GroupId);
            Assert.Equal(string.Empty, raised.Arguments.Role);
        }

        [Fact]
        public void SetGroup_ByNameAndRole()
        {
            var raised = Assert.Raises<SetGroupEventArgs>(
                 attach: n => _rlv.Actions.SetGroup += n,
                 detach: n => _rlv.Actions.SetGroup -= n,
                 testCode: () => _rlv.ProcessMessage("@setgroup:Group Name;Admin Role=force", _sender.Id, _sender.Name)
            );

            Assert.Equal("Group Name", raised.Arguments.GroupName);
            Assert.Equal("Admin Role", raised.Arguments.Role);
            Assert.Equal(Guid.Empty, raised.Arguments.GroupId);
        }

        [Fact]
        public void SetGroup_ById()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");

            var raised = Assert.Raises<SetGroupEventArgs>(
                 attach: n => _rlv.Actions.SetGroup += n,
                 detach: n => _rlv.Actions.SetGroup -= n,
                 testCode: () => _rlv.ProcessMessage($"@setgroup:{objectId1}=force", _sender.Id, _sender.Name)
            );

            Assert.Equal(string.Empty, raised.Arguments.GroupName);
            Assert.Equal(objectId1, raised.Arguments.GroupId);
            Assert.Equal(string.Empty, raised.Arguments.Role);
        }

        #endregion

        #region @setgroup=<y/n>
        [Fact]
        public void CanSetGroup()
        {
            CheckSimpleCommand("setGroup", m => m.CanSetGroup());
        }
        #endregion

        #region @getgroup=<channel_number>

        [Fact]
        public void GetGroup_Default()
        {
            var actual = _callbacks.RecordReplies();
            var actualGroupName = "Group Name";

            _callbacks.Setup(e =>
                e.TryGetGroup(out actualGroupName)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, actualGroupName),
            };

            Assert.True(_rlv.ProcessMessage("@getgroup=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetGroup_NoGroup()
        {
            var actual = _callbacks.RecordReplies();
            var actualGroupName = "";

            _callbacks.Setup(e =>
                e.TryGetGroup(out actualGroupName)
            ).ReturnsAsync(false);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "none"),
            };

            Assert.True(_rlv.ProcessMessage("@getgroup=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        #endregion

        //
        // Viewer Control
        //

        //
        // Unofficial Commands
        //

        #region @allowidle=<y/n>
        [Fact]
        public void CanAllowIdle()
        {
            CheckSimpleCommand("allowIdle", m => m.CanAllowIdle());
        }
        #endregion
    }
}
