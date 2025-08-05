using LibRLV.EventArguments;

namespace LibRLV.Tests
{
    public class RestrictionsTeleportTests : RestrictionsBase
    {

        #region @TpLocal
        [Fact]
        public async Task CanTpLocal_Default()
        {
            await _rlv.ProcessMessage("@TpLocal=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanTpLocal(out var distance));
            Assert.Equal(0.0f, distance, FloatTolerance);
        }

        [Fact]
        public async Task CanTpLocal()
        {
            await _rlv.ProcessMessage("@TpLocal:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanTpLocal(out var distance));
            Assert.Equal(0.9f, distance, FloatTolerance);
        }
        #endregion

        #region @tplm
        [Fact]
        public async Task CanTpLm()
        {
            await CheckSimpleCommand("tpLm", m => m.CanTpLm());
        }
        #endregion

        #region @tploc
        [Fact]
        public async Task CanTpLoc()
        {
            await CheckSimpleCommand("tpLoc", m => m.CanTpLoc());
        }
        #endregion

        #region @tplure @tplure_sec 

        [Fact]
        public void CanTpLure_Default()
        {
            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            Assert.True(_rlv.Permissions.CanTPLure(null));
            Assert.True(_rlv.Permissions.CanTPLure(userId1));
        }

        [Fact]
        public async Task CanTpLure()
        {
            await _rlv.ProcessMessage("@tplure=n", _sender.Id, _sender.Name);

            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            Assert.False(_rlv.Permissions.CanTPLure(null));
            Assert.False(_rlv.Permissions.CanTPLure(userId1));
        }

        [Fact]
        public async Task CanTpLure_Except()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@tplure=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@tplure:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanTPLure(null));
            Assert.True(_rlv.Permissions.CanTPLure(userId1));
            Assert.False(_rlv.Permissions.CanTPLure(userId2));
        }

