using LibRLV.EventArguments;
using Moq;

namespace LibRLV.Tests
{
    public class RestrictionsCameraTests : RestrictionsBase
    {

        #region CamMinFunctionsThrough

        [Fact]
        public void CamZoomMin_Default()
        {
            Assert.False(_rlv.Permissions.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(default, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Single()
        {
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(1.5f, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Multiple_SingleSender()
        {
            _rlv.ProcessMessage("@CamZoomMin:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:4.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamZoomMin(out var camZoomMin));
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

            Assert.True(_rlv.Permissions.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(4.5f, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Multiple_MultipleSenders()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var sender3 = new RlvObject("Sender 3", new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"));

            _rlv.ProcessMessage("@CamZoomMin:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:4.5=n", sender2.Id, sender2.Name);
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", sender3.Id, sender3.Name);

            Assert.True(_rlv.Permissions.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(4.5f, camZoomMin);
        }

        [Fact]
        public void CamZoomMin_Off()
        {
            _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMin:1.5=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(default, camZoomMin);
        }
        #endregion

        #region CamMaxFunctionsThrough
        [Fact]
        public void CamZoomMax_Default()
        {
            Assert.False(_rlv.Permissions.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(default, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Single()
        {
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Multiple_SingleSender()
        {
            _rlv.ProcessMessage("@CamZoomMax:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:4.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamZoomMax(out var camZoomMax));
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

            Assert.True(_rlv.Permissions.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Multiple_MultipleSenders()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var sender3 = new RlvObject("Sender 3", new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"));

            _rlv.ProcessMessage("@CamZoomMax:3.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:4.5=n", sender2.Id, sender2.Name);
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", sender3.Id, sender3.Name);

            Assert.True(_rlv.Permissions.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }

        [Fact]
        public void CamZoomMax_Off()
        {
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamZoomMax:1.5=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(default, camZoomMax);
        }

        #endregion

        #region @CamZoomMin
        [Fact]
        public void CamZoomMin()
        {
            _rlv.ProcessMessage("@CamZoomMin:0.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamZoomMin(out var camZoomMin));
            Assert.Equal(0.5f, camZoomMin);
        }
        #endregion

        #region @CamZoomMax
        [Fact]
        public void CamZoomMax()
        {
            _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamZoomMax(out var camZoomMax));
            Assert.Equal(1.5f, camZoomMax);
        }
        #endregion

        #region @setcam_fovmin
        [Fact]
        public void SetCamFovMin()
        {
            _rlv.ProcessMessage("@setcam_fovmin:15=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasSetCamFovMin(out var setCamFovMin));
            Assert.Equal(15f, setCamFovMin);
        }
        #endregion

        #region @setcam_fovmax
        [Fact]
        public void SetCamFovMax()
        {
            _rlv.ProcessMessage("@setcam_fovmax:45=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasSetCamFovMax(out var setCamFovMax));
            Assert.Equal(45f, setCamFovMax);
        }
        #endregion

        #region @setcam_fov:<fov_in_radians>=force
        [Fact]
        public void SetCamFov()
        {
            var raised = Assert.Raises<SetCamFOVEventArgs>(
                 attach: n => _rlv.Commands.SetCamFOV += n,
                 detach: n => _rlv.Commands.SetCamFOV -= n,
                 testCode: () => _rlv.ProcessMessage("@setcam_fov:1.75=force", _sender.Id, _sender.Name)
             );

            Assert.Equal(1.75f, raised.Arguments.FOVInRadians, FloatTolerance);
        }

        [Fact]
        public void SetCamFov_Restricted()
        {
            var raisedEvent = false;
            _rlv.Commands.SetCamFOV += (sender, args) =>
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
            _rlv.Commands.SetCamFOV += (sender, args) =>
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

            Assert.True(_rlv.Permissions.HasSetCamAvDistMax(out var setCamAvDistMax));
            Assert.Equal(30f, setCamAvDistMax);
        }
        [Fact]
        public void SetCamAvDistMax_Synonym()
        {
            _rlv.ProcessMessage("@camdistmax:30=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasSetCamAvDistMax(out var setCamAvDistMax));
            Assert.Equal(30f, setCamAvDistMax);
        }
        #endregion

        #region @setcam_avdistmin
        [Fact]
        public void SetCamAvDistMin()
        {
            _rlv.ProcessMessage("@setcam_avdistmin:0.3=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasSetCamAvDistMin(out var setCamAvDistMin));
            Assert.Equal(0.3f, setCamAvDistMin);
        }

        [Fact]
        public void SetCamAvDistMin_Synonym()
        {
            _rlv.ProcessMessage("@camdistmin:0.3=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasSetCamAvDistMin(out var setCamAvDistMin));
            Assert.Equal(0.3f, setCamAvDistMin);
        }
        #endregion

        #region @CamDrawAlphaMax
        [Fact]
        public void CamDrawAlphaMax()
        {
            _rlv.ProcessMessage("@CamDrawAlphaMax:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamDrawAlphaMax(out var camDrawAlphaMax));
            Assert.Equal(0.9f, camDrawAlphaMax);
        }
        #endregion

        #region @camdrawmin:<min_distance>=<y/n>

        [Fact]
        public void CamDrawMin()
        {
            _rlv.ProcessMessage("@camdrawmin:1.75=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamDrawMin(out var camDrawMin));
            Assert.Equal(1.75f, camDrawMin);
        }

        [Fact]
        public void CamDrawMin_Small()
        {
            Assert.False(_rlv.ProcessMessage("@camdrawmin:0.15=n", _sender.Id, _sender.Name));
            Assert.False(_rlv.Permissions.HasCamDrawMin(out var camDrawMin));
        }

        #endregion

        #region @camdrawmax:<max_distance>=<y/n>

        [Fact]
        public void CamDrawMax()
        {
            _rlv.ProcessMessage("@camdrawmax:1.75=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamDrawMax(out var camDrawMax));
            Assert.Equal(1.75f, camDrawMax);
        }

        [Fact]
        public void CamDrawMax_Small()
        {
            Assert.False(_rlv.ProcessMessage("@camdrawmax:0.15=n", _sender.Id, _sender.Name));
            Assert.False(_rlv.Permissions.HasCamDrawMax(out var camDrawMax));
        }

        #endregion

        #region @camdrawalphamin:<min_distance>=<y/n>

        [Fact]
        public void CamDrawAlphaMin()
        {
            _rlv.ProcessMessage("@camdrawalphamin:1.75=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamDrawAlphaMin(out var camDrawAlphaMin));
            Assert.Equal(1.75f, camDrawAlphaMin);
        }

        #endregion

        #region @CamDrawColor

        [Fact]
        public void CamDrawColor()
        {
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamDrawColor(out var color));

            Assert.Equal(0.1f, color.X, FloatTolerance);
            Assert.Equal(0.2f, color.Y, FloatTolerance);
            Assert.Equal(0.3f, color.Z, FloatTolerance);
        }

        [Fact]
        public void CamDrawColor_Default()
        {
            Assert.False(_rlv.Permissions.HasCamDrawColor(out var color));
        }

        [Fact]
        public void CamDrawColor_Large()
        {
            _rlv.ProcessMessage("@CamDrawColor:5;6;7=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamDrawColor(out var color));
            Assert.Equal(1.0f, color.X, FloatTolerance);
            Assert.Equal(1.0f, color.Y, FloatTolerance);
            Assert.Equal(1.0f, color.Z, FloatTolerance);
        }

        [Fact]
        public void CamDrawColor_Negative()
        {
            _rlv.ProcessMessage("@CamDrawColor:-5;-6;-7=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamDrawColor(out var color));
            Assert.Equal(0.0f, color.X, FloatTolerance);
            Assert.Equal(0.0f, color.Y, FloatTolerance);
            Assert.Equal(0.0f, color.Z, FloatTolerance);
        }

        [Fact]
        public void CamDrawColor_Removal()
        {
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=y", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.HasCamDrawColor(out var color));
        }

        [Fact]
        public void CamDrawColor_Multi()
        {
            _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@CamDrawColor:0.2;0.3;0.6=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasCamDrawColor(out var color));
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

            Assert.True(_rlv.Permissions.HasCamAvDist(out var camAvDist));
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

            Assert.True(_rlv.Permissions.HasSetCamtextures(out var actualTextureId));

            Assert.Equal(Guid.Empty, actualTextureId);
        }

        [Theory]
        [InlineData("setcam_textures")]
        [InlineData("camtextures")]
        public void SetCamTextures_Single(string command)
        {
            var textureId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage($"@{command}:{textureId1}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasSetCamtextures(out var actualTextureId));

            Assert.Equal(textureId1, actualTextureId);
        }

        [Theory]
        [InlineData("setcam_textures", "setcam_textures")]
        [InlineData("setcam_textures", "camtextures")]
        [InlineData("camtextures", "camtextures")]
        [InlineData("camtextures", "setcam_textures")]
        public void SetCamTextures_Multiple_Synonyms(string command1, string command2)
        {
            var textureId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var textureId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@{command1}:{textureId1}=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@{command2}:{textureId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasSetCamtextures(out var actualTextureId2));

            _rlv.ProcessMessage($"@{command1}:{textureId2}=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.HasSetCamtextures(out var actualTextureId1));

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
    }
}
