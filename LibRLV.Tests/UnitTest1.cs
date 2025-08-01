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

        #region RLVA
        //
        // RLVA stuff to implement
        //

        // @getattachnames[:<grp>]=<channel>
        // @getaddattachnames[:<grp>]=<channel>
        // @getremattachnames[:<grp>]=<channel>
        // @getoutfitnames=<channel>
        // @getaddoutfitnames=<channel>
        // @getremoutfitnames=<channel>

        // @fly:[true|false]=force

        // @setcam_eyeoffset[:<vector3>]=force,
        // @setcam_eyeoffsetscale[:<float>]=force
        // @setcam_focusoffset[:<vector3>]=force
        // @setcam_focus:<uuid>[;<dist>[;<direction>]]=force
        // @setcam_mode[:<option>]=force

        // @setcam_focusoffset:<vector3>=n|y
        // @setcam_eyeoffset:<vector3>=n|y
        // @setcam_eyeoffsetscale:<float>=n|y
        // @setcam_mouselook=n|y
        // @setcam=n|y

        // @getcam_avdist=<channel>
        // @getcam_textures=<channel>


        // @setoverlay_tween:[<alpha>];[<tint>];<duration>=force
        // @setoverlay=n|y
        // @setoverlay_touch=n
        // @setsphere=n|y

        // @getcommand[:<behaviour>[;<type>[;<separator>]]]=<channel>
        // @getheightoffset=<channel>

        // @buy=n|y
        // @pay=n|y

        // @showself=n|y
        // @showselfhead=n|y 
        // @viewtransparent=n|y
        // @viewwireframe=n|y


        // Probably don't care about/not going to touch:
        //  @bhvr=n|y
        //  @bhvr:<uuid>=n|y
        //  @bhvr[:<uuid>]=n|y
        //  @bhvr:<modifier>=n|y
        //  @bhvr:<global modifier>=n|y
        //  @bhvr:<local modifier>=force
        //  @bhvr:<modifier>=force
        #endregion


        #region Utilities
        private void CheckSimpleCommand(string cmd, Func<RLVManager, bool> canFunc)
        {
            _rlv.ProcessMessage($"@{cmd}=n", _sender.Id, _sender.Name);
            Assert.False(canFunc(_rlv.Restrictions));

            _rlv.ProcessMessage($"@{cmd}=y", _sender.Id, _sender.Name);
            Assert.True(canFunc(_rlv.Restrictions));
        }


        #endregion

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

            var sitTarget = UUID.Random();

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

            var sitTarget = UUID.Random();

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

            var sitTarget = UUID.Random();

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
            var wornItem = new RlvObject("TargetItem", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            _rlv.ProcessMessage("@notify:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportWornItemChange(UUID.Random(), false, WearableType.Skin, RLV.WornItemChange.Attached);
            _rlv.ReportWornItemChange(UUID.Random(), true, WearableType.Tattoo, RLV.WornItemChange.Attached);

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
            _rlv.ReportWornItemChange(UUID.Random(), false, WearableType.Skin, RLV.WornItemChange.Attached);

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
            _rlv.ReportWornItemChange(UUID.Random(), false, WearableType.Skin, RLV.WornItemChange.Detached);
            _rlv.ReportWornItemChange(UUID.Random(), true, WearableType.Tattoo, RLV.WornItemChange.Detached);

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

            _rlv.ReportWornItemChange(UUID.Random(), false, WearableType.Skin, RLV.WornItemChange.Detached);

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
            _rlv.ReportAttachedItemChange(UUID.Random(), false, AttachmentPoint.Chest, RLV.AttachedItemChange.Attached);
            _rlv.ReportAttachedItemChange(UUID.Random(), true, AttachmentPoint.Skull, RLV.AttachedItemChange.Attached);

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
            _rlv.ReportAttachedItemChange(UUID.Random(), false, AttachmentPoint.Chest, RLV.AttachedItemChange.Attached);

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
            _rlv.ReportAttachedItemChange(UUID.Random(), false, AttachmentPoint.Chest, RLV.AttachedItemChange.Detached);
            _rlv.ReportAttachedItemChange(UUID.Random(), true, AttachmentPoint.Skull, RLV.AttachedItemChange.Detached);

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
            _rlv.ReportAttachedItemChange(UUID.Random(), false, AttachmentPoint.Chest, RLV.AttachedItemChange.Detached);

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
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

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
        [Fact]
        public void CanFly()
        {
            CheckSimpleCommand("fly", m => m.CanFly());
        }
        #endregion

        #region @jump (RLVa)
        [Fact]
        public void CanJump()
        {
            CheckSimpleCommand("jump", m => m.CanJump());
        }
        #endregion

        #region @temprun
        [Fact]
        public void CanTempRun()
        {
            CheckSimpleCommand("tempRun", m => m.CanTempRun());
        }
        #endregion

        #region @alwaysrun
        [Fact]
        public void CanAlwaysRun()
        {
            CheckSimpleCommand("alwaysRun", m => m.CanAlwaysRun());
        }
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
            Assert.False(_rlv.Restrictions.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(default, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Single()
        {
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(1.5f, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Multiple_SingleSender()
        {
            _rlv.ProcessMessage("@CamZoomMin:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:4.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamZoomMin(out var camZoomMin));
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

            Assert.True(_rlv.Restrictions.HasCamZoomMin(out var camZoomMin));
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

            Assert.True(_rlv.Restrictions.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(4.5f, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Off()
        {
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:1.5=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(default, camZoomMin);
        }
        #endregion

        #region CamMaxFunctionsThrough
        [Fact]
        public void CamZoomMax_Default()
        {
            Assert.False(_rlv.Restrictions.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(default, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Single()
        {
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Multiple_SingleSender()
        {
            _rlv.ProcessMessage("@CamZoomMax:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:4.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamZoomMax(out var camZoomMax));
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

            Assert.True(_rlv.Restrictions.HasCamZoomMax(out var camZoomMax));
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

            Assert.True(_rlv.Restrictions.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Off()
        {
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:1.5=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(default, camZoomMax);
        }

        #endregion

        #region @CamZoomMin
        [Fact]
        public void CamZoomMin()
        {
            _rlv.ProcessMessage("@CamZoomMin:0.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(0.5f, camZoomMin);
        }
        #endregion

        #region @CamZoomMax
        [Fact]
        public void CamZoomMax()
        {
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }
        #endregion

        #region @setcam_fovmin
        [Fact]
        public void SetCamFovMin()
        {
            _rlv.ProcessMessage("@setcam_fovmin:15=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasSetCamFovMin(out var setCamFovMin));
            Assert.Equal(15f, setCamFovMin);
        }
        #endregion

        #region @setcam_fovmax
        [Fact]
        public void SetCamFovMax()
        {
            _rlv.ProcessMessage("@setcam_fovmax:45=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasSetCamFovMax(out var setCamFovMax));
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
            var raisedEvent = false;
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
            var raisedEvent = false;
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

            Assert.True(_rlv.Restrictions.HasSetCamAvDistMax(out var setCamAvDistMax));
            Assert.Equal(30f, setCamAvDistMax);
        }
        [Fact]
        public void SetCamAvDistMax_Synonym()
        {
            _rlv.ProcessMessage("@camdistmax:30=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasSetCamAvDistMax(out var setCamAvDistMax));
            Assert.Equal(30f, setCamAvDistMax);
        }
        #endregion

        #region @setcam_avdistmin
        [Fact]
        public void SetCamAvDistMin()
        {
            _rlv.ProcessMessage("@setcam_avdistmin:0.3=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasSetCamAvDistMin(out var setCamAvDistMin));
            Assert.Equal(0.3f, setCamAvDistMin);
        }

        [Fact]
        public void SetCamAvDistMin_Synonym()
        {
            _rlv.ProcessMessage("@camdistmin:0.3=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasSetCamAvDistMin(out var setCamAvDistMin));
            Assert.Equal(0.3f, setCamAvDistMin);
        }
        #endregion

        #region @CamDrawAlphaMax
        [Fact]
        public void CamDrawAlphaMax()
        {
            _rlv.ProcessMessage("@CamDrawAlphaMax:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamDrawAlphaMax(out var camDrawAlphaMax));
            Assert.Equal(0.9f, camDrawAlphaMax);
        }
        #endregion

        #region @camdrawmin:<min_distance>=<y/n>

        [Fact]
        public void CamDrawMin()
        {
            _rlv.ProcessMessage("@camdrawmin:1.75=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamDrawMin(out var camDrawMin));
            Assert.Equal(1.75f, camDrawMin);
        }

        [Fact]
        public void CamDrawMin_Small()
        {
            Assert.False(_rlv.ProcessMessage("@camdrawmin:0.15=n", _sender.Id, _sender.Name));
            Assert.False(_rlv.Restrictions.HasCamDrawMin(out var camDrawMin));
        }

        #endregion

        #region @camdrawmax:<max_distance>=<y/n>

        [Fact]
        public void CamDrawMax()
        {
            _rlv.ProcessMessage("@camdrawmax:1.75=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamDrawMax(out var camDrawMax));
            Assert.Equal(1.75f, camDrawMax);
        }

        [Fact]
        public void CamDrawMax_Small()
        {
            Assert.False(_rlv.ProcessMessage("@camdrawmax:0.15=n", _sender.Id, _sender.Name));
            Assert.False(_rlv.Restrictions.HasCamDrawMax(out var camDrawMax));
        }

        #endregion

        #region @camdrawalphamin:<min_distance>=<y/n>

        [Fact]
        public void CamDrawAlphaMin()
        {
            _rlv.ProcessMessage("@camdrawalphamin:1.75=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamDrawAlphaMin(out var camDrawAlphaMin));
            Assert.Equal(1.75f, camDrawAlphaMin);
        }

        #endregion

        #region @CamDrawColor

        [Fact]
        public void CamDrawColor()
        {
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamDrawColor(out var color));

            Assert.Equal(0.1f, color.X, FloatTolerance);
            Assert.Equal(0.2f, color.Y, FloatTolerance);
            Assert.Equal(0.3f, color.Z, FloatTolerance);
        }

        [Fact]
        public void CamDrawColor_Default()
        {
            Assert.False(_rlv.Restrictions.HasCamDrawColor(out var color));
        }

        [Fact]
        public void CamDrawColor_Large()
        {
            _rlv.ProcessMessage("@CamDrawColor:5;6;7=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamDrawColor(out var color));
            Assert.Equal(1.0f, color.X, FloatTolerance);
            Assert.Equal(1.0f, color.Y, FloatTolerance);
            Assert.Equal(1.0f, color.Z, FloatTolerance);
        }

        [Fact]
        public void CamDrawColor_Negative()
        {
            _rlv.ProcessMessage("@CamDrawColor:-5;-6;-7=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamDrawColor(out var color));
            Assert.Equal(0.0f, color.X, FloatTolerance);
            Assert.Equal(0.0f, color.Y, FloatTolerance);
            Assert.Equal(0.0f, color.Z, FloatTolerance);
        }

        [Fact]
        public void CamDrawColor_Removal()
        {
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.HasCamDrawColor(out var color));
        }

        [Fact]
        public void CamDrawColor_Multi()
        {
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamDrawColor:0.2;0.3;0.6=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamDrawColor(out var color));
            Assert.Equal(0.15f, color.X, FloatTolerance);
            Assert.Equal(0.25f, color.Y, FloatTolerance);
            Assert.Equal(0.45f, color.Z, FloatTolerance);
        }

        #endregion

        #region @camunlock
        [Fact]
        public void CanSetCamUnlock()
        {
            CheckSimpleCommand("setcam_unlock", m => !m.IsCamLocked());
        }
        #endregion

        #region @setcam_unlock
        [Fact]
        public void CanCamUnlock()
        {
            CheckSimpleCommand("camunlock", m => !m.IsCamLocked());
        }
        #endregion

        #region @camavdist
        [Fact]
        public void CamAvDist()
        {
            _rlv.ProcessMessage("@CamAvDist:5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasCamAvDist(out var camAvDist));
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

            Assert.True(_rlv.Restrictions.HasSetCamtextures(out var actualTextureId));

            Assert.Equal(UUID.Zero, actualTextureId);
        }

        [Theory]
        [InlineData("setcam_textures")]
        [InlineData("camtextures")]
        public void SetCamTextures_Single(string command)
        {
            var textureId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage($"@{command}:{textureId1}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasSetCamtextures(out var actualTextureId));

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

            Assert.True(_rlv.Restrictions.HasSetCamtextures(out var actualTextureId2));

            _rlv.ProcessMessage($"@{command1}:{textureId2}=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.HasSetCamtextures(out var actualTextureId1));

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
        [Fact]
        public void CanSendChat()
        {
            CheckSimpleCommand("sendChat", m => m.CanSendChat());
        }
        #endregion

        #region @chatshout
        [Fact]
        public void CanChatShout()
        {
            CheckSimpleCommand("chatShout", m => m.CanChatShout());
        }
        #endregion

        #region @chatnormal
        [Fact]
        public void CanChatNormal()
        {
            CheckSimpleCommand("chatNormal", m => m.CanChatNormal());
        }
        #endregion

        #region @chatwhisper
        [Fact]
        public void CanChatWhisper()
        {
            CheckSimpleCommand("chatWhisper", m => m.CanChatWhisper());
        }
        #endregion

        #region @redirchat

        [Fact]
        public void IsRedirChat()
        {
            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsRedirChat(out var channels));

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

            Assert.False(_rlv.Restrictions.IsRedirChat(out var channels));
        }

        [Fact]
        public void IsRedirChat_MultipleChannels()
        {
            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@redirchat:12345=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsRedirChat(out var channels));

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
            _rlv.ReportSendPublicMessage("Hello World");

            Assert.True(_rlv.Restrictions.IsRedirChat(out var channels));
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

            _rlv.ReportSendPublicMessage("Hello World");
            _rlv.Restrictions.IsRedirChat(out var channels);

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

            _rlv.ReportSendPublicMessage("/me says Hello World");

            Assert.True(_rlv.Restrictions.IsRedirChat(out var channels));
            Assert.Empty(actual);
        }

        #endregion

        #region CanReceiveChat @recvchat @recvchat_sec @recvchatfrom @recvemote @recvemote_sec @recvemotefrom

        [Fact]
        public void CanRecvChat_Default()
        {
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", UUID.Random()));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", UUID.Random()));
        }

        [Fact]
        public void CanRecvChat()
        {
            _rlv.ProcessMessage("@recvchat=n", _sender.Id, _sender.Name);
            var userId = UUID.Random();

            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", null));
            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", userId));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId));
        }

        [Fact]
        public void CanRecvChat_Except()
        {
            var userId = UUID.Random();

            _rlv.ProcessMessage("@recvchat=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvchat:{userId}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId));
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

            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", null));
            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId2));
        }

        [Fact]
        public void CanRecvChat_RecvEmote()
        {
            _rlv.ProcessMessage("@recvemote=n", _sender.Id, _sender.Name);
            var userId = UUID.Random();

            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId));
            Assert.False(_rlv.Restrictions.CanReceiveChat("/me says Hello world", null));
            Assert.False(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId));
        }

        [Fact]
        public void CanRecvChat_RecvEmoteFrom()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvemotefrom:{userId1}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId2));
            Assert.False(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public void CanRecvChat_RecvEmote_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@recvemote=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvemote:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId2));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId1));
            Assert.False(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId2));
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

            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId2));
            Assert.False(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public void CanRecvChatFrom()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvchatfrom:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId1));

            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId2));
        }

        #endregion

        #region @sendGesture

        [Fact]
        public void CanSendGesture()
        {
            CheckSimpleCommand("sendGesture", m => m.CanSendGesture());
        }

        #endregion

        #region @emote
        [Fact]
        public void CanEmote()
        {
            CheckSimpleCommand("emote", m => m.CanEmote());
        }

        // TODO: Check 'ProcessChat' funcationality (not yet created, but the function doesn't exist yet) to make
        //       sure it no longer censors emotes on @chat=n
        #endregion

        #region @rediremote:<channel_number>=<rem/add>
        [Fact]
        public void IsRedirEmote()
        {
            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsRedirEmote(out var channels));

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

            Assert.False(_rlv.Restrictions.IsRedirEmote(out var channels));
        }

        [Fact]
        public void IsRedirEmote_MultipleChannels()
        {
            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@rediremote:12345=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsRedirEmote(out var channels));

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
            _rlv.ReportSendPublicMessage("/me says Hello World");

            Assert.True(_rlv.Restrictions.IsRedirEmote(out var channels));
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

            _rlv.ReportSendPublicMessage("/me says Hello World");
            _rlv.Restrictions.IsRedirEmote(out var channels);

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
            _rlv.ReportSendPublicMessage("Hello World");

            Assert.True(_rlv.Restrictions.IsRedirEmote(out var channels));
            Assert.Empty(actual);
        }

        #endregion

        #region CanChat @sendchat @sendchannel @sendchannel_sec @sendchannel_except

        [Fact]
        public void CanChat_Default()
        {
            Assert.True(_rlv.Restrictions.CanChat(0, "Hello"));
            Assert.True(_rlv.Restrictions.CanChat(0, "/me says Hello"));
            Assert.True(_rlv.Restrictions.CanChat(5, "Hello"));
        }

        [Fact]
        public void CanChat_SendChatRestriction()
        {
            _rlv.ProcessMessage("@sendchat=n", _sender.Id, _sender.Name);

            // No public chat allowed unless it starts with '/'
            Assert.False(_rlv.Restrictions.CanChat(0, "Hello"));

            // Emotes and other messages starting with / are allowed
            Assert.True(_rlv.Restrictions.CanChat(0, "/me says Hello"));
            Assert.True(_rlv.Restrictions.CanChat(0, "/ something?"));

            // Messages containing ()"-*=_^ are prohibited
            Assert.False(_rlv.Restrictions.CanChat(0, "/me says Hello ^_^"));

            // Private channels are not impacted
            Assert.True(_rlv.Restrictions.CanChat(5, "Hello"));
        }

        [Fact]
        public void CanSendChannel_Default()
        {
            Assert.True(_rlv.Restrictions.CanChat(123, "Hello world"));
        }

        [Fact]
        public void CanSendChannel()
        {
            _rlv.ProcessMessage("@sendchannel=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanChat(123, "Hello world"));
        }

        [Fact]
        public void CanSendChannel_Exception()
        {
            _rlv.ProcessMessage("@sendchannel=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendchannel:123=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanChat(123, "Hello world"));
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

            Assert.True(_rlv.Restrictions.CanChat(123, "Hello world"));
            Assert.False(_rlv.Restrictions.CanChat(456, "Hello world"));
        }

        [Fact]
        public void CanSendChannelExcept()
        {
            _rlv.ProcessMessage("@sendchannel_except:456=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanChat(123, "Hello world"));
            Assert.False(_rlv.Restrictions.CanChat(456, "Hello world"));
        }

        #endregion

        #region @sendim @sendim_sec @sendimto

        [Fact]
        public void CanSendIM_Default()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanSendIM("Hello", userId1));
            Assert.True(_rlv.Restrictions.CanSendIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanSendIM()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanSendIM("Hello", userId1));
            Assert.False(_rlv.Restrictions.CanSendIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanSendIM_Exception()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", userId1));
        }

        [Fact]
        public void CanSendIM_Exception_SingleGroup()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:Group Name=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Group Name"));
        }

        [Fact]
        public void CanSendIM_Exception_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Group name"));
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

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", userId1));
            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", userId2));
        }

        [Fact]
        public void CanSendIM_Secure_Group()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:Group Name=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:allgroups=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Group Name"));
            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanSendIM_Secure_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Group Name"));
            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanSendIMTo()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@sendimto:{userId1}=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", userId2));
        }

        [Fact]
        public void CanSendIMTo_Group()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var groupId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@sendimto:First Group=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "First Group"));
            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId2, "Second Group"));
        }

        [Fact]
        public void CanSendIMTo_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var groupId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@sendimto:allgroups=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "First Group"));
            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", groupId2, "Second Group"));
        }

        #endregion

        #region @startim @startimto

        [Fact]
        public void CanStartIM_Default()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanStartIM(null));
            Assert.True(_rlv.Restrictions.CanStartIM(userId1));
        }

        [Fact]
        public void CanStartIM()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@startim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanStartIM(null));
            Assert.False(_rlv.Restrictions.CanStartIM(userId1));
        }

        [Fact]
        public void CanStartIM_Exception()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@startim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@startim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanStartIM(userId1));
            Assert.False(_rlv.Restrictions.CanStartIM(userId2));
        }

        [Fact]
        public void CanStartIMTo()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@startimto:{userId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanStartIM(userId1));
            Assert.False(_rlv.Restrictions.CanStartIM(userId2));
        }

        #endregion

        #region @recvim @recvim_sec @recvimto @recvimfrom

        [Fact]
        public void CanReceiveIM_Default()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanReceiveIM()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello", userId1));
            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanReceiveIM_Exception()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", userId1));
        }

        [Fact]
        public void CanReceiveIM_Exception_SingleGroup()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:Group Name=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Group Name"));
        }

        [Fact]
        public void CanReceiveIM_Exception_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Group name"));
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

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", userId1));
            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", userId2));
        }

        [Fact]
        public void CanReceiveIM_Secure_Group()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:Group Name=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:allgroups=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Group Name"));
            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanReceiveIM_Secure_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Group Name"));
            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanReceiveIMFrom()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvimfrom:{userId1}=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", userId2));
        }

        [Fact]
        public void CanReceiveIMFrom_Group()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var groupId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvimfrom:First Group=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "First Group"));
            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId2, "Second Group"));
        }

        [Fact]
        public void CanReceiveIMTo_AllGroups()
        {
            var groupId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var groupId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvimfrom:allgroups=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "First Group"));
            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", groupId2, "Second Group"));
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

            Assert.True(_rlv.Restrictions.CanTpLocal(out var distance));
            Assert.Equal(0.0f, distance, FloatTolerance);
        }

        [Fact]
        public void CanTpLocal()
        {
            _rlv.ProcessMessage("@TpLocal:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanTpLocal(out var distance));
            Assert.Equal(0.9f, distance, FloatTolerance);
        }
        #endregion

        #region @tplm
        [Fact]
        public void CanTpLm()
        {
            CheckSimpleCommand("tpLm", m => m.CanTpLm());
        }
        #endregion

        #region @tploc
        [Fact]
        public void CanTpLoc()
        {
            CheckSimpleCommand("tpLoc", m => m.CanTpLoc());
        }
        #endregion

        #region @tplure @tplure_sec 

        [Fact]
        public void CanTpLure_Default()
        {
            Assert.True(_rlv.Restrictions.CanTPLure(null));
            Assert.True(_rlv.Restrictions.CanTPLure(UUID.Random()));
        }

        [Fact]
        public void CanTpLure()
        {
            _rlv.ProcessMessage("@tplure=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanTPLure(null));
            Assert.False(_rlv.Restrictions.CanTPLure(UUID.Random()));
        }

        [Fact]
        public void CanTpLure_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@tplure=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@tplure:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanTPLure(null));
            Assert.True(_rlv.Restrictions.CanTPLure(userId1));
            Assert.False(_rlv.Restrictions.CanTPLure(userId2));
        }

        [Fact]
        public void CanTpLure_Secure_Default()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@tplure_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanTPLure(null));
            Assert.False(_rlv.Restrictions.CanTPLure(userId1));
            Assert.False(_rlv.Restrictions.CanTPLure(userId2));
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

            Assert.False(_rlv.Restrictions.CanTPLure(null));
            Assert.True(_rlv.Restrictions.CanTPLure(userId1));
            Assert.False(_rlv.Restrictions.CanTPLure(userId2));
        }

        #endregion

        #region @sittp

        [Fact]
        public void CanSitTp_Default()
        {
            Assert.False(_rlv.Restrictions.CanSitTp(out var maxDistance));
            Assert.Equal(1.5f, maxDistance);
        }

        [Fact]
        public void CanSitTp_Single()
        {
            _rlv.ProcessMessage("@SitTp:2.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanSitTp(out var maxDistance));
            Assert.Equal(2.5f, maxDistance);
        }

        [Fact]
        public void CanSitTp_Multiple_SingleSender()
        {
            _rlv.ProcessMessage("@SitTp:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@SitTp:4.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@SitTp:2.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanSitTp(out var maxDistance));
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

            Assert.True(_rlv.Restrictions.CanSitTp(out var maxDistance));
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

            Assert.True(_rlv.Restrictions.CanSitTp(out var maxDistance));
            Assert.Equal(2.5f, maxDistance);
        }

        [Fact]
        public void CanSitTp_Off()
        {
            _rlv.ProcessMessage("@SitTp:2.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@SitTp:2.5=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanSitTp(out var maxDistance));
            Assert.Equal(1.5f, maxDistance);
        }
        #endregion

        #region @standtp
        [Fact]
        public void CanStandTp()
        {
            CheckSimpleCommand("standTp", m => m.CanStandTp());
        }
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

            var raisedEvent = false;
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

            var raisedEvent = false;
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

            Assert.False(_rlv.Restrictions.IsAutoAcceptTp(userId1));
            Assert.False(_rlv.Restrictions.IsAutoAcceptTp(userId2));
            Assert.False(_rlv.Restrictions.IsAutoAcceptTp());
        }

        [Fact]
        public void CanAutoAcceptTp_User()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@accepttp:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsAutoAcceptTp(userId1));
            Assert.False(_rlv.Restrictions.IsAutoAcceptTp(userId2));
            Assert.False(_rlv.Restrictions.IsAutoAcceptTp());
        }

        [Fact]
        public void CanAutoAcceptTp_All()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@accepttp=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsAutoAcceptTp(userId1));
            Assert.True(_rlv.Restrictions.IsAutoAcceptTp(userId2));
            Assert.True(_rlv.Restrictions.IsAutoAcceptTp());
        }

        #endregion

        #region @accepttprequest

        [Fact]
        public void CanAutoAcceptTpRequest_Default()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            Assert.False(_rlv.Restrictions.IsAutoAcceptTpRequest(userId1));
            Assert.False(_rlv.Restrictions.IsAutoAcceptTpRequest(userId2));
            Assert.False(_rlv.Restrictions.IsAutoAcceptTpRequest());
        }

        [Fact]
        public void CanAutoAcceptTpRequest_User()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@accepttprequest:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsAutoAcceptTpRequest(userId1));
            Assert.False(_rlv.Restrictions.IsAutoAcceptTpRequest(userId2));
            Assert.False(_rlv.Restrictions.IsAutoAcceptTpRequest());
        }

        [Fact]
        public void CanAutoAcceptTpRequest_All()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@accepttprequest=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsAutoAcceptTpRequest(userId1));
            Assert.True(_rlv.Restrictions.IsAutoAcceptTpRequest(userId2));
            Assert.True(_rlv.Restrictions.IsAutoAcceptTpRequest());
        }

        #endregion

        #region @tprequest @tprequest_sec

        [Fact]
        public void CanTpRequest_Default()
        {
            Assert.True(_rlv.Restrictions.CanTpRequest(null));
            Assert.True(_rlv.Restrictions.CanTpRequest(UUID.Random()));
        }

        [Fact]
        public void CanTpRequest()
        {
            _rlv.ProcessMessage("@tprequest=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanTpRequest(null));
            Assert.False(_rlv.Restrictions.CanTpRequest(UUID.Random()));
        }

        [Fact]
        public void CanTpRequest_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@tprequest=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@tprequest:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanTpRequest(null));
            Assert.True(_rlv.Restrictions.CanTpRequest(userId1));
            Assert.False(_rlv.Restrictions.CanTpRequest(userId2));
        }

        [Fact]
        public void CanTpRequest_Secure_Default()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@tprequest_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanTpRequest(null));
            Assert.False(_rlv.Restrictions.CanTpRequest(userId1));
            Assert.False(_rlv.Restrictions.CanTpRequest(userId2));
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

            Assert.False(_rlv.Restrictions.CanTpRequest(null));
            Assert.True(_rlv.Restrictions.CanTpRequest(userId1));
            Assert.False(_rlv.Restrictions.CanTpRequest(userId2));
        }

        #endregion

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
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

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
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

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
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

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
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

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
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

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
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

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
            Assert.True(_rlv.Restrictions.CanShare(null));
            Assert.True(_rlv.Restrictions.CanShare(UUID.Random()));
        }

        [Fact]
        public void CanShare()
        {
            _rlv.ProcessMessage("@share=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShare(null));
            Assert.False(_rlv.Restrictions.CanShare(UUID.Random()));
        }

        [Fact]
        public void CanShare_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@share=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@share:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShare(null));
            Assert.True(_rlv.Restrictions.CanShare(userId1));
            Assert.False(_rlv.Restrictions.CanShare(userId2));
        }

        [Fact]
        public void CanShare_Secure_Default()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@share_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShare(null));
            Assert.False(_rlv.Restrictions.CanShare(userId1));
            Assert.False(_rlv.Restrictions.CanShare(userId2));
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

            Assert.False(_rlv.Restrictions.CanShare(null));
            Assert.True(_rlv.Restrictions.CanShare(userId1));
            Assert.False(_rlv.Restrictions.CanShare(userId2));
        }

        #endregion

        //
        // Sitting
        //

        #region @unsit
        [Fact]
        public void CanUnsit()
        {
            CheckSimpleCommand("unsit", m => m.CanUnsit());
        }
        #endregion

        #region @sit:<uuid>=force
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

            var raisedEvent = false;
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

            var raisedEvent = false;
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

            var raisedEvent = false;
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

            var raisedEvent = false;
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
        [Fact]
        public void CanSit()
        {
            CheckSimpleCommand("sit", m => m.CanSit());
        }
        #endregion

        #region @sitground=force

        [Fact]
        public void ForceSitGround()
        {
            // TODO: Check reaction
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

        #region @detach=<y/n> |  @detach:<attach_point_name>=<y/n>

        [Fact]
        public void Detach_Default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

            var folderId1 = new UUID("99999999-9999-4999-8999-999999999999");

            Assert.True(_rlv.Restrictions.CanDetach(folderId1, false, null, null));
            Assert.True(_rlv.Restrictions.CanDetach(folderId1, false, AttachmentPoint.Chest, null));
            Assert.True(_rlv.Restrictions.CanDetach(folderId1, false, null, WearableType.Shirt));

            Assert.True(_rlv.Restrictions.CanDetach(folderId1, true, null, null));
            Assert.True(_rlv.Restrictions.CanDetach(folderId1, true, AttachmentPoint.Chest, null));
            Assert.True(_rlv.Restrictions.CanDetach(folderId1, true, null, WearableType.Shirt));
        }

        [Fact]
        public void Detach()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

            var folderId1 = new UUID("99999999-9999-4999-8999-999999999999");

            Assert.True(_rlv.ProcessMessage("@detach=n", _sender.Id, _sender.Name));

            Assert.False(_rlv.Restrictions.CanDetach(folderId1, false, null, null));
            Assert.False(_rlv.Restrictions.CanDetach(folderId1, false, AttachmentPoint.Chest, null));
            Assert.False(_rlv.Restrictions.CanDetach(folderId1, false, null, WearableType.Shirt));

            Assert.False(_rlv.Restrictions.CanDetach(folderId1, true, null, null));
            Assert.False(_rlv.Restrictions.CanDetach(folderId1, true, AttachmentPoint.Chest, null));
            Assert.False(_rlv.Restrictions.CanDetach(folderId1, true, null, WearableType.Shirt));
        }

        [Fact]
        public void Detach_AttachPoint()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

            var folderId1 = new UUID("99999999-9999-4999-8999-999999999999");

            Assert.True(_rlv.ProcessMessage("@detach:skull=n", _sender.Id, _sender.Name));

            Assert.True(_rlv.Restrictions.CanDetach(folderId1, false, null, null));
            Assert.True(_rlv.Restrictions.CanDetach(folderId1, false, AttachmentPoint.Chest, null));
            Assert.False(_rlv.Restrictions.CanDetach(folderId1, false, AttachmentPoint.Skull, null));
            Assert.True(_rlv.Restrictions.CanDetach(folderId1, false, null, WearableType.Shirt));

            Assert.True(_rlv.Restrictions.CanDetach(folderId1, true, null, null));
            Assert.True(_rlv.Restrictions.CanDetach(folderId1, true, AttachmentPoint.Chest, null));
            Assert.False(_rlv.Restrictions.CanDetach(folderId1, true, AttachmentPoint.Skull, null));
            Assert.True(_rlv.Restrictions.CanDetach(folderId1, true, null, WearableType.Shirt));
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
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));
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
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));
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
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));
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
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));
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
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
                 attach: n => _rlv.Actions.RemOutfit += n,
                 detach: n => _rlv.Actions.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit=force", _sender.Id, _sender.Name)
            );

            var expected = new List<UUID>()
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

            var externalWearable = new InventoryTree.InventoryItem()
            {
                Name = "External Tattoo",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                WornOn = WearableType.Tattoo,
                AttachedTo = null,
                Id = new UUID("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa")
            };
            var externalAttachable = new InventoryTree.InventoryItem()
            {
                Name = "External Jaw Thing",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                AttachedTo = AttachmentPoint.Jaw,
                WornOn = null,
                Id = new UUID("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa")
            };

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<RemOutfitEventArgs>(
                 attach: n => _rlv.Actions.RemOutfit += n,
                 detach: n => _rlv.Actions.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit=force", _sender.Id, _sender.Name)
            );

            var expected = new List<UUID>()
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

            var externalWearable = new InventoryTree.InventoryItem()
            {
                Name = "External Tattoo",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                WornOn = WearableType.Tattoo,
                AttachedTo = null,
                Id = new UUID("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa")
            };
            var externalAttachable = new InventoryTree.InventoryItem()
            {
                Name = "External Jaw Thing",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                AttachedTo = AttachmentPoint.Jaw,
                WornOn = null,
                Id = new UUID("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa")
            };

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<RemOutfitEventArgs>(
                 attach: n => _rlv.Actions.RemOutfit += n,
                 detach: n => _rlv.Actions.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit:tattoo=force", _sender.Id, _sender.Name)
            );

            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.RemOutfit += n,
                 detach: n => _rlv.Actions.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit:Clothing/Hats=force", _sender.Id, _sender.Name)
            );

            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.RemOutfit += n,
                 detach: n => _rlv.Actions.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit:tattoo=force", _sender.Id, _sender.Name)
            );

            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.RemOutfit += n,
                 detach: n => _rlv.Actions.RemOutfit -= n,
                 testCode: () => _rlv.ProcessMessage("@remoutfit:skin=force", _sender.Id, _sender.Name)
            );

            var expected = new List<UUID>()
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
            var externalWearable = new InventoryTree.InventoryItem()
            {
                Name = "External Tattoo",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                WornOn = WearableType.Tattoo,
                AttachedTo = null,
                Id = new UUID("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa")
            };
            var externalAttachable = new InventoryTree.InventoryItem()
            {
                Name = "External Jaw Thing",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                AttachedTo = AttachmentPoint.Jaw,
                WornOn = null,
                Id = new UUID("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa")
            };

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
                new()
                {
                    WornOn = WearableType.Socks,
                    FolderId = new UUID("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                    AttachedTo = null,
                    Name = $"My Socks",
                    Id = new UUID($"c0000000-cccc-4ccc-8ccc-cccccccccccc")
                },
                new()
                {
                    WornOn = WearableType.Hair,
                    FolderId = new UUID("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                    AttachedTo = null,
                    Name = $"My Hair",
                    Id = new UUID($"c0000001-cccc-4ccc-8ccc-cccccccccccc")
                },
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

                currentOutfit.Add(new InventoryTree.InventoryItem()
                {
                    WornOn = item,
                    FolderId = new UUID("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                    AttachedTo = null,
                    Name = $"My {item}",
                    Id = new UUID($"c{(int)item:D7}-cccc-4ccc-8ccc-cccccccccccc")
                });
            }
            ;

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
                new()
                {
                    WornOn = WearableType.Socks,
                    FolderId = new UUID("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                    AttachedTo = null,
                    Name = $"My Socks",
                    Id = new UUID($"c0000000-cccc-4ccc-8ccc-cccccccccccc")
                },
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
                new()
                {
                    WornOn = WearableType.Hair,
                    FolderId = new UUID("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                    AttachedTo = null,
                    Name = $"My Hair",
                    Id = new UUID($"c0000001-cccc-4ccc-8ccc-cccccccccccc")
                },
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
            var externalWearable = new InventoryTree.InventoryItem()
            {
                Name = "External Tattoo",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                WornOn = WearableType.Tattoo,
                AttachedTo = null,
                Id = new UUID("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa")
            };
            var externalAttachable = new InventoryTree.InventoryItem()
            {
                Name = "External Jaw Thing",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                AttachedTo = AttachmentPoint.Jaw,
                WornOn = null,
                Id = new UUID("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa")
            };

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
                new()
                {
                    WornOn = null,
                    FolderId = new UUID("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                    AttachedTo = AttachmentPoint.LeftFoot,
                    Name = $"My Socks",
                    Id = new UUID($"c0000000-cccc-4ccc-8ccc-cccccccccccc")
                },
                new()
                {
                    WornOn = null,
                    FolderId = new UUID("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                    AttachedTo = AttachmentPoint.Skull,
                    Name = $"My Hair",
                    Id = new UUID($"c0000001-cccc-4ccc-8ccc-cccccccccccc")
                },
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
                currentAttach.Add(new InventoryTree.InventoryItem()
                {
                    WornOn = null,
                    FolderId = new UUID("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                    AttachedTo = item,
                    Name = $"My {item}",
                    Id = new UUID($"c{(int)item:D7}-cccc-4ccc-8ccc-cccccccccccc")
                });
            }
            ;

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
                new()
                {
                    WornOn = null,
                    FolderId = new UUID("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                    AttachedTo = AttachmentPoint.LeftFoot,
                    Name = $"My Sock",
                    Id = new UUID($"c0000000-cccc-4ccc-8ccc-cccccccccccc")
                },
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
                new()
                {
                    WornOn = null,
                    FolderId = new UUID("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                    AttachedTo = AttachmentPoint.Skull,
                    Name = $"My Hair",
                    Id = new UUID($"c0000001-cccc-4ccc-8ccc-cccccccccccc")
                },
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
            Assert.True(_rlv.Restrictions.IsAutoAcceptPermissions());

            Assert.True(_rlv.ProcessMessage($"@acceptpermission=rem", _sender.Id, _sender.Name));
            Assert.False(_rlv.Restrictions.IsAutoAcceptPermissions());
        }
        #endregion

        #region @denypermission=<rem/add>
        [Fact]
        public void DenyPermission()
        {
            Assert.True(_rlv.ProcessMessage($"@denypermission=add", _sender.Id, _sender.Name));
            Assert.True(_rlv.Restrictions.IsAutoDenyPermissions());

            Assert.True(_rlv.ProcessMessage($"@denypermission=rem", _sender.Id, _sender.Name));
            Assert.False(_rlv.Restrictions.IsAutoDenyPermissions());
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachme=force", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name)
            );

            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachme=force", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name)
            );

            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Attach += n,
                 detach: n => _rlv.Actions.Attach -= n,
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage(command, _sender.Id, _sender.Name)
            );

            // Remove everything except for clothing despite what you would think. Just how things go.
            var expected = new List<UUID>()
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

            var externalWearable = new InventoryTree.InventoryItem()
            {
                Name = "External Tattoo",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                WornOn = WearableType.Tattoo,
                AttachedTo = null,
                Id = new UUID("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa")
            };
            var externalAttachable = new InventoryTree.InventoryItem()
            {
                Name = "External Jaw Thing",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                AttachedTo = AttachmentPoint.Jaw,
                WornOn = null,
                Id = new UUID("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa")
            };

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage(command, _sender.Id, _sender.Name)
            );

            // Remove everything except for clothing despite what you would think. Just how things go.
            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage(command, _sender.Id, _sender.Name)
            );

            var expected = new List<UUID>()
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

            var externalAttachable = new InventoryTree.InventoryItem()
            {
                Name = "External Groin Thing",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                AttachedTo = AttachmentPoint.Groin,
                WornOn = null,
                Id = new UUID("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa")
            };

            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage(command, _sender.Id, _sender.Name)
            );

            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage(command, _sender.Id, _sender.Name)
            );

            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:{sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id}=force", _sender.Id, _sender.Name)
            );

            var expected = new List<UUID>()
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

            var externalWearable = new InventoryTree.InventoryItem()
            {
                Name = "External Tattoo",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                WornOn = WearableType.Tattoo,
                AttachedTo = null,
                Id = new UUID("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa")
            };
            var externalAttachable = new InventoryTree.InventoryItem()
            {
                Name = "External Jaw Thing",
                Folder = null,
                FolderId = new UUID("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                AttachedTo = AttachmentPoint.Jaw,
                WornOn = null,
                Id = new UUID("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa")
            };

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);

            _callbacks.Setup(e =>
                e.TryGetCurrentOutfit(out currentOutfit)
            ).ReturnsAsync(true);

            _callbacks.Setup(e =>
                e.TryGetRlvInventoryTree(out sharedFolder)
            ).ReturnsAsync(true);

            var raised = Assert.Raises<DetachEventArgs>(
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage($"@{command}:{sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id}=force", _sender.Id, _sender.Name)
            );

            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachall:Clothing=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing folder, and all of its subfolders will be removed
            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachthis=force", sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, sampleTree.Root_Clothing_HappyShirt_AttachChest.Name)
            );

            // Everything under the clothing folder will be detached because happyshirt exists in the clothing folder
            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachthis:chest=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing and accessories folder will be detached, not recursive
            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachthis:pants=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing and accessories folder will be detached, not recursive
            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachthis:pants=force", _sender.Id, _sender.Name)
            );

            // Only accessories will be removed even though pants exist in our clothing folder. The clothing folder is private ".clothing"
            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachallthis=force", sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, sampleTree.Root_Clothing_HappyShirt_AttachChest.Name)
            );

            // Everything under the clothing folder (and its subfolders recursively) will be detached because happy shirt exists in the clothing folder
            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachallthis:chest=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing and accessories folder will be detached, and their subfolders recursively
            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachallthis:pants=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing and accessories folder will be detached, recursive
            var expected = new List<UUID>()
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
                 attach: n => _rlv.Actions.Detach += n,
                 detach: n => _rlv.Actions.Detach -= n,
                 testCode: () => _rlv.ProcessMessage("@detachallthis:pants=force", _sender.Id, _sender.Name)
            );

            // Everything under the clothing and accessories folder will be detached, recursive.
            //   Hats will be excluded because they are in a private folder ".hats"
            var expected = new List<UUID>()
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
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - folder was locked because PartyHat (groin)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED) - folder was locked because BusinessPants (groin)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses (LOCKED) - folder was locked from Watch (tattoo)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED) - Folder locked due to RetroPants being worn as 'pants'
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED) - Folder locked due to RetroPants being worn as 'pants'
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED) - Folder locked due to HappyShirt attachment of 'chest'
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED) - Folder locked due to HappyShirt attachment of 'chest'
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()  - Locked, but folder has exception
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt () - Locked, but folder has exception
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants () - Locked, but folder has exception
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat () - Parent folder locked recursively, but has exception
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat () - Parent folder locked recursively, but parent has recursive exception
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()  - Locked, but folder has exception
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt () - Locked, but folder has exception
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants () - Locked, but folder has exception
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - folder was locked because PartyHat (groin)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED) - folder was locked because BusinessPants (groin)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses (LOCKED) - folder was locked from Watch (tattoo)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED) - Folder locked due to RetroPants being worn as 'pants'
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED) - Folder locked due to RetroPants being worn as 'pants'
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED) - Folder locked due to HappyShirt attachment of 'chest'
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED) - Folder locked due to HappyShirt attachment of 'chest'
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()  - Locked, but folder has exception
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt () - Locked, but folder has exception
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants () - Locked, but folder has exception
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat () - Parent folder locked recursively, but has exception
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
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
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat () - Parent folder locked recursively, but parent has recursive exception
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()  - Locked, but folder has exception
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt () - Locked, but folder has exception
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants () - Locked, but folder has exception
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Restrictions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        #endregion

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

            Assert.True(_rlv.Restrictions.CanFarTouch(out var distance));
            Assert.Equal(0.9f, distance);
        }

        [Theory]
        [InlineData("fartouch")]
        [InlineData("touchfar")]
        public void CanFarTouch_Synonym(string command)
        {
            _rlv.ProcessMessage($"@{command}:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanFarTouch(out var distance));
            Assert.Equal(0.9f, distance);
        }

        [Theory]
        [InlineData("fartouch")]
        [InlineData("touchfar")]
        public void CanFarTouch_Default(string command)
        {
            _rlv.ProcessMessage($"@{command}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanFarTouch(out var distance));
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

            Assert.True(_rlv.Restrictions.CanFarTouch(out var actualDistance2));

            _rlv.ProcessMessage($"@{command1}:6.78=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanFarTouch(out var actualDistance1));

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

            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public void TouchAll_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@touchall=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
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

            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public void TouchWorld_Exception()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage("@touchworld=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@touchworld:{objectId2}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));

            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId2, null, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId2, userId1, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId2, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId2, null, null));
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

            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));

            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId2, null, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId2, userId1, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId2, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId2, null, null));
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

            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));

            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, _sender.Id, null, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, _sender.Id, userId1, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, _sender.Id, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, _sender.Id, null, null));
        }

        #endregion

        #region @touchattach=<y/n>

        [Fact]
        public void TouchAttach_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage("@touchattach=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        #endregion

        #region @touchattachself=<y/n>

        [Fact]
        public void TouchAttachSelf_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage("@touchattachself=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        #endregion

        #region @touchattachother=<y/n> @touchattachother:<UUID>=<y/n>

        [Fact]
        public void TouchAttachOther_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage("@touchattachother=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public void TouchAttachOther_Specific()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");
            var userId2 = new UUID("66666666-6666-4666-8666-666666666666");

            _rlv.ProcessMessage($"@touchattachother:{userId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId2, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        #endregion

        #region @touchhud[:<UUID>]=<y/n>

        [Fact]
        public void TouchHud_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage($"@touchhud=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public void TouchHud_specific()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

            _rlv.ProcessMessage($"@touchhud:{objectId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId1, null, null));
            Assert.False(_rlv.Restrictions.CanTouch(RLVManager.TouchLocation.Hud, objectId2, null, null));
        }

        #endregion

        #region @interact=<y/n>

        [Fact]
        public void CanInteract()
        {
            CheckSimpleCommand("interact", m => m.CanInteract());
        }

        [Fact]
        public void CanInteract_default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId1 = new UUID("55555555-5555-4555-8555-555555555555");

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
            Assert.True(_rlv.Restrictions.CanShowNames(null));
            Assert.True(_rlv.Restrictions.CanShowNames(UUID.Random()));
        }

        [Fact]
        public void CanShowNames()
        {
            _rlv.ProcessMessage("@shownames=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowNames(null));
            Assert.False(_rlv.Restrictions.CanShowNames(UUID.Random()));
        }

        [Fact]
        public void CanShowNames_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@shownames=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@shownames:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowNames(null));
            Assert.True(_rlv.Restrictions.CanShowNames(userId1));
            Assert.False(_rlv.Restrictions.CanShowNames(userId2));
        }

        [Fact]
        public void CanShowNames_Secure_Default()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@shownames_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowNames(null));
            Assert.False(_rlv.Restrictions.CanShowNames(userId1));
            Assert.False(_rlv.Restrictions.CanShowNames(userId2));
        }

        [Fact]
        public void CanShowNames_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new UUID("22222222-2222-4222-8222-222222222222"));
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

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
            Assert.True(_rlv.Restrictions.CanShowNameTags(null));
            Assert.True(_rlv.Restrictions.CanShowNameTags(UUID.Random()));
        }

        [Fact]
        public void CanShowNameTags()
        {
            _rlv.ProcessMessage("@shownametags=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowNameTags(null));
            Assert.False(_rlv.Restrictions.CanShowNameTags(UUID.Random()));
        }

        [Fact]
        public void CanShowNameTags_Except()
        {
            var userId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

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
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverTextAll()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var userId2 = new UUID("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@showhovertextall=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.False(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        #endregion

        #region @showhovertext:<UUID>=<y/n>

        [Fact]
        public void CanShowHoverText_Default()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverText()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

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
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverTextHud()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

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
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.World, objectId1));
            Assert.True(_rlv.Restrictions.CanShowHoverText(RLVManager.HoverTextLocation.Hud, objectId1));
        }

        [Fact]
        public void CanShowHoverTextWorld()
        {
            var objectId1 = new UUID("00000000-0000-4000-8000-000000000000");
            var objectId2 = new UUID("11111111-1111-4111-8111-111111111111");

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
            Assert.Equal(UUID.Zero, raised.Arguments.GroupId);
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
            Assert.Equal(UUID.Zero, raised.Arguments.GroupId);
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

        #region @setdebug=<y/n>
        [Fact]
        public void CanSetDebug()
        {
            CheckSimpleCommand("setDebug", m => m.CanSetDebug());
        }
        #endregion

        #region @setdebug_<setting>:<value>=force
        [Theory]
        [InlineData("RenderResolutionDivisor", "RenderResolutionDivisor Success")]
        [InlineData("Unknown Setting", "Unknown Setting Success")]
        public void SetDebug_Default(string settingName, string settingValue)
        {
            var raised = Assert.Raises<SetSettingEventArgs>(
                 attach: n => _rlv.Actions.SetDebug += n,
                 detach: n => _rlv.Actions.SetDebug -= n,
                 testCode: () => _rlv.ProcessMessage($"@setdebug_{settingName}:{settingValue}=force", _sender.Id, _sender.Name)
             );

            Assert.Equal(raised.Arguments.SettingName, settingName.ToLower());
            Assert.Equal(raised.Arguments.SettingValue, settingValue);
        }

        [Fact]
        public void SetDebug_Invalid()
        {
            var eventRaised = false;
            _rlv.Actions.SetDebug += (sender, args) =>
            {
                eventRaised = true;
            };

            Assert.False(_rlv.ProcessMessage($"@setdebug_:42=force", _sender.Id, _sender.Name));
            Assert.False(eventRaised);
        }
        #endregion

        #region @getdebug_<setting>=<channel_number>
        [Theory]
        [InlineData("RenderResolutionDivisor", "RenderResolutionDivisor Success")]
        [InlineData("Unknown Setting", "Unknown Setting Success")]
        public void GetDebug_Default(string settingName, string settingValue)
        {
            var actual = _callbacks.RecordReplies();

            _callbacks.Setup(e =>
                e.GetDebugInfoAsync(settingName.ToLower())
            ).ReturnsAsync(settingValue);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, settingValue),
            };

            Assert.True(_rlv.ProcessMessage($"@getdebug_{settingName}=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }
        #endregion

        #region @setenv=<y/n>
        [Fact]
        public void CanSetEnv()
        {
            CheckSimpleCommand("setEnv", m => m.CanSetEnv());
        }
        #endregion

        #region @setenv_<setting>:<value>=force

        [Theory]
        [InlineData("Daytime", "Daytime Success")]
        [InlineData("Unknown Setting", "Unknown Setting Success")]
        public void SetEnv_Default(string settingName, string settingValue)
        {
            var raised = Assert.Raises<SetSettingEventArgs>(
                 attach: n => _rlv.Actions.SetEnv += n,
                 detach: n => _rlv.Actions.SetEnv -= n,
                 testCode: () => _rlv.ProcessMessage($"@setenv_{settingName}:{settingValue}=force", _sender.Id, _sender.Name)
             );

            Assert.Equal(raised.Arguments.SettingName, settingName.ToLower());
            Assert.Equal(raised.Arguments.SettingValue, settingValue);
        }

        #endregion

        #region @getenv_<setting>=<channel_number>

        [Theory]
        [InlineData("Daytime", "Daytime Success")]
        [InlineData("Unknown Setting", "Unknown Setting Success")]
        public void GetEnv_Default(string settingName, string settingValue)
        {
            var actual = _callbacks.RecordReplies();

            _callbacks.Setup(e =>
                e.GetEnvironmentAsync(settingName.ToLower())
            ).ReturnsAsync(settingValue);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, settingValue),
            };

            Assert.True(_rlv.ProcessMessage($"@getenv_{settingName}=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        #endregion

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