        [Fact]
        public async Task CanTpLure_Secure_Default()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@tplure_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanTPLure(null));
            Assert.False(_rlv.Permissions.CanTPLure(userId1));
            Assert.False(_rlv.Permissions.CanTPLure(userId2));
        }

        [Fact]
        public async Task CanTpLure_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@tplure_sec=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@tplure:{userId1}=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@tplure:{userId2}=add", sender2.Id, sender2.Name);

            Assert.False(_rlv.Permissions.CanTPLure(null));
            Assert.True(_rlv.Permissions.CanTPLure(userId1));
            Assert.False(_rlv.Permissions.CanTPLure(userId2));
        }

        #endregion

        #region @sittp

        [Fact]
        public void CanSitTp_Default()
        {
            Assert.False(_rlv.Permissions.CanSitTp(out var maxDistance));
            Assert.Equal(1.5f, maxDistance);
        }

        [Fact]
        public async Task CanSitTp_Single()
        {
            await _rlv.ProcessMessage("@SitTp:2.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanSitTp(out var maxDistance));
            Assert.Equal(2.5f, maxDistance);
        }

        [Fact]
        public async Task CanSitTp_Multiple_SingleSender()
        {
            await _rlv.ProcessMessage("@SitTp:3.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@SitTp:4.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@SitTp:2.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanSitTp(out var maxDistance));
            Assert.Equal(2.5f, maxDistance);
        }

        [Fact]
        public async Task CanSitTp_Multiple_SingleSender_WithRemoval()
        {
            await _rlv.ProcessMessage("@SitTp:3.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@SitTp:4.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@SitTp:2.5=n", _sender.Id, _sender.Name);

            await _rlv.ProcessMessage("@SitTp:8.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@SitTp:8.5=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanSitTp(out var maxDistance));
            Assert.Equal(2.5f, maxDistance);
        }

        [Fact]
        public async Task CanSitTp_Multiple_MultipleSenders()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var sender3 = new RlvObject("Sender 3", new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"));

            await _rlv.ProcessMessage("@SitTp:3.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@SitTp:4.5=n", sender2.Id, sender2.Name);
            await _rlv.ProcessMessage("@SitTp:2.5=n", sender3.Id, sender3.Name);

            Assert.True(_rlv.Permissions.CanSitTp(out var maxDistance));
            Assert.Equal(2.5f, maxDistance);
        }

        [Fact]
        public async Task CanSitTp_Off()
        {
            await _rlv.ProcessMessage("@SitTp:2.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@SitTp:2.5=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanSitTp(out var maxDistance));
            Assert.Equal(1.5f, maxDistance);
        }
        #endregion

        #region @standtp
        [Fact]
        public async Task CanStandTp()
        {
            await CheckSimpleCommand("standTp", m => m.CanStandTp());
        }
        #endregion

        #region @tpto:<region_name>/<X_local>/<Y_local>/<Z_local>[;lookat]=force

        [Fact]
        public async Task TpTo_Default()
        {
            var raised = await Assert.RaisesAsync<TpToEventArgs>(
                attach: n => _rlv.Commands.TpTo += n,
                detach: n => _rlv.Commands.TpTo -= n,
                testCode: () => _rlv.ProcessMessage("@tpto:1.5/2.5/3.5=force", _sender.Id, _sender.Name)
            );

            Assert.Equal(1.5f, raised.Arguments.X, FloatTolerance);
            Assert.Equal(2.5f, raised.Arguments.Y, FloatTolerance);
            Assert.Equal(3.5f, raised.Arguments.Z, FloatTolerance);
            Assert.Null(raised.Arguments.RegionName);
            Assert.Null(raised.Arguments.Lookat);
        }

        [Fact]
        public async Task TpTo_WithRegion()
        {
            var raised = await Assert.RaisesAsync<TpToEventArgs>(
                attach: n => _rlv.Commands.TpTo += n,
                detach: n => _rlv.Commands.TpTo -= n,
                testCode: () => _rlv.ProcessMessage("@tpto:Region Name/1.5/2.5/3.5=force", _sender.Id, _sender.Name)
            );

            Assert.Equal(1.5f, raised.Arguments.X, FloatTolerance);
            Assert.Equal(2.5f, raised.Arguments.Y, FloatTolerance);
            Assert.Equal(3.5f, raised.Arguments.Z, FloatTolerance);
            Assert.Equal("Region Name", raised.Arguments.RegionName);
            Assert.Null(raised.Arguments.Lookat);
        }

        [Fact]
        public async Task TpTo_WithRegionAndLookAt()
        {
            var raised = await Assert.RaisesAsync<TpToEventArgs>(
                attach: n => _rlv.Commands.TpTo += n,
                detach: n => _rlv.Commands.TpTo -= n,
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
        public async Task TpTo_RestrictedUnsit()
        {
            await _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);

            var raisedEvent = false;
            _rlv.Commands.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(await _rlv.ProcessMessage("@tpto:1.5/2.5/3.5=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }

        [Fact]
        public async Task TpTo_RestrictedTpLoc()
        {
            await _rlv.ProcessMessage("@tploc=n", _sender.Id, _sender.Name);

            var raisedEvent = false;
            _rlv.Commands.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(await _rlv.ProcessMessage("@tpto:1.5/2.5/3.5=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }

        #endregion

        #region @accepttp

        [Fact]
        public void CanAutoAcceptTp_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            Assert.False(_rlv.Permissions.IsAutoAcceptTp(userId1));
            Assert.False(_rlv.Permissions.IsAutoAcceptTp(userId2));
            Assert.False(_rlv.Permissions.IsAutoAcceptTp());
        }

        [Fact]
        public async Task CanAutoAcceptTp_User()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@accepttp:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.IsAutoAcceptTp(userId1));
            Assert.False(_rlv.Permissions.IsAutoAcceptTp(userId2));
            Assert.False(_rlv.Permissions.IsAutoAcceptTp());
        }

        [Fact]
        public async Task CanAutoAcceptTp_All()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@accepttp=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.IsAutoAcceptTp(userId1));
            Assert.True(_rlv.Permissions.IsAutoAcceptTp(userId2));
            Assert.True(_rlv.Permissions.IsAutoAcceptTp());
        }

        #endregion

        #region @accepttprequest

        [Fact]
        public void CanAutoAcceptTpRequest_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            Assert.False(_rlv.Permissions.IsAutoAcceptTpRequest(userId1));
            Assert.False(_rlv.Permissions.IsAutoAcceptTpRequest(userId2));
            Assert.False(_rlv.Permissions.IsAutoAcceptTpRequest());
        }

        [Fact]
        public async Task CanAutoAcceptTpRequest_User()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@accepttprequest:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.IsAutoAcceptTpRequest(userId1));
            Assert.False(_rlv.Permissions.IsAutoAcceptTpRequest(userId2));
            Assert.False(_rlv.Permissions.IsAutoAcceptTpRequest());
        }

        [Fact]
        public async Task CanAutoAcceptTpRequest_All()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@accepttprequest=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.IsAutoAcceptTpRequest(userId1));
            Assert.True(_rlv.Permissions.IsAutoAcceptTpRequest(userId2));
            Assert.True(_rlv.Permissions.IsAutoAcceptTpRequest());
        }

        #endregion

        #region @tprequest @tprequest_sec

        [Fact]
        public void CanTpRequest_Default()
        {
            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            Assert.True(_rlv.Permissions.CanTpRequest(null));
            Assert.True(_rlv.Permissions.CanTpRequest(userId1));
        }

        [Fact]
        public async Task CanTpRequest()
        {
            await _rlv.ProcessMessage("@tprequest=n", _sender.Id, _sender.Name);

            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            Assert.False(_rlv.Permissions.CanTpRequest(null));
            Assert.False(_rlv.Permissions.CanTpRequest(userId1));
        }

        [Fact]
        public async Task CanTpRequest_Except()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@tprequest=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@tprequest:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanTpRequest(null));
            Assert.True(_rlv.Permissions.CanTpRequest(userId1));
            Assert.False(_rlv.Permissions.CanTpRequest(userId2));
        }

        [Fact]
        public async Task CanTpRequest_Secure_Default()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@tprequest_sec=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanTpRequest(null));
            Assert.False(_rlv.Permissions.CanTpRequest(userId1));
            Assert.False(_rlv.Permissions.CanTpRequest(userId2));
        }

        [Fact]
        public async Task CanTpRequest_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@tprequest_sec=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@tprequest:{userId1}=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@tprequest:{userId2}=add", sender2.Id, sender2.Name);

            Assert.False(_rlv.Permissions.CanTpRequest(null));
            Assert.True(_rlv.Permissions.CanTpRequest(userId1));
            Assert.False(_rlv.Permissions.CanTpRequest(userId2));
        }

        #endregion
    }
}
