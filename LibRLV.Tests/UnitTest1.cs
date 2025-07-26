using LibRLV.EventArguments;
using Moq;
using OpenMetaverse;

namespace LibRLV.Tests
{
    public class UnitTest1
    {
        public record RlvObject(string Name, UUID Id);

        private readonly RlvObject _sender;
        private readonly Mock<IRLVCallbacks> _callbacks;
        private readonly RLV _rlv;

        public UnitTest1()
        {
            _sender = new RlvObject("Sender 1", new UUID("ffffffff-ffff-4fff-8fff-ffffffffffff"));
            _callbacks = new Mock<IRLVCallbacks>();
            _rlv = new RLV(_callbacks.Object, true);
        }

        #region General
        [Theory]
        [InlineData("@versionnew=1234", RLV.RLVVersion)]
        [InlineData("@versionnew=-1234", RLV.RLVVersion)]
        public void CheckChannelResponseGood(string command, string expectedReply)
        {
            var expectedChannel = int.Parse(command.Substring(command.IndexOf('=') + 1));

            _rlv.ProcessMessage(command, _sender.Id, _sender.Name);

            _callbacks.Verify(c =>
                c.SendReplyAsync(expectedChannel, expectedReply, It.IsAny<CancellationToken>()),
                Times.Once);

            _callbacks.VerifyNoOtherCalls();
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

        [Theory]
        [InlineData("@version")]
        [InlineData("@getblacklist")]
        public void CheckInstantMessageProcessingOff(string command)
        {
            Assert.False(_rlv.ProcessInstantMessage(command, _sender.Id, _sender.Name));
            _callbacks.VerifyNoOtherCalls();
        }
        #endregion

        #region Version
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

        [Theory]
        [InlineData("@version=1234", RLV.RLVVersion)]
        [InlineData("@versionnew=1234", RLV.RLVVersion)]
        [InlineData("@versionnum=1234", RLV.RLVVersionNum)]
        public void CheckVersions(string command, string expectedReply)
        {
            var expectedChannel = int.Parse(command.Substring(command.IndexOf('=') + 1));

            _rlv.ProcessMessage(command, _sender.Id, _sender.Name);

            _callbacks.Verify(c =>
                c.SendReplyAsync(expectedChannel, expectedReply, It.IsAny<CancellationToken>()),
                Times.Once);

            _callbacks.VerifyNoOtherCalls();
        }
        #endregion

        #region Blacklist
        private void SeedBlacklist(string seed)
        {
            var blacklistEntries = seed.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in blacklistEntries)
            {
                _rlv.Blacklist.BlacklistCommand(item);
            }
        }

        [Theory]
        [InlineData("@getblacklist", "sendim,recvim", "sendim,recvim")]
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

        [Theory]
        [InlineData("@getblacklist=1234", "sendim,recvim", "sendim,recvim")]
        [InlineData("@getblacklist:im=1234", "sendim,recvim", "sendim,recvim")]
        [InlineData("@getblacklist:send=1234", "sendim,recvim", "sendim")]
        [InlineData("@getblacklist:tpto=1234", "sendim,recvim", "")]
        [InlineData("@getblacklist=1234", "", "")]
        public void GetBlacklist(string command, string seed, string expected)
        {
            var expectedChannel = int.Parse(command.Substring(command.IndexOf('=') + 1));
            SeedBlacklist(seed);

            _rlv.ProcessMessage(command, _sender.Id, _sender.Name);

            _callbacks.Verify(c =>
                c.SendReplyAsync(expectedChannel, expected, It.IsAny<CancellationToken>()),
                Times.Once);

            _callbacks.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("@versionnumbl=1234", "", RLV.RLVVersionNum)]
        [InlineData("@versionnumbl=1234", "sendim,recvim", RLV.RLVVersionNum + ",sendim,recvim")]
        public void VersionNumBL(string command, string seed, string expected)
        {
            var expectedChannel = int.Parse(command.Substring(command.IndexOf('=') + 1));

            SeedBlacklist(seed);

            _rlv.ProcessMessage(command, _sender.Id, _sender.Name);

            _callbacks.Verify(c =>
                c.SendReplyAsync(expectedChannel, expected, It.IsAny<CancellationToken>()),
                Times.Once);

            _callbacks.VerifyNoOtherCalls();
        }
        #endregion

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

            _rlv.ProcessMessage("@notify:1234=add", UUID.Random(), "Main");
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
            _rlv.RLVManager.ReportInventoryOffer("#RLV/~MyCuffs", RLVManager.InventoryOfferAction.Accepted);
            _rlv.RLVManager.ReportInventoryOffer("Objects/New Folder (3)", RLVManager.InventoryOfferAction.Accepted);
            _rlv.RLVManager.ReportInventoryOffer("#RLV/Foo/Bar", RLVManager.InventoryOfferAction.Denied);

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

            var sitTarget = UUID.Random();

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.RLVManager.ReportSit(RLVManager.SitType.Sit, sitTarget, 1.0f);
            _rlv.RLVManager.ReportSit(RLVManager.SitType.Stand, sitTarget, 0);
            _rlv.RLVManager.ReportSit(RLVManager.SitType.Sit, null, null);
            _rlv.RLVManager.ReportSit(RLVManager.SitType.Stand, null, null);

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

            var sitTarget = UUID.Random();

