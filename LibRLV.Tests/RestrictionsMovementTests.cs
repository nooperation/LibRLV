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
            _actionCallbacks
                .Setup(e => e.SetRotAsync(It.IsAny<float>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _rlv.ProcessMessage("@setrot:1.5=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.SetRotAsync(1.5f, It.IsAny<CancellationToken>()),
                Times.Once);

            _actionCallbacks.VerifyNoOtherCalls();
        }
        #endregion

        #region @adjustheight:<distance_pelvis_to_foot_in_meters>;<factor>[;delta_in_meters]=force
        [Fact]
        public async Task AdjustHeight()
        {
            _actionCallbacks
                .Setup(e => e.AdjustHeightAsync(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _rlv.ProcessMessage("@adjustheight:4.3;1.25=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.AdjustHeightAsync(4.3f, 1.25f, 0.0f, It.IsAny<CancellationToken>()),
                Times.Once);

            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AdjustHeight_WithDelta()
        {
            _actionCallbacks
                .Setup(e => e.AdjustHeightAsync(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _rlv.ProcessMessage("@adjustheight:4.3;1.25;12.34=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.AdjustHeightAsync(4.3f, 1.25f, 12.34f, It.IsAny<CancellationToken>()),
                Times.Once);

            _actionCallbacks.VerifyNoOtherCalls();
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
        private void SetObjectExists(Guid objectId)
        {
            _queryCallbacks.Setup(e =>
                e.ObjectExistsAsync(objectId, default)
            ).ReturnsAsync(true);
        }

        private void SetIsSitting(bool isCurrentlySitting)
        {
            _queryCallbacks.Setup(e =>
                e.IsSittingAsync(default)
            ).ReturnsAsync(isCurrentlySitting);
        }

        private void SetCurrentSitId(Guid objectId)
        {
            _queryCallbacks.Setup(e =>
                e.TryGetSitIdAsync(default)
            ).ReturnsAsync((objectId != Guid.Empty, objectId));
        }

        [Fact]
        public async Task ForceSit_Default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            SetObjectExists(objectId1);
            SetIsSitting(false);

            _actionCallbacks
                .Setup(e => e.SitAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.SitAsync(objectId1, It.IsAny<CancellationToken>()),
                Times.Once);

            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ForceSit_RestrictedUnsit_WhileStanding()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            SetObjectExists(objectId1);
            SetIsSitting(false);

            await _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);

            _actionCallbacks
                .Setup(e => e.SitAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.SitAsync(objectId1, It.IsAny<CancellationToken>()),
                Times.Once);

            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ForceSit_RestrictedUnsit_WhileSeated()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            SetObjectExists(objectId1);
            SetIsSitting(true);

            await _rlv.ProcessMessage("@unsit=n", _sender.Id, _sender.Name);

            _actionCallbacks
                .Setup(e => e.SitAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Assert.False(await _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));

            // Assert
            _actionCallbacks.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task ForceSit_RestrictedSit()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            SetObjectExists(objectId1);
            SetIsSitting(true);

            await _rlv.ProcessMessage("@sit=n", _sender.Id, _sender.Name);

            _actionCallbacks
                .Setup(e => e.SitAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Assert.False(await _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));

            // Assert
            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ForceSit_RestrictedStandTp()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            SetObjectExists(objectId1);
            SetIsSitting(true);

            await _rlv.ProcessMessage("@standtp=n", _sender.Id, _sender.Name);

            _actionCallbacks
                .Setup(e => e.SitAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Assert.False(await _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));

            // Assert
            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ForceSit_InvalidObject()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            // SetupSitTarget(objectId1, true); <-- Don't setup sit target for this test

            _actionCallbacks
                .Setup(e => e.SitAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Assert.False(await _rlv.ProcessMessage($"@sit:{objectId1}=force", _sender.Id, _sender.Name));

            // Assert
            _actionCallbacks.VerifyNoOtherCalls();
        }
        #endregion

        #region @getsitid=<channel_number>

        [Fact]
        public async Task GetSitID()
        {
            var actual = _actionCallbacks.RecordReplies();
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
            var actual = _actionCallbacks.RecordReplies();
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
