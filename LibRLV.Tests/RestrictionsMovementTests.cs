using LibRLV.EventArguments;
using Moq;

namespace LibRLV.Tests
{
    public class RestrictionsMovementTests : RestrictionsBase
    {
        #region @fly
        [Fact]
        public async Task CanFly()
        {
            await CheckSimpleCommand("fly", m => m.CanFly());
        }
        #endregion

        #region @jump (RLVa)
        [Fact]
        public async Task CanJump()
        {
            await CheckSimpleCommand("jump", m => m.CanJump());
        }
        #endregion

        #region @temprun
        [Fact]
        public async Task CanTempRun()
        {
            await CheckSimpleCommand("tempRun", m => m.CanTempRun());
        }
        #endregion

        #region @alwaysrun
        [Fact]
        public async Task CanAlwaysRun()
        {
            await CheckSimpleCommand("alwaysRun", m => m.CanAlwaysRun());
        }
        #endregion

        #region @setrot:<angle_in_radians>=force
        [Fact]
        public async Task SetRot()
        {
            var raised = await Assert.RaisesAsync<SetRotEventArgs>(
                 attach: n => _rlv.Commands.SetRot += n,
                 detach: n => _rlv.Commands.SetRot -= n,
                 testCode: () => _rlv.ProcessMessage("@setrot:1.5=force", _sender.Id, _sender.Name)
             );

            Assert.Equal(1.5f, raised.Arguments.AngleInRadians, FloatTolerance);
        }
        #endregion

        #region @adjustheight:<distance_pelvis_to_foot_in_meters>;<factor>[;delta_in_meters]=force
        [Fact]
        public async Task AdjustHeight()
        {
            var raised = await Assert.RaisesAsync<AdjustHeightEventArgs>(
                 attach: n => _rlv.Commands.AdjustHeight += n,
                 detach: n => _rlv.Commands.AdjustHeight -= n,
                 testCode: () => _rlv.ProcessMessage("@adjustheight:4.3;1.25=force", _sender.Id, _sender.Name)
             );

            Assert.Equal(4.3f, raised.Arguments.Distance, FloatTolerance);
            Assert.Equal(1.25f, raised.Arguments.Factor, FloatTolerance);
            Assert.Equal(0.0f, raised.Arguments.Delta, FloatTolerance);
        }

        [Fact]
        public async Task AdjustHeight_WithDelta()
        {
            var raised = await Assert.RaisesAsync<AdjustHeightEventArgs>(
                 attach: n => _rlv.Commands.AdjustHeight += n,
                 detach: n => _rlv.Commands.AdjustHeight -= n,
                 testCode: () => _rlv.ProcessMessage("@adjustheight:4.3;1.25;12.34=force", _sender.Id, _sender.Name)
             );


            Assert.Equal(4.3f, raised.Arguments.Distance, FloatTolerance);
            Assert.Equal(1.25f, raised.Arguments.Factor, FloatTolerance);
            Assert.Equal(12.34f, raised.Arguments.Delta, FloatTolerance);
        }
        #endregion


        #region @unsit
        [Fact]
        public async Task CanUnsit()
        {
            await CheckSimpleCommand("unsit", m => m.CanUnsit());
        }
        #endregion

        #region @sit:<uuid>=force
        private void SetObjectExists(Guid objectId, bool isCurrentlySitting)
        {
            _callbacks.Setup(e =>
                e.TryGetObjectExists(objectId, out isCurrentlySitting)
            ).ReturnsAsync(true);
        }

        private void SetCurrentSitId(Guid objectId)
        {
            _callbacks.Setup(e =>
                e.TryGetSitId(out objectId)
            ).ReturnsAsync(objectId != Guid.Empty);
        }

        [Fact]
        public async Task ForceSit_Default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            SetObjectExists(objectId1, false);

            var raised = await Assert.RaisesAsync<SitEventArgs>(
                attach: n => _rlv.Commands.Sit += n,
                detach: n => _rlv.Commands.Sit -= n,
                testCode: () => _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name)
            );

            Assert.Equal(objectId1, raised.Arguments.Target);
        }

        [Fact]
        public async Task ForceSit_RestrictedUnsit_WhileStanding()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            SetObjectExists(objectId1, false);

            await _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);

            var raised = await Assert.RaisesAsync<SitEventArgs>(
                attach: n => _rlv.Commands.Sit += n,
                detach: n => _rlv.Commands.Sit -= n,
                testCode: () => _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name)
            );

            Assert.Equal(objectId1, raised.Arguments.Target);
        }

        [Fact]
        public async Task ForceSit_RestrictedUnsit_WhileSeated()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            SetObjectExists(objectId1, true);

            await _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);

            var raisedEvent = false;
            _rlv.Commands.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(await _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }


        [Fact]
        public async Task ForceSit_RestrictedSit()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            SetObjectExists(objectId1, true);

            await _rlv.ProcessMessage("@sit=n", _sender.Id, _sender.Name);

            var raisedEvent = false;
            _rlv.Commands.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(await _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }

        [Fact]
        public async Task ForceSit_RestrictedStandTp()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            SetObjectExists(objectId1, true);

            await _rlv.ProcessMessage("@standtp=n", _sender.Id, _sender.Name);

            var raisedEvent = false;
            _rlv.Commands.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(await _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }

        [Fact]
        public async Task ForceSit_InvalidObject()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            // SetupSitTarget(objectId1, true); <-- Don't setup sit target for this test

            var raisedEvent = false;
            _rlv.Commands.TpTo += (sender, args) =>
            {
                raisedEvent = true;
            };

            Assert.False(await _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }
        #endregion

        #region @getsitid=<channel_number>

        [Fact]
        public async Task GetSitID()
        {
            var actual = _callbacks.RecordReplies();
            SetCurrentSitId(Guid.Empty);

            await _rlv.ProcessMessage("@getsitid=1234", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "NULL_KEY"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetSitID_Default()
        {
            var actual = _callbacks.RecordReplies();
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            SetCurrentSitId(objectId1);

            await _rlv.ProcessMessage("@getsitid=1234", _sender.Id, _sender.Name);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, objectId1.ToString()),
            };

            Assert.Equal(expected, actual);
        }

        #endregion

        #region @unsit=force

        [Fact]
        public async Task ForceUnSit()
        {
            Assert.True(await _rlv.ProcessMessage("@unsit=force", _sender.Id, _sender.Name));
        }

        [Fact]
        public async Task ForceUnSit_RestrictedUnsit()
        {
            await _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);

            Assert.False(await _rlv.ProcessMessage("@unsit=force", _sender.Id, _sender.Name));
        }

        #endregion

        #region @sit
        [Fact]
        public async Task CanSit()
        {
            await CheckSimpleCommand("sit", m => m.CanSit());
        }
        #endregion

        #region @sitground=force

        [Fact]
        public async Task ForceSitGround()
        {
            // TODO: Check reaction
            Assert.True(await _rlv.ProcessMessage("@sitground=force", _sender.Id, _sender.Name));
        }

        [Fact]
        public async Task ForceSitGround_RestrictedSit()
        {
            await _rlv.ProcessMessage("@sit=n", _sender.Id, _sender.Name);

            Assert.False(await _rlv.ProcessMessage("@sitground=force", _sender.Id, _sender.Name));
        }

        #endregion
    }
}