            _rlv.ProcessMessage("@sit=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);

            _rlv.RLVManager.ReportSit(RLVManager.SitType.Sit, sitTarget, 1.0f);
            _rlv.RLVManager.ReportSit(RLVManager.SitType.Stand, sitTarget, 1.0f);
            _rlv.RLVManager.ReportSit(RLVManager.SitType.Sit, null, null);
            _rlv.RLVManager.ReportSit(RLVManager.SitType.Stand, null, null);

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

            var sitTarget = UUID.Random();

            _rlv.ProcessMessage("@sittp=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);

            _rlv.RLVManager.ReportSit(RLVManager.SitType.Sit, sitTarget, 100.0f);

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
            var wornItem = new RlvObject("TargetItem", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.RLVManager.ReportWornItemChange(wornItem.Id, UUID.Random(), false, WearableType.Skin, RLVManager.WornItemChange.Attached);
            _rlv.RLVManager.ReportWornItemChange(wornItem.Id, UUID.Random(), true, WearableType.Tattoo, RLVManager.WornItemChange.Attached);

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
            var wornItem = new RlvObject("TargetItem", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@addoutfit:skin=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.RLVManager.ReportWornItemChange(wornItem.Id, UUID.Random(), false, WearableType.Skin, RLVManager.WornItemChange.Attached);

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
            var wornItem = new RlvObject("TargetItem", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.RLVManager.ReportWornItemChange(wornItem.Id, UUID.Random(), false, WearableType.Skin, RLVManager.WornItemChange.Detached);
            _rlv.RLVManager.ReportWornItemChange(wornItem.Id, UUID.Random(), true, WearableType.Tattoo, RLVManager.WornItemChange.Detached);

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
            var wornItem = new RlvObject("TargetItem", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@remoutfit:skin=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);

            _rlv.RLVManager.ReportWornItemChange(wornItem.Id, UUID.Random(), false, WearableType.Skin, RLVManager.WornItemChange.Detached);

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
            var wornItem = new RlvObject("TargetItem", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.RLVManager.ReportAttachedItemChange(wornItem.Id, UUID.Random(), false, AttachmentPoint.Chest, RLVManager.AttachedItemChange.Attached);
            _rlv.RLVManager.ReportAttachedItemChange(wornItem.Id, UUID.Random(), true, AttachmentPoint.Skull, RLVManager.AttachedItemChange.Attached);

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
            var wornItem = new RlvObject("TargetItem", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@addattach:chest=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.RLVManager.ReportAttachedItemChange(wornItem.Id, UUID.Random(), false, AttachmentPoint.Chest, RLVManager.AttachedItemChange.Attached);

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
            var wornItem = new RlvObject("TargetItem", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.RLVManager.ReportAttachedItemChange(wornItem.Id, UUID.Random(), false, AttachmentPoint.Chest, RLVManager.AttachedItemChange.Detached);
            _rlv.RLVManager.ReportAttachedItemChange(wornItem.Id, UUID.Random(), true, AttachmentPoint.Skull, RLVManager.AttachedItemChange.Detached);

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
            var wornItem = new RlvObject("TargetItem", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@remattach:chest=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.RLVManager.ReportAttachedItemChange(wornItem.Id, UUID.Random(), false, AttachmentPoint.Chest, RLVManager.AttachedItemChange.Detached);

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

            Assert.False(_rlv.RLVManager.IsPermissive());
        }

        [Fact]
        public void Permissive_Off()
        {
            _rlv.ProcessMessage("@permissive=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@permissive=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.IsPermissive());
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

            var restrictions = _rlv.Restrictions.GetRestrictions();
            Assert.Empty(restrictions);
        }

        [Fact]
        public void Clear_SenderBased()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@tploc=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplm=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@unsit=n", sender2.Id, sender2.Name);
            _rlv.ProcessMessage("@fly=n", sender2.Id, sender2.Name);

            _rlv.ProcessMessage("@clear", sender2.Id, sender2.Name);

            Assert.False(_rlv.RLVManager.CanTpLoc());
            Assert.False(_rlv.RLVManager.CanTpLm());
            Assert.True(_rlv.RLVManager.CanUnsit());
            Assert.True(_rlv.RLVManager.CanFly());
        }

        [Fact]
        public void Clear_Filtered()
        {
            _rlv.ProcessMessage("@tploc=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@tplm=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@fly=n", _sender.Id, _sender.Name);

            _rlv.ProcessMessage("@clear=tp", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanTpLoc());
            Assert.True(_rlv.RLVManager.CanTpLm());
            Assert.False(_rlv.RLVManager.CanUnsit());
            Assert.False(_rlv.RLVManager.CanFly());
        }
        #endregion

        #region @GetStatus

        [Fact]
        public void GetStatus()
        {
            var actual = _callbacks.RecordReplies();
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

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
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

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
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

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

        #region @GetStatusAll

        [Fact]
        public void GetStatusAll()
        {
            var actual = _callbacks.RecordReplies();
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

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

        #region SimpleBooleanFlags

        private void CheckSimpleCommand(string cmd, Func<RLVManager, bool> canFunc)
        {
            _rlv.ProcessMessage($"@{cmd}=n", _sender.Id, _sender.Name);
            Assert.False(canFunc(_rlv.RLVManager));

            _rlv.ProcessMessage($"@{cmd}=y", _sender.Id, _sender.Name);
            Assert.True(canFunc(_rlv.RLVManager));
        }

        [Fact] public void CanFly() => CheckSimpleCommand("fly", m => m.CanFly());
        [Fact] public void CanTempRun() => CheckSimpleCommand("tempRun", m => m.CanTempRun());
        [Fact] public void CanAlwaysRun() => CheckSimpleCommand("alwaysRun", m => m.CanAlwaysRun());
        [Fact] public void CanChatShout() => CheckSimpleCommand("chatShout", m => m.CanChatShout());
        [Fact] public void CanChatWhisper() => CheckSimpleCommand("chatWhisper", m => m.CanChatWhisper());
        [Fact] public void CanChatNormal() => CheckSimpleCommand("chatNormal", m => m.CanChatNormal());
        [Fact] public void CanSendChat() => CheckSimpleCommand("sendChat", m => m.CanSendChat());
        [Fact] public void CanSendGesture() => CheckSimpleCommand("sendGesture", m => m.CanSendGesture());
        [Fact] public void CanCamUnlock() => CheckSimpleCommand("camUnlock", m => m.CanSetCamUnlock()); // CanSetCamUnlock() is correct here - alias
        [Fact] public void CanSetCamUnlock() => CheckSimpleCommand("setcam_unlock", m => m.CanSetCamUnlock());
        [Fact] public void CanTpLm() => CheckSimpleCommand("tpLm", m => m.CanTpLm());
        [Fact] public void CanTpLoc() => CheckSimpleCommand("tpLoc", m => m.CanTpLoc());
        [Fact] public void CanSit() => CheckSimpleCommand("sit", m => m.CanSit());
        [Fact] public void CanDefaultWear() => CheckSimpleCommand("defaultWear", m => m.CanDefaultWear());
        [Fact] public void CanSetGroup() => CheckSimpleCommand("setGroup", m => m.CanSetGroup());
        [Fact] public void CanSetDebug() => CheckSimpleCommand("setDebug", m => m.CanSetDebug());
        [Fact] public void CanSetEnv() => CheckSimpleCommand("setEnv", m => m.CanSetEnv());
        [Fact] public void CanAllowIdle() => CheckSimpleCommand("allowIdle", m => m.CanAllowIdle());
        [Fact] public void CanInteract() => CheckSimpleCommand("interact", m => m.CanInteract());
        [Fact] public void CanShowWorldMap() => CheckSimpleCommand("showWorldMap", m => m.CanShowWorldMap());
        [Fact] public void CanShowMiniMap() => CheckSimpleCommand("showMiniMap", m => m.CanShowMiniMap());
        [Fact] public void CanShowLoc() => CheckSimpleCommand("showLoc", m => m.CanShowLoc());
        [Fact] public void CanShowNearby() => CheckSimpleCommand("showNearby", m => m.CanShowNearby());
        [Fact] public void CanUnsharedWear() => CheckSimpleCommand("unsharedWear", m => m.CanUnsharedWear());
        [Fact] public void CanUnsharedUnwear() => CheckSimpleCommand("unsharedUnwear", m => m.CanUnsharedUnwear());
        [Fact] public void CanSharedWear() => CheckSimpleCommand("sharedWear", m => m.CanSharedWear());
        [Fact] public void CanSharedUnwear() => CheckSimpleCommand("sharedUnwear", m => m.CanSharedUnwear());

        #endregion

        #region CamMinFunctionsThrough

        [Fact]
        public void CamZoomMin_Default()
        {
            Assert.False(_rlv.RLVManager.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(default, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Single()
        {
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(1.5f, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Multiple_SingleSender()
        {
            _rlv.ProcessMessage("@CamZoomMin:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:4.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(4.5f, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Multiple_SingleSender_WithRemoval()
        {
            _rlv.ProcessMessage("@CamZoomMin:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:4.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);

            _rlv.ProcessMessage("@CamZoomMin:8.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:8.5=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(4.5f, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Multiple_MultipleSenders()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var sender3 = new RlvObject("Sender 3", new UUID("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"));

            _rlv.ProcessMessage("@CamZoomMin:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:4.5=n", sender2.Id, sender2.Name);
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", sender3.Id, sender3.Name);

            Assert.True(_rlv.RLVManager.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(4.5f, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Off()
        {
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:1.5=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(default, camZoomMin);
        }
        #endregion

        #region CamMaxFunctionsThrough
        [Fact]
        public void CamZoomMax_Default()
        {
            Assert.False(_rlv.RLVManager.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(default, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Single()
        {
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Multiple_SingleSender()
        {
            _rlv.ProcessMessage("@CamZoomMax:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:4.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Multiple_SingleSender_WithRemoval()
        {
            _rlv.ProcessMessage("@CamZoomMax:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:4.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            _rlv.ProcessMessage("@CamZoomMax:0.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:0.5=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Multiple_MultipleSenders()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var sender3 = new RlvObject("Sender 3", new UUID("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"));

            _rlv.ProcessMessage("@CamZoomMax:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:4.5=n", sender2.Id, sender2.Name);
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", sender3.Id, sender3.Name);

            Assert.True(_rlv.RLVManager.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Off()
        {
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:1.5=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(default, camZoomMax);
        }

        #endregion

        #region @CamZoomMin
        [Fact]
        public void CamZoomMin()
        {
            _rlv.ProcessMessage("@CamZoomMin:0.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(0.5f, camZoomMin);
        }
        #endregion

        #region @CamZoomMax
        [Fact]
        public void CamZoomMax()
        {
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }
        #endregion

        #region @setcam_fovmin
        [Fact]
        public void SetCamFovMin()
        {
            _rlv.ProcessMessage("@setcam_fovmin:15=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamFovMin(out var setCamFovMin));
            Assert.Equal(15f, setCamFovMin);
        }
        #endregion

        #region @CamDistMin
        [Fact]
        public void CamDistMin()
        {
            _rlv.ProcessMessage("@CamDistMin:0.2=n", _sender.Id, _sender.Name);

            // @CamDistMin is an alias of @SetCamAvDistMin
            Assert.True(_rlv.RLVManager.HasSetCamAvDistMin(out var distance));
            Assert.Equal(0.2f, distance);
        }
        #endregion

        #region @setcam_avdistmin
        [Fact]
        public void SetCamAvDistMin()
        {
            _rlv.ProcessMessage("@setcam_avdistmin:0.3=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamAvDistMin(out var setCamAvDistMin));
            Assert.Equal(0.3f, setCamAvDistMin);
        }
        #endregion

        #region @setcam_fovmax
        [Fact]
        public void SetCamFovMax()
        {
            _rlv.ProcessMessage("@setcam_fovmax:45=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamFovMax(out var setCamFovMax));
            Assert.Equal(45f, setCamFovMax);
        }
        #endregion

        #region @CamDistMax
        [Fact]
        public void CamDistMax()
        {
            _rlv.ProcessMessage("@CamDistMax:20=n", _sender.Id, _sender.Name);

            // CamDistMax is an alias for SetCamAvDistMax
            Assert.True(_rlv.RLVManager.HasSetCamAvDistMax(out var camDistMax));
            Assert.Equal(20f, camDistMax);
        }
        #endregion

        #region @setcam_avdistmax
        [Fact]
        public void SetCamAvDistMax()
        {
            _rlv.ProcessMessage("@setcam_avdistmax:30=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamAvDistMax(out var setCamAvDistMax));
            Assert.Equal(30f, setCamAvDistMax);
        }
        [Fact]
        public void SetCamAvDistMax_Synonym()
        {
            _rlv.ProcessMessage("@setcam_avdistmax:30=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamAvDistMax(out var setCamAvDistMax));
            Assert.Equal(30f, setCamAvDistMax);
        }
        #endregion

        #region @CamDrawAlphaMax
        [Fact]
        public void CamDrawAlphaMax()
        {
            _rlv.ProcessMessage("@CamDrawAlphaMax:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamDrawAlphaMax(out var camDrawAlphaMax));
            Assert.Equal(0.9f, camDrawAlphaMax);
        }
        #endregion

        #region @CamAvDist
        [Fact]
        public void CamAvDist()
        {
            _rlv.ProcessMessage("@CamAvDist:5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamAvDist(out var camAvDist));
            Assert.Equal(5f, camAvDist);
        }
        #endregion

        #region @FarTouch
        [Fact]
        public void CanFarTouch()
        {
            _rlv.ProcessMessage("@FarTouch:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanFarTouch(out var distance));
            Assert.Equal(0.9f, distance);
        }

        [Fact]
        public void CanFarTouch_Synonym()
        {
            _rlv.ProcessMessage("@TouchFar:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanFarTouch(out var distance));
            Assert.Equal(0.9f, distance);
        }

        [Fact]
        public void CanFarTouch_Default()
        {
            _rlv.ProcessMessage("@FarTouch=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanFarTouch(out var distance));
            Assert.Equal(1.5f, distance);
        }
        #endregion

        public const float FloatTolerance = 0.00001f;

        #region @CamDrawColor

        [Fact]
        public void CamDrawColor()
        {
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamDrawColor(out var color));

            Assert.Equal(0.1f, color.X, FloatTolerance);
            Assert.Equal(0.2f, color.Y, FloatTolerance);
            Assert.Equal(0.3f, color.Z, FloatTolerance);
        }

        [Fact]
        public void CamDrawColor_Default()
        {
            Assert.False(_rlv.RLVManager.HasCamDrawColor(out var color));
        }

        [Fact]
        public void CamDrawColor_Large()
        {
            _rlv.ProcessMessage("@CamDrawColor:5;6;7=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamDrawColor(out var color));
            Assert.Equal(1.0f, color.X, FloatTolerance);
            Assert.Equal(1.0f, color.Y, FloatTolerance);
            Assert.Equal(1.0f, color.Z, FloatTolerance);
        }

        [Fact]
        public void CamDrawColor_Negative()
        {
            _rlv.ProcessMessage("@CamDrawColor:-5;-6;-7=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamDrawColor(out var color));
            Assert.Equal(0.0f, color.X, FloatTolerance);
            Assert.Equal(0.0f, color.Y, FloatTolerance);
            Assert.Equal(0.0f, color.Z, FloatTolerance);
        }

        [Fact]
        public void CamDrawColor_Removal()
        {
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.HasCamDrawColor(out var color));
        }

        [Fact]
        public void CamDrawColor_Multi()
        {
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamDrawColor:0.2;0.3;0.6=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamDrawColor(out var color));
            Assert.Equal(0.15f, color.X, FloatTolerance);
            Assert.Equal(0.25f, color.Y, FloatTolerance);
            Assert.Equal(0.45f, color.Z, FloatTolerance);
        }

        #endregion

        #region @redirchat

        [Fact]
        public void IsRedirChat()
        {
            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.IsRedirChat(out var channels));

            var expected = new List<int>
            {
                1234,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public void IsRedirChat_Removed()
        {
            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@redirchat:1234=rem", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.IsRedirChat(out var channels));
        }

        [Fact]
        public void IsRedirChat_MultipleChannels()
        {
            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@redirchat:12345=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.IsRedirChat(out var channels));

            var expected = new List<int>
            {
                1234,
                12345,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public void IsRedirChat_RedirectChat()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            _rlv.RLVManager.ReportSendPublicMessage("Hello World");

            Assert.True(_rlv.RLVManager.IsRedirChat(out var channels));
            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsRedirChat_RedirectChatMultiple()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@redirchat:5678=add", _sender.Id, _sender.Name);

            _rlv.RLVManager.ReportSendPublicMessage("Hello World");
            _rlv.RLVManager.IsRedirChat(out var channels);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Hello World"),
                (5678, "Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsRedirChat_RedirectChatEmote()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);

            _rlv.RLVManager.ReportSendPublicMessage("/me says Hello World");

            Assert.True(_rlv.RLVManager.IsRedirChat(out var channels));
            Assert.Empty(actual);
        }

        #endregion

        #region @rediremote
        [Fact]
        public void IsRedirEmote()
        {
            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.IsRedirEmote(out var channels));

            var expected = new List<int>
            {
                1234,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public void IsRedirEmote_Removed()
        {
            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@rediremote:1234=rem", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.IsRedirEmote(out var channels));
        }

        [Fact]
        public void IsRedirEmote_MultipleChannels()
        {
            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@rediremote:12345=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.IsRedirEmote(out var channels));

            var expected = new List<int>
            {
                1234,
                12345,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public void IsRedirEmote_RedirectEmote()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            _rlv.RLVManager.ReportSendPublicMessage("/me says Hello World");

            Assert.True(_rlv.RLVManager.IsRedirEmote(out var channels));
            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/me says Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsRedirEmote_RedirectEmoteMultiple()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@rediremote:5678=n", _sender.Id, _sender.Name);

            _rlv.RLVManager.ReportSendPublicMessage("/me says Hello World");
            _rlv.RLVManager.IsRedirEmote(out var channels);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/me says Hello World"),
                (5678, "/me says Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsRedirEmote_RedirectEmoteChat()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            _rlv.RLVManager.ReportSendPublicMessage("Hello World");

            Assert.True(_rlv.RLVManager.IsRedirEmote(out var channels));
            Assert.Empty(actual);
        }

        #endregion

        #region CanChat

        [Fact]
        public void CanChat_Default()
        {
            Assert.True(_rlv.RLVManager.CanChat(0, "Hello"));
            Assert.True(_rlv.RLVManager.CanChat(0, "/me says Hello"));
            Assert.True(_rlv.RLVManager.CanChat(5, "Hello"));
        }

        [Fact]
        public void CanChat_SendChatRestriction()
        {
            _rlv.ProcessMessage("@sendchat=n", _sender.Id, _sender.Name);

            // No public chat allowed unless it starts with '/'
            Assert.False(_rlv.RLVManager.CanChat(0, "Hello"));

            // Emotes and other messages starting with / are allowed
            Assert.True(_rlv.RLVManager.CanChat(0, "/me says Hello"));
            Assert.True(_rlv.RLVManager.CanChat(0, "/ something?"));

            // Messages containing ()"-*=_^ are prohibited
            Assert.False(_rlv.RLVManager.CanChat(0, "/me says Hello ^_^"));

            // Private channels are not impacted
            Assert.True(_rlv.RLVManager.CanChat(5, "Hello"));
        }

        [Fact]
        public void CanSendChannel_Default()
        {
            Assert.True(_rlv.RLVManager.CanChat(123, "Hello world"));
        }

        [Fact]
        public void CanSendChannel()
        {
            _rlv.ProcessMessage("@sendchannel=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanChat(123, "Hello world"));
        }

        [Fact]
        public void CanSendChannel_Exception()
        {
            _rlv.ProcessMessage("@sendchannel=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendchannel:123=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanChat(123, "Hello world"));
        }

        [Fact]
        public void CanSendChannel_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@sendchannel_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendchannel:123=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendchannel:456=n", sender2.Id, sender2.Name);

            Assert.True(_rlv.RLVManager.CanChat(123, "Hello world"));
            Assert.False(_rlv.RLVManager.CanChat(456, "Hello world"));
        }

        [Fact]
        public void CanSendChannelExcept()
        {
            _rlv.ProcessMessage("@sendchannel_except:456=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanChat(123, "Hello world"));
            Assert.False(_rlv.RLVManager.CanChat(456, "Hello world"));
        }

        #endregion

        #region @recvchat @recvchat_sec

        [Fact]
        public void CanRecvChat_Default()
        {
            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", UUID.Random()));
            Assert.True(_rlv.RLVManager.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.RLVManager.CanReceiveChat("/me says Hello world", UUID.Random()));
        }

        [Fact]
        public void CanRecvChat()
        {
            _rlv.ProcessMessage("@recvchat=n", _sender.Id, _sender.Name);
            var userId = UUID.Random();

            Assert.False(_rlv.RLVManager.CanReceiveChat("Hello world", null));
            Assert.False(_rlv.RLVManager.CanReceiveChat("Hello world", userId));
            Assert.True(_rlv.RLVManager.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.RLVManager.CanReceiveChat("/me says Hello world", userId));
        }

        [Fact]
        public void CanRecvChat_Except()
        {
            var userId = UUID.Random();

            _rlv.ProcessMessage("@recvchat=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvchat:{userId}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", userId));
            Assert.True(_rlv.RLVManager.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.RLVManager.CanReceiveChat("/me says Hello world", userId));
        }

        [Fact]
        public void CanRecvChat_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@recvchat_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvchat:{userId1}=add", sender2.Id, sender2.Name);
            _rlv.ProcessMessage($"@recvchat:{userId2}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanReceiveChat("Hello world", null));
            Assert.False(_rlv.RLVManager.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", userId2));
        }

        [Fact]
        public void CanRecvChat_RecvEmote()
        {
            _rlv.ProcessMessage("@recvemote=n", _sender.Id, _sender.Name);
            var userId = UUID.Random();

            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", userId));
            Assert.False(_rlv.RLVManager.CanReceiveChat("/me says Hello world", null));
            Assert.False(_rlv.RLVManager.CanReceiveChat("/me says Hello world", userId));
        }

        [Fact]
        public void CanRecvChat_RecvEmoteFrom()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvemotefrom:{userId1}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", userId2));
            Assert.False(_rlv.RLVManager.CanReceiveChat("/me says Hello world", userId1));
            Assert.True(_rlv.RLVManager.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public void CanRecvChat_RecvEmote_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@recvemote=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvemote:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", userId2));
            Assert.True(_rlv.RLVManager.CanReceiveChat("/me says Hello world", userId1));
            Assert.False(_rlv.RLVManager.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public void CanRecvChat_RecvEmote_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@recvemote_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvemote:{userId1}=add", sender2.Id, sender2.Name);
            _rlv.ProcessMessage($"@recvemote:{userId2}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", userId2));
            Assert.False(_rlv.RLVManager.CanReceiveChat("/me says Hello world", userId1));
            Assert.True(_rlv.RLVManager.CanReceiveChat("/me says Hello world", userId2));
        }
        #endregion

        #region @recvchatfrom

        [Fact]
        public void CanRecvChatFrom()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvchatfrom:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.RLVManager.CanReceiveChat("/me says Hello world", userId1));

            Assert.True(_rlv.RLVManager.CanReceiveChat("Hello world", userId2));
        }

        #endregion


        #region @sendim @sendim_sec @sendimto

        [Fact]
        public void CanSendIM_Default()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.RLVManager.CanSendIM("Hello", userId1));
            Assert.True(_rlv.RLVManager.CanSendIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanSendIM()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanSendIM("Hello", userId1));
            Assert.False(_rlv.RLVManager.CanSendIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanSendIM_Exception()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanSendIM("Hello world", userId1));
        }

        [Fact]
        public void CanSendIM_Exception_SingleGroup()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:Group Name=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanSendIM("Hello world", groupId1, "Group Name"));
        }

        [Fact]
        public void CanSendIM_Exception_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanSendIM("Hello world", groupId1, "Group name"));
        }

        [Fact]
        public void CanSendIM_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@sendim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:{userId1}=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:{userId2}=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.RLVManager.CanSendIM("Hello world", userId1));
            Assert.False(_rlv.RLVManager.CanSendIM("Hello world", userId2));
        }

        [Fact]
        public void CanSendIM_Secure_Group()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:Group Name=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:allgroups=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.RLVManager.CanSendIM("Hello world", groupId1, "Group Name"));
            Assert.False(_rlv.RLVManager.CanSendIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanSendIM_Secure_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanSendIM("Hello world", groupId1, "Group Name"));
            Assert.True(_rlv.RLVManager.CanSendIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanSendIMTo()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@sendimto:{userId1}=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanSendIM("Hello world", userId1));
            Assert.True(_rlv.RLVManager.CanSendIM("Hello world", userId2));
        }

        [Fact]
        public void CanSendIMTo_Group()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var groupId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@sendimto:First Group=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanSendIM("Hello world", groupId1, "First Group"));
            Assert.True(_rlv.RLVManager.CanSendIM("Hello world", groupId2, "Second Group"));
        }

        [Fact]
        public void CanSendIMTo_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var groupId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@sendimto:allgroups=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanSendIM("Hello world", groupId1, "First Group"));
            Assert.False(_rlv.RLVManager.CanSendIM("Hello world", groupId2, "Second Group"));
        }

        #endregion

        #region @startim @startimto

        [Fact]
        public void CanStartIM_Default()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.RLVManager.CanStartIM(null));
            Assert.True(_rlv.RLVManager.CanStartIM(userId1));
        }

        [Fact]
        public void CanStartIM()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@startim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanStartIM(null));
            Assert.False(_rlv.RLVManager.CanStartIM(userId1));
        }

        [Fact]
        public void CanStartIM_Exception()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@startim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@startim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanStartIM(userId1));
            Assert.False(_rlv.RLVManager.CanStartIM(userId2));
        }

        [Fact]
        public void CanStartIMTo()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@startimto:{userId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanStartIM(userId1));
            Assert.False(_rlv.RLVManager.CanStartIM(userId2));
        }

        #endregion

        #region @recvim @recvim_sec @recvimto

        [Fact]
        public void CanReceiveIM_Default()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.RLVManager.CanReceiveIM("Hello", userId1));
            Assert.True(_rlv.RLVManager.CanReceiveIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanReceiveIM()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanReceiveIM("Hello", userId1));
            Assert.False(_rlv.RLVManager.CanReceiveIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanReceiveIM_Exception()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanReceiveIM("Hello world", userId1));
        }

        [Fact]
        public void CanReceiveIM_Exception_SingleGroup()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:Group Name=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanReceiveIM("Hello world", groupId1, "Group Name"));
        }

        [Fact]
        public void CanReceiveIM_Exception_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanReceiveIM("Hello world", groupId1, "Group name"));
        }

        [Fact]
        public void CanReceiveIM_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@recvim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:{userId1}=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:{userId2}=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.RLVManager.CanReceiveIM("Hello world", userId1));
            Assert.False(_rlv.RLVManager.CanReceiveIM("Hello world", userId2));
        }

        [Fact]
        public void CanReceiveIM_Secure_Group()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:Group Name=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:allgroups=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.RLVManager.CanReceiveIM("Hello world", groupId1, "Group Name"));
            Assert.False(_rlv.RLVManager.CanReceiveIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanReceiveIM_Secure_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanReceiveIM("Hello world", groupId1, "Group Name"));
            Assert.True(_rlv.RLVManager.CanReceiveIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanReceiveIMFrom()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvimfrom:{userId1}=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanReceiveIM("Hello world", userId1));
            Assert.True(_rlv.RLVManager.CanReceiveIM("Hello world", userId2));
        }

        [Fact]
        public void CanReceiveIMFrom_Group()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var groupId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvimfrom:First Group=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanReceiveIM("Hello world", groupId1, "First Group"));
            Assert.True(_rlv.RLVManager.CanReceiveIM("Hello world", groupId2, "Second Group"));
        }

        [Fact]
        public void CanReceiveIMTo_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var groupId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvimfrom:allgroups=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanReceiveIM("Hello world", groupId1, "First Group"));
            Assert.False(_rlv.RLVManager.CanReceiveIM("Hello world", groupId2, "Second Group"));
        }

        #endregion

        #region @TpLocal
        [Fact]
        public void CanTpLocal_Default()
        {
            _rlv.ProcessMessage("@TpLocal=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanTpLocal(out var distance));
            Assert.Equal(0.0f, distance, FloatTolerance);
        }

        [Fact]
        public void CanTpLocal()
        {
            _rlv.ProcessMessage("@TpLocal:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanTpLocal(out var distance));
            Assert.Equal(0.9f, distance, FloatTolerance);
        }
        #endregion

        #region @tplure @tplure_sec 

        [Fact]
        public void CanTpLure_Default()
        {
            Assert.True(_rlv.RLVManager.CanTPLure(null));
            Assert.True(_rlv.RLVManager.CanTPLure(UUID.Random()));
        }

        [Fact]
        public void CanTpLure()
        {
            _rlv.ProcessMessage("@tplure=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTPLure(null));
            Assert.False(_rlv.RLVManager.CanTPLure(UUID.Random()));
        }

        [Fact]
        public void CanTpLure_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@tplure=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@tplure:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTPLure(null));
            Assert.True(_rlv.RLVManager.CanTPLure(userId1));
            Assert.False(_rlv.RLVManager.CanTPLure(userId2));
        }

        [Fact]
        public void CanTpLure_Secure_Default()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@tplure_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTPLure(null));
            Assert.False(_rlv.RLVManager.CanTPLure(userId1));
            Assert.False(_rlv.RLVManager.CanTPLure(userId2));
        }

        [Fact]
        public void CanTpLure_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@tplure_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@tplure:{userId1}=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@tplure:{userId2}=add", sender2.Id, sender2.Name);

            Assert.False(_rlv.RLVManager.CanTPLure(null));
            Assert.True(_rlv.RLVManager.CanTPLure(userId1));
            Assert.False(_rlv.RLVManager.CanTPLure(userId2));
        }

        #endregion

        #region @sittp

        [Fact]
        public void CanSitTp_Default()
        {
            Assert.False(_rlv.RLVManager.CanSitTp(out var maxDistance));
            Assert.Equal(1.5f, maxDistance);
        }

        [Fact]
        public void CanSitTp_Single()
        {
            _rlv.ProcessMessage("@SitTp:2.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanSitTp(out var maxDistance));
            Assert.Equal(2.5f, maxDistance);
        }

        [Fact]
        public void CanSitTp_Multiple_SingleSender()
        {
            _rlv.ProcessMessage("@SitTp:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@SitTp:4.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@SitTp:2.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanSitTp(out var maxDistance));
            Assert.Equal(2.5f, maxDistance);
        }

        [Fact]
        public void CanSitTp_Multiple_SingleSender_WithRemoval()
        {
            _rlv.ProcessMessage("@SitTp:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@SitTp:4.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@SitTp:2.5=n", _sender.Id, _sender.Name);

            _rlv.ProcessMessage("@SitTp:8.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@SitTp:8.5=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanSitTp(out var maxDistance));
            Assert.Equal(2.5f, maxDistance);
        }

        [Fact]
        public void CanSitTp_Multiple_MultipleSenders()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var sender3 = new RlvObject("Sender 3", new UUID("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"));

            _rlv.ProcessMessage("@SitTp:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@SitTp:4.5=n", sender2.Id, sender2.Name);
            _rlv.ProcessMessage("@SitTp:2.5=n", sender3.Id, sender3.Name);

            Assert.True(_rlv.RLVManager.CanSitTp(out var maxDistance));
            Assert.Equal(2.5f, maxDistance);
        }

        [Fact]
        public void CanSitTp_Off()
        {
            _rlv.ProcessMessage("@SitTp:2.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@SitTp:2.5=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanSitTp(out var maxDistance));
            Assert.Equal(1.5f, maxDistance);
        }
        #endregion

        #region @standtp
        [Fact] public void CanStandTp() => CheckSimpleCommand("standTp", m => m.CanStandTp());
        #endregion

        #region @tpto FORCE

        [Fact]
        public void TpTo_Default()
        {
            var raised = Assert.Raises<TpToEventArgs>(
                attach: n => _rlv.Actions.TpTo += n,
                detach: n => _rlv.Actions.TpTo -= n,
                testCode: () => _rlv.ProcessMessage("@tpto:1.5/2.5/3.5=force", _sender.Id, _sender.Name)
            );

            Assert.Equal(1.5f, raised.Arguments.X, FloatTolerance);
            Assert.Equal(2.5f, raised.Arguments.Y, FloatTolerance);
            Assert.Equal(3.5f, raised.Arguments.Z, FloatTolerance);
            Assert.Null(raised.Arguments.RegionName);
            Assert.Null(raised.Arguments.Lookat);
        }

        [Fact]
        public void TpTo_WithRegion()
        {
            var raised = Assert.Raises<TpToEventArgs>(
                attach: n => _rlv.Actions.TpTo += n,
                detach: n => _rlv.Actions.TpTo -= n,
                testCode: () => _rlv.ProcessMessage("@tpto:Region Name/1.5/2.5/3.5=force", _sender.Id, _sender.Name)
            );

            Assert.Equal(1.5f, raised.Arguments.X, FloatTolerance);
            Assert.Equal(2.5f, raised.Arguments.Y, FloatTolerance);
            Assert.Equal(3.5f, raised.Arguments.Z, FloatTolerance);
            Assert.Equal("Region Name", raised.Arguments.RegionName);
            Assert.Null(raised.Arguments.Lookat);
        }

        [Fact]
        public void TpTo_WithRegionAndLookAt()
        {
            var raised = Assert.Raises<TpToEventArgs>(
                attach: n => _rlv.Actions.TpTo += n,
                detach: n => _rlv.Actions.TpTo -= n,
                testCode: () => _rlv.ProcessMessage("@tpto:Region Name/1.5/2.5/3.5;3.1415=force", _sender.Id, _sender.Name)
            );

            Assert.Equal(1.5f, raised.Arguments.X, FloatTolerance);
            Assert.Equal(2.5f, raised.Arguments.Y, FloatTolerance);
            Assert.Equal(3.5f, raised.Arguments.Z, FloatTolerance);
            Assert.Equal("Region Name", raised.Arguments.RegionName);
            Assert.NotNull(raised.Arguments.Lookat);
            Assert.Equal(3.1415f, raised.Arguments.Lookat.Value, FloatTolerance);
        }

        [Fact]
        public void TpTo_RestrictedUnsit()
        {
            _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);

            bool raisedEvent = false;
            _rlv.Actions.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(_rlv.ProcessMessage("@tpto:1.5/2.5/3.5=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }

        [Fact]
        public void TpTo_RestrictedTpLoc()
        {
            _rlv.ProcessMessage("@tploc=n", _sender.Id, _sender.Name);

            bool raisedEvent = false;
            _rlv.Actions.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(_rlv.ProcessMessage("@tpto:1.5/2.5/3.5=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }

        #endregion

        #region @accepttp

        [Fact]
        public void CanAutoAcceptTp_Default()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            Assert.False(_rlv.RLVManager.IsAutoAcceptTp(userId1));
            Assert.False(_rlv.RLVManager.IsAutoAcceptTp(userId2));
            Assert.False(_rlv.RLVManager.IsAutoAcceptTp());
        }

        [Fact]
        public void CanAutoAcceptTp_User()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@accepttp:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.IsAutoAcceptTp(userId1));
            Assert.False(_rlv.RLVManager.IsAutoAcceptTp(userId2));
            Assert.False(_rlv.RLVManager.IsAutoAcceptTp());
        }

        [Fact]
        public void CanAutoAcceptTp_All()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@accepttp=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.IsAutoAcceptTp(userId1));
            Assert.True(_rlv.RLVManager.IsAutoAcceptTp(userId2));
            Assert.True(_rlv.RLVManager.IsAutoAcceptTp());
        }

        #endregion

        #region @accepttprequest

        [Fact]
        public void CanAutoAcceptTpRequest_Default()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            Assert.False(_rlv.RLVManager.IsAutoAcceptTpRequest(userId1));
            Assert.False(_rlv.RLVManager.IsAutoAcceptTpRequest(userId2));
            Assert.False(_rlv.RLVManager.IsAutoAcceptTpRequest());
        }

        [Fact]
        public void CanAutoAcceptTpRequest_User()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@accepttprequest:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.IsAutoAcceptTpRequest(userId1));
            Assert.False(_rlv.RLVManager.IsAutoAcceptTpRequest(userId2));
            Assert.False(_rlv.RLVManager.IsAutoAcceptTpRequest());
        }

        [Fact]
        public void CanAutoAcceptTpRequest_All()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@accepttprequest=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.IsAutoAcceptTpRequest(userId1));
            Assert.True(_rlv.RLVManager.IsAutoAcceptTpRequest(userId2));
            Assert.True(_rlv.RLVManager.IsAutoAcceptTpRequest());
        }

        #endregion

        #region @tprequest @tprequest_sec

        [Fact]
        public void CanTpRequest_Default()
        {
            Assert.True(_rlv.RLVManager.CanTpRequest(null));
            Assert.True(_rlv.RLVManager.CanTpRequest(UUID.Random()));
        }

        [Fact]
        public void CanTpRequest()
        {
            _rlv.ProcessMessage("@tprequest=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTpRequest(null));
            Assert.False(_rlv.RLVManager.CanTpRequest(UUID.Random()));
        }

        [Fact]
        public void CanTpRequest_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@tprequest=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@tprequest:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTpRequest(null));
            Assert.True(_rlv.RLVManager.CanTpRequest(userId1));
            Assert.False(_rlv.RLVManager.CanTpRequest(userId2));
        }

        [Fact]
        public void CanTpRequest_Secure_Default()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@tprequest_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTpRequest(null));
            Assert.False(_rlv.RLVManager.CanTpRequest(userId1));
            Assert.False(_rlv.RLVManager.CanTpRequest(userId2));
        }

        [Fact]
        public void CanTpRequest_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@tprequest_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@tprequest:{userId1}=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@tprequest:{userId2}=add", sender2.Id, sender2.Name);

            Assert.False(_rlv.RLVManager.CanTpRequest(null));
            Assert.True(_rlv.RLVManager.CanTpRequest(userId1));
            Assert.False(_rlv.RLVManager.CanTpRequest(userId2));
        }

        #endregion

        #region @showinv
        [Fact] public void CanShowInv() => CheckSimpleCommand("showInv", m => m.CanShowInv());

        #endregion

        #region @viewNote
        [Fact] public void CanViewNote() => CheckSimpleCommand("viewNote", m => m.CanViewNote());
        #endregion

        #region @viewscript
        [Fact] public void CanViewScript() => CheckSimpleCommand("viewScript", m => m.CanViewScript());
        #endregion

        #region @viewtexture
        [Fact] public void CanViewTexture() => CheckSimpleCommand("viewTexture", m => m.CanViewTexture());
        #endregion

        #region @edit @editobj @editworld @editattach

        [Fact]
        public void CanEdit_Default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));
        }

