using Moq;
using OpenMetaverse;

namespace LibRLV.Tests
{
    public class UnitTest1
    {
        public class RlvObject
        {
            public RlvObject(string name)
            {
                Id = new UUID(Guid.NewGuid());
                Name = name;
            }

            public UUID Id { get; set; }
            public string Name { get; set; }
        }

        private readonly RlvObject _sender;
        private readonly Mock<IRLVCallbacks> _callbacks;
        private readonly RLV _rlv;

        public UnitTest1()
        {
            _sender = new RlvObject("Sender 1");
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
        #endregion
    }
}
