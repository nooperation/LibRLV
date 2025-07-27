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

        public const float FloatTolerance = 0.00001f;

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

        //
        // Blacklist handling
        //

        #region @versionnumbl=<channel_number>

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

        #region @getblacklist[:filter]=<channel_number>
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

        #region @getstatus

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

        #region @getstatusall

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

        //
        // Movement
        //

        #region @fly
        [Fact] public void CanFly() => CheckSimpleCommand("fly", m => m.CanFly());
        #endregion

        #region @temprun
        [Fact] public void CanTempRun() => CheckSimpleCommand("tempRun", m => m.CanTempRun());
        #endregion

        #region @alwaysrun
        [Fact] public void CanAlwaysRun() => CheckSimpleCommand("alwaysRun", m => m.CanAlwaysRun());
        #endregion

        #region @setrot:<angle_in_radians>=force
        [Fact]
        public void SetRot()
        {
            var raised = Assert.Raises<SetRotEventArgs>(
                 attach: n => _rlv.Actions.SetRot += n,
                 detach: n => _rlv.Actions.SetRot -= n,
                 testCode: () => _rlv.ProcessMessage("@setrot:1.5=force", _sender.Id, _sender.Name)
             );

            Assert.Equal(1.5f, raised.Arguments.AngleInRadians, FloatTolerance);
        }
        #endregion

        #region @adjustheight:<distance_pelvis_to_foot_in_meters>;<factor>[;delta_in_meters]=force
        [Fact]
        public void AdjustHeight()
        {
            var raised = Assert.Raises<AdjustHeightEventArgs>(
                 attach: n => _rlv.Actions.AdjustHeight += n,
                 detach: n => _rlv.Actions.AdjustHeight -= n,
                 testCode: () => _rlv.ProcessMessage("@adjustheight:4.3;1.25=force", _sender.Id, _sender.Name)
             );

            Assert.Equal(4.3f, raised.Arguments.Distance, FloatTolerance);
            Assert.Equal(1.25f, raised.Arguments.Factor, FloatTolerance);
            Assert.Equal(0.0f, raised.Arguments.Delta, FloatTolerance);
        }

        [Fact]
        public void AdjustHeight_WithDelta()
        {
            var raised = Assert.Raises<AdjustHeightEventArgs>(
                 attach: n => _rlv.Actions.AdjustHeight += n,
                 detach: n => _rlv.Actions.AdjustHeight -= n,
                 testCode: () => _rlv.ProcessMessage("@adjustheight:4.3;1.25;12.34=force", _sender.Id, _sender.Name)
             );


            Assert.Equal(4.3f, raised.Arguments.Distance, FloatTolerance);
            Assert.Equal(1.25f, raised.Arguments.Factor, FloatTolerance);
            Assert.Equal(12.34f, raised.Arguments.Delta, FloatTolerance);
        }
        #endregion

        //
        // Camera and view
        //

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

        #region @setcam_fovmax
        [Fact]
        public void SetCamFovMax()
        {
            _rlv.ProcessMessage("@setcam_fovmax:45=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamFovMax(out var setCamFovMax));
            Assert.Equal(45f, setCamFovMax);
        }
        #endregion

        #region @setcam_fov:<fov_in_radians>=force
        [Fact]
        public void SetCamFov()
        {
            var raised = Assert.Raises<SetCamFOVEventArgs>(
                 attach: n => _rlv.Actions.SetCamFOV += n,
                 detach: n => _rlv.Actions.SetCamFOV -= n,
                 testCode: () => _rlv.ProcessMessage("@setcam_fov:1.75=force", _sender.Id, _sender.Name)
             );

            Assert.Equal(1.75f, raised.Arguments.FOVInRadians, FloatTolerance);
        }

        [Fact]
        public void SetCamFov_Restricted()
        {
            bool raisedEvent = false;
            _rlv.Actions.SetCamFOV += (sender, args) =>
            {
                raisedEvent = true;
            };

            _rlv.ProcessMessage("@setcam_unlock=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.ProcessMessage("@setcam_fov:1.75=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }

        [Fact]
        public void SetCamFov_Restricted_Synonym()
        {
            bool raisedEvent = false;
            _rlv.Actions.SetCamFOV += (sender, args) =>
            {
                raisedEvent = true;
            };

            _rlv.ProcessMessage("@camunlock=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.ProcessMessage("@setcam_fov:1.75=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
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
            _rlv.ProcessMessage("@camdistmax:30=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamAvDistMax(out var setCamAvDistMax));
            Assert.Equal(30f, setCamAvDistMax);
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

        [Fact]
        public void SetCamAvDistMin_Synonym()
        {
            _rlv.ProcessMessage("@camdistmin:0.3=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamAvDistMin(out var setCamAvDistMin));
            Assert.Equal(0.3f, setCamAvDistMin);
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

        #region @camdrawmin:<min_distance>=<y/n>

        [Fact]
        public void CamDrawMin()
        {
            _rlv.ProcessMessage("@camdrawmin:1.75=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamDrawMin(out var camDrawMin));
            Assert.Equal(1.75f, camDrawMin);
        }

        [Fact]
        public void CamDrawMin_Small()
        {
            Assert.False(_rlv.ProcessMessage("@camdrawmin:0.15=n", _sender.Id, _sender.Name));
            Assert.False(_rlv.RLVManager.HasCamDrawMin(out var camDrawMin));
        }

        #endregion

        #region @camdrawmax:<max_distance>=<y/n>

        [Fact]
        public void CamDrawMax()
        {
            _rlv.ProcessMessage("@camdrawmax:1.75=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamDrawMax(out var camDrawMax));
            Assert.Equal(1.75f, camDrawMax);
        }

        [Fact]
        public void CamDrawMax_Small()
        {
            Assert.False(_rlv.ProcessMessage("@camdrawmax:0.15=n", _sender.Id, _sender.Name));
            Assert.False(_rlv.RLVManager.HasCamDrawMax(out var camDrawMax));
        }

        #endregion

        #region @camdrawalphamin:<min_distance>=<y/n>

        [Fact]
        public void CamDrawAlphaMin()
        {
            _rlv.ProcessMessage("@camdrawalphamin:1.75=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamDrawAlphaMin(out var camDrawAlphaMin));
            Assert.Equal(1.75f, camDrawAlphaMin);
        }

        #endregion

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

        #region @camunlock
        [Fact] public void CanSetCamUnlock() => CheckSimpleCommand("setcam_unlock", m => !m.IsCamLocked());
        #endregion

        #region @setcam_unlock
        [Fact] public void CanCamUnlock() => CheckSimpleCommand("camunlock", m => !m.IsCamLocked());
        #endregion

        #region @camavdist
        [Fact]
        public void CamAvDist()
        {
            _rlv.ProcessMessage("@CamAvDist:5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasCamAvDist(out var camAvDist));
            Assert.Equal(5f, camAvDist);
        }
        #endregion

        #region @camtextures @setcam_textures[:texture_uuid]=<y/n>

        [Theory]
        [InlineData("setcam_textures")]
        [InlineData("camtextures")]
        public void SetCamTextures(string command)
        {
            _rlv.ProcessMessage($"@{command}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamtextures(out var actualTextureId));

            Assert.Equal(UUID.Zero, actualTextureId);
        }

        [Theory]
        [InlineData("setcam_textures")]
        [InlineData("camtextures")]
        public void SetCamTextures_Single(string command)
        {
            var textureId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage($"@{command}:{textureId1}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamtextures(out var actualTextureId));

            Assert.Equal(textureId1, actualTextureId);
        }

        [Theory]
        [InlineData("setcam_textures", "setcam_textures")]
        [InlineData("setcam_textures", "camtextures")]
        [InlineData("camtextures", "camtextures")]
        [InlineData("camtextures", "setcam_textures")]
        public void SetCamTextures_Multiple_Synonyms(string command1, string command2)
        {
            var textureId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var textureId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@{command1}:{textureId1}=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@{command2}:{textureId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamtextures(out var actualTextureId2));

            _rlv.ProcessMessage($"@{command1}:{textureId2}=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.HasSetCamtextures(out var actualTextureId1));

            Assert.Equal(textureId2, actualTextureId2);
            Assert.Equal(textureId1, actualTextureId1);
        }

        #endregion

        #region @getcam_avdistmin=<channel_number>
        [Fact]
        public void GetCamAvDistMin()
        {
            var actual = _callbacks.RecordReplies();

            var distance = 12.34f;

            _callbacks.Setup(e =>
                e.TryGetCamAvDistMin(out distance)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, distance.ToString()),
            };

            Assert.True(_rlv.ProcessMessage("@getcam_avdistmin=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetCamAvDistMin_Default()
        {
            var actual = _callbacks.RecordReplies();

            var distance = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamAvDistMin(out distance)
            ).ReturnsAsync(false);

            Assert.False(_rlv.ProcessMessage("@getcam_avdistmin=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion

        #region @getcam_avdistmax=<channel_number>
        [Fact]
        public void GetCamAvDistMax()
        {
            var actual = _callbacks.RecordReplies();

            var distance = 12.34f;

            _callbacks.Setup(e =>
                e.TryGetCamAvDistMax(out distance)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, distance.ToString()),
            };

            Assert.True(_rlv.ProcessMessage("@getcam_avdistmax=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetCamAvDistMax_Default()
        {
            var actual = _callbacks.RecordReplies();

            var distance = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamAvDistMax(out distance)
            ).ReturnsAsync(false);

            Assert.False(_rlv.ProcessMessage("@getcam_avdistmax=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion

        #region @getcam_fovmin=<channel_number>
        [Fact]
        public void GetCamFovMin()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 15.25f;

            _callbacks.Setup(e =>
                e.TryGetCamFovMin(out fov)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, fov.ToString()),
            };

            Assert.True(_rlv.ProcessMessage("@getcam_fovmin=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetCamFovMin_Default()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamFovMin(out fov)
            ).ReturnsAsync(false);

            Assert.False(_rlv.ProcessMessage("@getcam_fovmin=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion

        #region @getcam_fovmax=<channel_number>
        [Fact]
        public void GetCamFovMax()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 45.75f;
            _callbacks.Setup(e =>
                e.TryGetCamFovMax(out fov)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, fov.ToString()),
            };

            Assert.True(_rlv.ProcessMessage("@getcam_fovmax=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetCamFovMax_Default()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamFovMax(out fov)
            ).ReturnsAsync(false);

            Assert.False(_rlv.ProcessMessage("@getcam_fovmax=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion

        #region @getcam_zoommin=<channel_number>
        [Fact]
        public void GetCamZoomMin()
        {
            var actual = _callbacks.RecordReplies();

            var zoom = 0.75f;

            _callbacks.Setup(e =>
                e.TryGetCamZoomMin(out zoom)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, zoom.ToString()),
            };

            Assert.True(_rlv.ProcessMessage("@getcam_zoommin=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetCamZoomMin_Default()
        {
            var actual = _callbacks.RecordReplies();

            var zoom = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamZoomMin(out zoom)
            ).ReturnsAsync(false);

            Assert.False(_rlv.ProcessMessage("@getcam_zoommin=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion

        #region @getcam_fov=<channel_number>
        [Fact]
        public void GetCamFov()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 25.5f;

            _callbacks.Setup(e =>
                e.TryGetCamFov(out fov)
            ).ReturnsAsync(true);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, fov.ToString()),
            };

            Assert.True(_rlv.ProcessMessage("@getcam_fov=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetCamFov_Default()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamFov(out fov)
            ).ReturnsAsync(false);

            Assert.False(_rlv.ProcessMessage("@getcam_fov=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion

        //
        // Chat, Emotes and Instant Messages
        //

        #region @sendChat
        [Fact] public void CanSendChat() => CheckSimpleCommand("sendChat", m => m.CanSendChat());
        #endregion

        #region @chatshout
        [Fact] public void CanChatShout() => CheckSimpleCommand("chatShout", m => m.CanChatShout());
        #endregion

        #region @chatnormal
        [Fact] public void CanChatNormal() => CheckSimpleCommand("chatNormal", m => m.CanChatNormal());
        #endregion

        #region @chatwhisper
        [Fact] public void CanChatWhisper() => CheckSimpleCommand("chatWhisper", m => m.CanChatWhisper());
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

        #region CanReceiveChat @recvchat @recvchat_sec @recvchatfrom @recvemote @recvemote_sec @recvemotefrom

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

        #region @sendGesture

        [Fact] public void CanSendGesture() => CheckSimpleCommand("sendGesture", m => m.CanSendGesture());

        #endregion

        #region @emote
        [Fact] public void CanEmote() => CheckSimpleCommand("emote", m => m.CanEmote());

        // TODO: Check 'ProcessChat' funcationality (not yet created, but the function doesn't exist yet) to make
        //       sure it no longer censors emotes on @chat=n
        #endregion

        #region @rediremote:<channel_number>=<rem/add>
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

        #region CanChat @sendchat @sendchannel @sendchannel_sec @sendchannel_except

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

        #region @recvim @recvim_sec @recvimto @recvimfrom

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

        //
        // Teleportation
        //

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

        #region @tplm
        [Fact] public void CanTpLm() => CheckSimpleCommand("tpLm", m => m.CanTpLm());
        #endregion

        #region @tploc
        [Fact] public void CanTpLoc() => CheckSimpleCommand("tpLoc", m => m.CanTpLoc());
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

        #region @tpto:<region_name>/<X_local>/<Y_local>/<Z_local>[;lookat]=force

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

        //
        // Inventory, Editing and Rezzing
        //

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

        //
        // Sitting
        //

        #region @unsit
        [Fact] public void CanUnsit() => CheckSimpleCommand("unsit", m => m.CanUnsit());
        #endregion

        #region @sit FORCE
        private void SetObjectExists(UUID objectId, bool isCurrentlySitting)
        {
            _callbacks.Setup(e =>
                e.TryGetObjectExists(objectId, out isCurrentlySitting)
            ).ReturnsAsync(true);
        }

        private void SetCurrentSitId(UUID objectId)
        {
            _callbacks.Setup(e =>
                e.TryGetSitId(out objectId)
            ).ReturnsAsync(objectId != UUID.Zero);
        }

        [Fact]
        public void ForceSit_Default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            SetObjectExists(objectId1, false);

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
            SetObjectExists(objectId1, false);

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
            SetObjectExists(objectId1, true);

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
            SetObjectExists(objectId1, true);

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
            SetObjectExists(objectId1, true);

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

        #region @getsitid=<channel_number>

        [Fact]
        public void GetSitID()
        {
            var actual = _callbacks.RecordReplies();
            SetCurrentSitId(UUID.Zero);

            _rlv.ProcessMessage("@getsitid=1234", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "NULL_KEY"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetSitID_Default()
        {
            var actual = _callbacks.RecordReplies();
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            SetCurrentSitId(objectId1);

            _rlv.ProcessMessage("@getsitid=1234", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, objectId1.ToString()),
            };

            Assert.Equal(expected, actual);
        }

        #endregion

        #region @unsit=force

        [Fact]
        public void ForceUnSit()
        {
            Assert.True(_rlv.ProcessMessage("@unsit=force", _sender.Id, _sender.Name));
        }

        [Fact]
        public void ForceUnSit_RestrictedUnsit()
        {
            _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.ProcessMessage("@unsit=force", _sender.Id, _sender.Name));
        }

        #endregion

        #region @sit
        [Fact] public void CanSit() => CheckSimpleCommand("sit", m => m.CanSit());
        #endregion

        #region @sitground=force

        [Fact]
        public void ForceSitGround()
        {
            Assert.True(_rlv.ProcessMessage("@sitground=force", _sender.Id, _sender.Name));
        }

        [Fact]
        public void ForceSitGround_RestrictedSit()
        {
            _rlv.ProcessMessage("@sit=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.ProcessMessage("@sitground=force", _sender.Id, _sender.Name));
        }

        #endregion

        //
        // Clothing and Attachments
        //

        private void CheckSimpleCommand(string cmd, Func<RLVManager, bool> canFunc)
        {
            _rlv.ProcessMessage($"@{cmd}=n", _sender.Id, _sender.Name);
            Assert.False(canFunc(_rlv.RLVManager));

            _rlv.ProcessMessage($"@{cmd}=y", _sender.Id, _sender.Name);
            Assert.True(canFunc(_rlv.RLVManager));
        }


        // @detach=<y/n>

        // @detach:<attach_point_name>=<y/n>

        // @addattach[:<attach_point_name>]=<y/n>

        // @remattach[:<attach_point_name>]=<y/n>

        #region @defaultwear=<y/n>
        [Fact] public void CanDefaultWear() => CheckSimpleCommand("defaultWear", m => m.CanDefaultWear());
        #endregion

        // @detach[:attachpt]=force @remattach[:attachpt or :uuid]=force

        // @addoutfit[:<part>]=<y/n>

        // @remoutfit[:<part>]=<y/n>

        // @remoutfit[:<part>]=force

        // @getoutfit[:part]=<channel_number>

        // @getattach[:attachpt]=<channel_number>

        // @acceptpermission=<rem/add>

        // @denypermission=<rem/add>

        // @detachme=force

        //
        // Clothing and Attachments (Shared Folders)
        //

        #region @unsharedwear=<y/n>
        [Fact] public void CanUnsharedWear() => CheckSimpleCommand("unsharedWear", m => m.CanUnsharedWear());
        #endregion

        #region @unsharedunwear=<y/n>
        [Fact] public void CanUnsharedUnwear() => CheckSimpleCommand("unsharedUnwear", m => m.CanUnsharedUnwear());
        #endregion

        #region @sharedwear=<y/n>
        [Fact] public void CanSharedWear() => CheckSimpleCommand("sharedWear", m => m.CanSharedWear());
        #endregion

        #region @sharedunwear=<y/n>
        [Fact] public void CanSharedUnwear() => CheckSimpleCommand("sharedUnwear", m => m.CanSharedUnwear());
        #endregion

        // @getinv[:folder1/.../folderN]=<channel_number>

        // @getinvworn[:folder1/.../folderN]=<channel_number>

        // @findfolder:part1[&&...&&partN]=<channel_number>

        // @findfolders:part1[&&...&&partN][;output_separator]=<channel_number>

        // @attach:<folder1/.../folderN>=force

        // @attachover:<folder1/.../folderN>=force

        // @attachoverorreplace:<folder1/.../folderN>=force

        // @attachall:<folder1/.../folderN>=force

        // @attachallover:<folder1/.../folderN>=force

        // @attachalloverorreplace:<folder1/.../folderN>=force

        // @detach:<folder_name>=force

        // @detachall:<folder1/.../folderN>=force

        // @getpath[:<attachpt> or <clothing_layer> or <uuid>]=<channel_number>

        // @getpathnew[:<attachpt> or <clothing_layer> or <uuid>]=<channel_number>

        // @attachthis[:<attachpt> or <clothing_layer>]=force

        // @attachthisover[:<attachpt> or <clothing_layer>]=force

        // @attachthisoverorreplace[:<attachpt> or <clothing_layer>]=force

        // @attachallthis[:<attachpt> or <clothing_layer>]=force

        // @attachallthisover[:<attachpt> or <clothing_layer>]=force

        // @attachallthisoverorreplace[:<attachpt> or <clothing_layer>]=force

        // @detachthis[:<attachpt> or <clothing_layer> or <uuid>]=force

        // @detachallthis[:<attachpt> or <clothing_layer>]=force

        // @detachthis[:<layer>|<attachpt>|<path_to_folder>]=<y/n>

        // @detachallthis[:<layer>|<attachpt>|<path_to_folder>]=<y/n>

        // @attachthis:<layer>|<attachpt>|<path_to_folder>=<y/n>

        // @attachallthis[:<layer>|<attachpt>|<path_to_folder>]=<y/n>

        // @detachthis_except:<folder>=<rem/add>

        // @detachallthis_except:<folder>=<rem/add>

        // @attachthis_except:<folder>=<rem/add>

        // @attachallthis_except:<folder>=<rem/add>

        //
        // Touch
        //

        #region  @touchfar @fartouch[:max_distance]=<y/n>

        [Theory]
        [InlineData("fartouch")]
        [InlineData("touchfar")]
        public void CanFarTouch(string command)
        {
            _rlv.ProcessMessage($"@{command}:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanFarTouch(out var distance));
            Assert.Equal(0.9f, distance);
        }

        [Theory]
        [InlineData("fartouch")]
        [InlineData("touchfar")]
        public void CanFarTouch_Synonym(string command)
        {
            _rlv.ProcessMessage($"@{command}:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanFarTouch(out var distance));
            Assert.Equal(0.9f, distance);
        }

        [Theory]
        [InlineData("fartouch")]
        [InlineData("touchfar")]
        public void CanFarTouch_Default(string command)
        {
            _rlv.ProcessMessage($"@{command}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanFarTouch(out var distance));
            Assert.Equal(1.5f, distance);
        }

        [Theory]
        [InlineData("fartouch", "fartouch")]
        [InlineData("fartouch", "touchfar")]
        [InlineData("touchfar", "touchfar")]
        [InlineData("touchfar", "fartouch")]
        public void CanFarTouch_Multiple_Synonyms(string command1, string command2)
        {
            _rlv.ProcessMessage($"@{command1}:12.34=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@{command2}:6.78=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanFarTouch(out var actualDistance2));

            _rlv.ProcessMessage($"@{command1}:6.78=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanFarTouch(out var actualDistance1));

            Assert.Equal(12.34f, actualDistance1, FloatTolerance);
            Assert.Equal(6.78f, actualDistance2, FloatTolerance);
        }

        #endregion

        #region @touchall=<y/n>

        [Fact]
        public void TouchAll()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("11111111-1111-4111-8111-111111111111");

            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public void TouchAll_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@touchall=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        #endregion

        #region @touchworld=<y/n> @touchworld:<UUID>=<rem/add>

        [Fact]
        public void TouchWorld_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage("@touchworld=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public void TouchWorld_Exception()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage("@touchworld=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@touchworld:{objectId2}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));

            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId2, null, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId2, userId1, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId2, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId2, null, null));
        }

        #endregion

        #region @touchthis:<UUID>=<rem/add>

        [Fact]
        public void TouchThis_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage($"@touchthis:{objectId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));

            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId2, null, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId2, userId1, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId2, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId2, null, null));
        }

        #endregion

        #region @touchme=<rem/add>

        [Fact]
        public void TouchMe_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage("@touchall=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@touchme=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));

            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, _sender.Id, null, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, _sender.Id, userId1, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, _sender.Id, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, _sender.Id, null, null));
        }

        #endregion

        #region @touchattach=<y/n>

        [Fact]
        public void TouchAttach_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage("@touchattach=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        #endregion

        #region @touchattachself=<y/n>

        [Fact]
        public void TouchAttachSelf_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage("@touchattachself=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        #endregion

        #region @touchattachother=<y/n> @touchattachother:<UUID>=<y/n>

        [Fact]
        public void TouchAttachOther_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage("@touchattachother=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public void TouchAttachOther_Specific()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");
            var userId2 = new UUID("66666666-6666-4666-8666-666666666666");

            _rlv.ProcessMessage($"@touchattachother:{userId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId2, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        #endregion

        #region @touchhud[:<UUID>]=<y/n>

        [Fact]
        public void TouchHud_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage($"@touchhud=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public void TouchHud_specific()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage($"@touchhud:{objectId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId2, null, null));
        }

        #endregion

        #region @interact=<y/n>

        [Fact] public void CanInteract() => CheckSimpleCommand("interact", m => m.CanInteract());

        [Fact]
        public void CanInteract_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage($"@interact=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.False(_rlv.RLVManager.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));

            Assert.False(_rlv.RLVManager.CanTouchHud(objectId1));

            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Attached, objectId1));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.RezzedInWorld, objectId1));
            Assert.False(_rlv.RLVManager.CanEdit(RLVManager.ObjectLocation.Hud, objectId1));

            Assert.False(_rlv.RLVManager.CanRez());

            Assert.False(_rlv.RLVManager.CanSit());
        }

        #endregion

        //
        // Location
        //

        #region  @showworldmap=<y/n>
        [Fact] public void CanShowWorldMap() => CheckSimpleCommand("showWorldMap", m => m.CanShowWorldMap());
        #endregion

        #region @showminimap=<y/n>
        [Fact] public void CanShowMiniMap() => CheckSimpleCommand("showMiniMap", m => m.CanShowMiniMap());
        #endregion

        #region @showloc=<y/n>
        [Fact] public void CanShowLoc() => CheckSimpleCommand("showLoc", m => m.CanShowLoc());
        #endregion

        //
        // Name Tags and Hovertext
        //

        #region @shownames[:except_uuid]=<y/n> @shownames_sec[:except_uuid]=<y/n>

        [Fact]
        public void CanShowNames_Default()
        {
            Assert.True(_rlv.RLVManager.CanShowNames(null));
            Assert.True(_rlv.RLVManager.CanShowNames(UUID.Random()));
        }

        [Fact]
        public void CanShowNames()
        {
            _rlv.ProcessMessage("@shownames=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanShowNames(null));
            Assert.False(_rlv.RLVManager.CanShowNames(UUID.Random()));
        }

        [Fact]
        public void CanShowNames_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@shownames=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@shownames:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanShowNames(null));
            Assert.True(_rlv.RLVManager.CanShowNames(userId1));
            Assert.False(_rlv.RLVManager.CanShowNames(userId2));
        }

        [Fact]
        public void CanShowNames_Secure_Default()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@shownames_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanShowNames(null));
            Assert.False(_rlv.RLVManager.CanShowNames(userId1));
            Assert.False(_rlv.RLVManager.CanShowNames(userId2));
        }

        [Fact]
        public void CanShowNames_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@shownames_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@shownames:{userId1}=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@shownames:{userId2}=add", sender2.Id, sender2.Name);

            Assert.False(_rlv.RLVManager.CanShowNames(null));
            Assert.True(_rlv.RLVManager.CanShowNames(userId1));
            Assert.False(_rlv.RLVManager.CanShowNames(userId2));
        }

        #endregion

        #region @shownametags[:uuid]=<y/n>

        [Fact]
        public void CanShowNameTags_Default()
        {
            Assert.True(_rlv.RLVManager.CanShowNameTags(null));
            Assert.True(_rlv.RLVManager.CanShowNameTags(UUID.Random()));
        }

        [Fact]
        public void CanShowNameTags()
        {
            _rlv.ProcessMessage("@shownametags=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanShowNameTags(null));
            Assert.False(_rlv.RLVManager.CanShowNameTags(UUID.Random()));
        }

        [Fact]
        public void CanShowNameTags_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@shownametags=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@shownametags:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.CanShowNameTags(null));
            Assert.True(_rlv.RLVManager.CanShowNameTags(userId1));
            Assert.False(_rlv.RLVManager.CanShowNameTags(userId2));
        }

        #endregion

        #region @shownearby=<y/n>
        [Fact] public void CanShowNearby() => CheckSimpleCommand("showNearby", m => m.CanShowNearby());
        #endregion

        #region @showhovertextall=<y/n>

        [Fact]
        public void CanShowHoverTextAll_Default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverTextAll()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@showhovertextall=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.False(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        #endregion

        #region @showhovertext:<UUID>=<y/n>

        [Fact]
        public void CanShowHoverText_Default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverText()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@showhovertext:{objectId1}=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.False(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));

            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.World, objectId2));
            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.Hud, objectId2));
        }

        #endregion

        #region @showhovertexthud=<y/n>

        [Fact]
        public void CanShowHoverTextHud_Default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverTextHud()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@showhovertexthud=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.False(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));

            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.World, objectId2));
            Assert.False(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.Hud, objectId2));
        }

        #endregion

        #region @showhovertextworld=<y/n>

        [Fact]
        public void CanShowHoverTextWorld_Default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverTextWorld()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@showhovertextworld=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));

            Assert.False(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.World, objectId2));
            Assert.True(_rlv.RLVManager.ShowHoverText(RLVManager.HoverTextLocation.Hud, objectId2));
        }

        #endregion

        //
        // Group
        //

        #region @setgroup:<group_name>=force

        [Fact]
        public void SetGroup_ByName()
        {
            var raised = Assert.Raises<SetGroupEventArgs>(
                 attach: n => _rlv.Actions.SetGroup += n,
                 detach: n => _rlv.Actions.SetGroup -= n,
                 testCode: () => _rlv.ProcessMessage("@setgroup:Group Name=force", _sender.Id, _sender.Name)
            );

            Assert.Equal("Group Name", raised.Arguments.GroupName);
        }

        [Fact]
        public void SetGroup_ById()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            var raised = Assert.Raises<SetGroupEventArgs>(
                 attach: n => _rlv.Actions.SetGroup += n,
                 detach: n => _rlv.Actions.SetGroup -= n,
                 testCode: () => _rlv.ProcessMessage($"@setgroup:{objectId1}=force", _sender.Id, _sender.Name)
            );

            Assert.Equal(string.Empty, raised.Arguments.GroupName);
            Assert.Equal(objectId1, raised.Arguments.GroupId);
        }

        #endregion

        #region @setgroup=<y/n>
        [Fact] public void CanSetGroup() => CheckSimpleCommand("setGroup", m => m.CanSetGroup());
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

        #region @setdebug=<y/n>
        [Fact] public void CanSetDebug() => CheckSimpleCommand("setDebug", m => m.CanSetDebug());
        #endregion

        // @setdebug_<setting>:<value>=force

        // @getdebug_<setting>=<channel_number>

        #region @setenv=<y/n>
        [Fact] public void CanSetEnv() => CheckSimpleCommand("setEnv", m => m.CanSetEnv());
        #endregion

        // @setenv_<setting>:<value>=force

        // @getenv_<setting>=<channel_number>

        //
        // Unofficial Commands
        //

        #region @allowidle=<y/n>
        [Fact] public void CanAllowIdle() => CheckSimpleCommand("allowIdle", m => m.CanAllowIdle());
        #endregion
    }
}