        [Fact]
        public void CanEdit()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@edit=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));
        }

        [Fact]
        public void CanEdit_Exception()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@edit=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@edit:{objectId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));

            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, objectId2));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, objectId2));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId2));
        }

        [Fact]
        public void CanEdit_Specific()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@editobj:{objectId1}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));

            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, objectId2));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, objectId2));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId2));
        }

        [Fact]
        public void CanEdit_World()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage($"@editworld=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));
        }

        [Fact]
        public void CanEdit_Attachment()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage($"@editattach=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, null));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, null));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, null));

            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.True(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));
        }

        #endregion

        #region @canrez
        [Fact] public void CanRez() => CheckSimpleCommand("rez", m => m.CanRez());

        #endregion

        #region @share @share_sec

        [Fact]
        public void CanShare_Default()
        {
            Assert.True(_rlv.RLVManager.CanShare(null));
            Assert.True(_rlv.RLVManager.CanShare(UUID.Random()));
        }

        [Fact]
        public void CanShare()
        {
            _rlv.ProcessMessage("@share=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanShare(null));
            Assert.False(_rlv.RLVManager.CanShare(UUID.Random()));
        }

        [Fact]
        public void CanShare_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@share=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@share:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanShare(null));
            Assert.True(_rlv.RLVManager.CanShare(userId1));
            Assert.False(_rlv.RLVManager.CanShare(userId2));
        }

        [Fact]
        public void CanShare_Secure_Default()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@share_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanShare(null));
            Assert.False(_rlv.RLVManager.CanShare(userId1));
            Assert.False(_rlv.RLVManager.CanShare(userId2));
        }

        [Fact]
        public void CanShare_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@share_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@share:{userId1}=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@share:{userId2}=add", sender2.Id, sender2.Name);

            Assert.False(_rlv.RLVManager.CanShare(null));
            Assert.True(_rlv.RLVManager.CanShare(userId1));
            Assert.False(_rlv.RLVManager.CanShare(userId2));
        }

        #endregion

        #region @unsit
        [Fact] public void CanUnsit() => CheckSimpleCommand("unsit", m => m.CanUnsit());
        #endregion

        #region @sit FORCE
        private void SetupSitTarget(UUID objectId, bool isCurrentlySitting)
        {
            _callbacks.Setup(e =>
                e.TryGetSitTarget(objectId, out isCurrentlySitting)
            ).ReturnsAsync(true);
        }

        [Fact]
        public void ForceSit_Default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            SetupSitTarget(objectId1, false);

            var raised = Assert.Raises<SitEventArgs>(
                attach: n => _rlv.Actions.Sit += n,
                detach: n => _rlv.Actions.Sit -= n,
                testCode: () => _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name)
            );

            Assert.Equal(objectId1, raised.Arguments.Target);
        }

        [Fact]
        public void ForceSit_RestrictedUnsit_WhileStanding()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            SetupSitTarget(objectId1, false);

            _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);

            var raised = Assert.Raises<SitEventArgs>(
                attach: n => _rlv.Actions.Sit += n,
                detach: n => _rlv.Actions.Sit -= n,
                testCode: () => _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name)
            );

            Assert.Equal(objectId1, raised.Arguments.Target);
        }

        [Fact]
        public void ForceSit_RestrictedUnsit_WhileSeated()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            SetupSitTarget(objectId1, true);

            _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);

            bool raisedEvent = false;
            _rlv.Actions.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(_rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }


        [Fact]
        public void ForceSit_RestrictedSit()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            SetupSitTarget(objectId1, true);

            _rlv.ProcessMessage("@sit=n", _sender.Id, _sender.Name);

            bool raisedEvent = false;
            _rlv.Actions.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(_rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }

        [Fact]
        public void ForceSit_RestrictedStandTp()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            SetupSitTarget(objectId1, true);

            _rlv.ProcessMessage("@standtp=n", _sender.Id, _sender.Name);

            bool raisedEvent = false;
            _rlv.Actions.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(_rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }

        [Fact]
        public void ForceSit_InvalidObject()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            // SetupSitTarget(objectId1, true); <-- Don't setup sit target for this test

            bool raisedEvent = false;
            _rlv.Actions.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(_rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }
        #endregion
    }
}
