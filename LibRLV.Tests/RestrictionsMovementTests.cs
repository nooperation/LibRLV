using LibRLV.EventArguments;
using Moq;

namespace LibRLV.Tests
{
    public class RestrictionsMovementTests : RestrictionsBase
    {
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


        #region @unsit
        [Fact]
        public void CanUnsit()
        {
            CheckSimpleCommand("unsit", m => m.CanUnsit());
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
        public void ForceSit_Default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
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
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
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
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
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
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
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
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
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
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
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
            SetCurrentSitId(Guid.Empty);

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
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
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
    }
}
