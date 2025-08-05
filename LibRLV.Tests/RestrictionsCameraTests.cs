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
            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();

            Assert.Null(cameraRestrictions.ZoomMin);
        }

        [Fact]
        public async Task CamZoomMin_Single()
        {
            await _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.ZoomMin);
            Assert.Equal(1.5f, cameraRestrictions.ZoomMin.Value, FloatTolerance);
        }

        [Fact]
        public async Task CamZoomMin_Multiple_SingleSender()
        {
            await _rlv.ProcessMessage("@CamZoomMin:3.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMin:4.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.ZoomMin);
            Assert.Equal(4.5f, cameraRestrictions.ZoomMin.Value, FloatTolerance);
        }

        [Fact]
        public async Task CamZoomMin_Multiple_SingleSender_WithRemoval()
        {
            await _rlv.ProcessMessage("@CamZoomMin:3.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMin:4.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);

            await _rlv.ProcessMessage("@CamZoomMin:8.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMin:8.5=y", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.ZoomMin);
            Assert.Equal(4.5f, cameraRestrictions.ZoomMin.Value, FloatTolerance);
        }

        [Fact]
        public async Task CamZoomMin_Multiple_MultipleSenders()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var sender3 = new RlvObject("Sender 3", new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"));

            await _rlv.ProcessMessage("@CamZoomMin:3.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMin:4.5=n", sender2.Id, sender2.Name);
            await _rlv.ProcessMessage("@CamZoomMin:1.5=n", sender3.Id, sender3.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.ZoomMin);
            Assert.Equal(4.5f, cameraRestrictions.ZoomMin.Value, FloatTolerance);
        }

        [Fact]
        public async Task CamZoomMin_Off()
        {
            await _rlv.ProcessMessage("@CamZoomMin:1.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMin:1.5=y", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.Null(cameraRestrictions.ZoomMin);
        }
        #endregion

        #region CamMaxFunctionsThrough
        [Fact]
        public void CamZoomMax_Default()
        {
            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.Null(cameraRestrictions.ZoomMax);
        }

        [Fact]
        public async Task CamZoomMax_Single()
        {
            await _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.ZoomMax);
            Assert.Equal(1.5f, cameraRestrictions.ZoomMax);
        }

        [Fact]
        public async Task CamZoomMax_Multiple_SingleSender()
        {
            await _rlv.ProcessMessage("@CamZoomMax:3.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMax:4.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.ZoomMax);
            Assert.Equal(1.5f, cameraRestrictions.ZoomMax);
        }

        [Fact]
        public async Task CamZoomMax_Multiple_SingleSender_WithRemoval()
        {
            await _rlv.ProcessMessage("@CamZoomMax:3.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMax:4.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            await _rlv.ProcessMessage("@CamZoomMax:0.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMax:0.5=y", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.ZoomMax);
            Assert.Equal(1.5f, cameraRestrictions.ZoomMax);
        }

        [Fact]
        public async Task CamZoomMax_Multiple_MultipleSenders()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));
            var sender3 = new RlvObject("Sender 3", new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"));

            await _rlv.ProcessMessage("@CamZoomMax:3.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMax:4.5=n", sender2.Id, sender2.Name);
            await _rlv.ProcessMessage("@CamZoomMax:1.5=n", sender3.Id, sender3.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.ZoomMax);
            Assert.Equal(1.5f, cameraRestrictions.ZoomMax);
        }

        [Fact]
        public async Task CamZoomMax_Off()
        {
            await _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamZoomMax:1.5=y", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.Null(cameraRestrictions.ZoomMax);
        }

        #endregion

        #region @CamZoomMin
        [Fact]
        public async Task CamZoomMin()
        {
            await _rlv.ProcessMessage("@CamZoomMin:0.5=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.ZoomMin);
            Assert.Equal(0.5f, cameraRestrictions.ZoomMin.Value, FloatTolerance);
        }
        #endregion

        #region @CamZoomMax
        [Fact]
        public async Task CamZoomMax()
        {
            await _rlv.ProcessMessage("@CamZoomMax:1.5=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.ZoomMax);
            Assert.Equal(1.5f, cameraRestrictions.ZoomMax.Value, FloatTolerance);
        }
        #endregion

        #region @setcam_fovmin
        [Fact]
        public async Task SetCamFovMin()
        {
            await _rlv.ProcessMessage("@setcam_fovmin:15=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.FovMin);
            Assert.Equal(15f, cameraRestrictions.FovMin.Value, FloatTolerance);
        }
        #endregion

        #region @setcam_fovmax
        [Fact]
        public async Task SetCamFovMax()
        {
            await _rlv.ProcessMessage("@setcam_fovmax:45=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.FovMax);
            Assert.Equal(45f, cameraRestrictions.FovMax.Value, FloatTolerance);
        }
        #endregion

        #region @setcam_fov:<fov_in_radians>=force
        [Fact]
        public async Task SetCamFov()
        {
            var raised = await Assert.RaisesAsync<SetCamFOVEventArgs>(
                 attach: n => _rlv.Commands.SetCamFOV += n,
                 detach: n => _rlv.Commands.SetCamFOV -= n,
                 testCode: () => _rlv.ProcessMessage("@setcam_fov:1.75=force", _sender.Id, _sender.Name)
             );

            Assert.Equal(1.75f, raised.Arguments.FOVInRadians, FloatTolerance);
        }

        [Fact]
        public async Task SetCamFov_Restricted()
        {
            var raisedEvent = false;
            _rlv.Commands.SetCamFOV += (sender, args) =>
            {
                raisedEvent = true;
            };

            await _rlv.ProcessMessage("@setcam_unlock=n", _sender.Id, _sender.Name);

            Assert.False(await _rlv.ProcessMessage("@setcam_fov:1.75=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }

        [Fact]
        public async Task SetCamFov_Restricted_Synonym()
        {
            var raisedEvent = false;
            _rlv.Commands.SetCamFOV += (sender, args) =>
            {
                raisedEvent = true;
            };

            await _rlv.ProcessMessage("@camunlock=n", _sender.Id, _sender.Name);

            Assert.False(await _rlv.ProcessMessage("@setcam_fov:1.75=force", _sender.Id, _sender.Name));
            Assert.False(raisedEvent);
        }
        #endregion

        #region @setcam_avdistmax
        [Fact]
        public async Task SetCamAvDistMax()
        {
            await _rlv.ProcessMessage("@setcam_avdistmax:30=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.AvDistMax);
            Assert.Equal(30f, cameraRestrictions.AvDistMax.Value, FloatTolerance);
        }
        [Fact]
        public async Task SetCamAvDistMax_Synonym()
        {
            await _rlv.ProcessMessage("@camdistmax:30=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.AvDistMax);
            Assert.Equal(30f, cameraRestrictions.AvDistMax.Value, FloatTolerance);
        }
        #endregion

        #region @setcam_avdistmin
        [Fact]
        public async Task SetCamAvDistMin()
        {
            await _rlv.ProcessMessage("@setcam_avdistmin:0.3=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.AvDistMin);
            Assert.Equal(0.3f, cameraRestrictions.AvDistMin.Value, FloatTolerance);
        }

        [Fact]
        public async Task SetCamAvDistMin_Synonym()
        {
            await _rlv.ProcessMessage("@camdistmin:0.3=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.AvDistMin);
            Assert.Equal(0.3f, cameraRestrictions.AvDistMin.Value, FloatTolerance);
        }
        #endregion

        #region @CamDrawAlphaMax
        [Fact]
        public async Task CamDrawAlphaMax()
        {
            await _rlv.ProcessMessage("@CamDrawAlphaMax:0.9=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.DrawAlphaMax);
            Assert.Equal(0.9f, cameraRestrictions.DrawAlphaMax.Value, FloatTolerance);
        }
        #endregion

        #region @camdrawmin:<min_distance>=<y/n>

        [Fact]
        public async Task CamDrawMin()
        {
            await _rlv.ProcessMessage("@camdrawmin:1.75=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.DrawMin);
            Assert.Equal(1.75f, cameraRestrictions.DrawMin.Value, FloatTolerance);
        }

        [Fact]
        public async Task CamDrawMin_Small()
        {
            Assert.False(await _rlv.ProcessMessage("@camdrawmin:0.15=n", _sender.Id, _sender.Name));

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.Null(cameraRestrictions.DrawMin);
        }

        #endregion

        #region @camdrawmax:<max_distance>=<y/n>

        [Fact]
        public async Task CamDrawMax()
        {
            await _rlv.ProcessMessage("@camdrawmax:1.75=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.DrawMax);
            Assert.Equal(1.75f, cameraRestrictions.DrawMax.Value, FloatTolerance);
        }

        [Fact]
        public async Task CamDrawMax_Small()
        {
            Assert.False(await _rlv.ProcessMessage("@camdrawmax:0.15=n", _sender.Id, _sender.Name));

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.Null(cameraRestrictions.DrawMax);
        }

        #endregion

        #region @camdrawalphamin:<min_distance>=<y/n>

        [Fact]
        public async Task CamDrawAlphaMin()
        {
            await _rlv.ProcessMessage("@camdrawalphamin:1.75=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.NotNull(cameraRestrictions.DrawAlphaMin);
            Assert.Equal(1.75f, cameraRestrictions.DrawAlphaMin.Value, FloatTolerance);
        }

        #endregion

        #region @CamDrawColor

        [Fact]
        public async Task CamDrawColor()
        {
            await _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();

            Assert.NotNull(cameraRestrictions.DrawColor);
            Assert.Equal(0.1f, cameraRestrictions.DrawColor.Value.X, FloatTolerance);
            Assert.Equal(0.2f, cameraRestrictions.DrawColor.Value.Y, FloatTolerance);
            Assert.Equal(0.3f, cameraRestrictions.DrawColor.Value.Z, FloatTolerance);
        }

        [Fact]
        public void CamDrawColor_Default()
        {
            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.Null(cameraRestrictions.DrawColor);
        }

        [Fact]
        public async Task CamDrawColor_Large()
        {
            await _rlv.ProcessMessage("@CamDrawColor:5;6;7=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();

            Assert.NotNull(cameraRestrictions.DrawColor);
            Assert.Equal(1.0f, cameraRestrictions.DrawColor.Value.X, FloatTolerance);
            Assert.Equal(1.0f, cameraRestrictions.DrawColor.Value.Y, FloatTolerance);
            Assert.Equal(1.0f, cameraRestrictions.DrawColor.Value.Z, FloatTolerance);
        }

        [Fact]
        public async Task CamDrawColor_Negative()
        {
            await _rlv.ProcessMessage("@CamDrawColor:-5;-6;-7=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();

            Assert.NotNull(cameraRestrictions.DrawColor);
            Assert.Equal(0.0f, cameraRestrictions.DrawColor.Value.X, FloatTolerance);
            Assert.Equal(0.0f, cameraRestrictions.DrawColor.Value.Y, FloatTolerance);
            Assert.Equal(0.0f, cameraRestrictions.DrawColor.Value.Z, FloatTolerance);
        }

        [Fact]
        public async Task CamDrawColor_Removal()
        {
            await _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=y", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.Null(cameraRestrictions.DrawColor);
        }

        [Fact]
        public async Task CamDrawColor_Multi()
        {
            await _rlv.ProcessMessage("@CamDrawColor:0.1;0.2;0.3=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@CamDrawColor:0.2;0.3;0.6=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();

            Assert.NotNull(cameraRestrictions.DrawColor);
            Assert.Equal(0.15f, cameraRestrictions.DrawColor.Value.X, FloatTolerance);
            Assert.Equal(0.25f, cameraRestrictions.DrawColor.Value.Y, FloatTolerance);
            Assert.Equal(0.45f, cameraRestrictions.DrawColor.Value.Z, FloatTolerance);
        }
        #endregion

        #region @camunlock
        [Fact]
        public async Task CanSetCamUnlock()
        {
            await _rlv.ProcessMessage("@setcam_unlock=n", _sender.Id, _sender.Name);
            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.True(cameraRestrictions.IsLocked);

            await _rlv.ProcessMessage("@setcam_unlock=y", _sender.Id, _sender.Name);
            cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.False(cameraRestrictions.IsLocked);
        }
        #endregion

        #region @setcam_unlock
        [Fact]
        public async Task CanCamUnlock()
        {
            await _rlv.ProcessMessage("@camunlock=n", _sender.Id, _sender.Name);
            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.True(cameraRestrictions.IsLocked);

            await _rlv.ProcessMessage("@camunlock=y", _sender.Id, _sender.Name);
            cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.False(cameraRestrictions.IsLocked);
        }
        #endregion

        #region @camavdist
        [Fact]
        public async Task CamAvDist()
        {
            await _rlv.ProcessMessage("@CamAvDist:5=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();

            Assert.NotNull(cameraRestrictions.AvDist);
            Assert.Equal(5f, cameraRestrictions.AvDist.Value, FloatTolerance);
        }
        #endregion

        #region @camtextures @setcam_textures[:texture_uuid]=<y/n>

        [Theory]
        [InlineData("setcam_textures")]
        [InlineData("camtextures")]
        public async Task SetCamTextures(string command)
        {
            await _rlv.ProcessMessage($"@{command}=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.Equal(Guid.Empty, cameraRestrictions.Texture);
        }

        [Theory]
        [InlineData("setcam_textures")]
        [InlineData("camtextures")]
        public async Task SetCamTextures_Single(string command)
        {
            var textureId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage($"@{command}:{textureId1}=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.Equal(textureId1, cameraRestrictions.Texture);
        }

        [Theory]
        [InlineData("setcam_textures", "setcam_textures")]
        [InlineData("setcam_textures", "camtextures")]
        [InlineData("camtextures", "camtextures")]
        [InlineData("camtextures", "setcam_textures")]
        public async Task SetCamTextures_Multiple_Synonyms(string command1, string command2)
        {
            var textureId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var textureId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@{command1}:{textureId1}=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@{command2}:{textureId2}=n", _sender.Id, _sender.Name);

            var cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.True(cameraRestrictions.Texture == textureId2);

            await _rlv.ProcessMessage($"@{command1}:{textureId2}=y", _sender.Id, _sender.Name);

            cameraRestrictions = _rlv.Permissions.GetCameraRestrictions();
            Assert.True(cameraRestrictions.Texture == textureId1);
        }

        #endregion

        #region @getcam_avdistmin=<channel_number>
        [Fact]
        public async Task GetCamAvDistMin()
        {
            var actual = _callbacks.RecordReplies();

            var distance = 12.34f;

            _callbacks.Setup(e =>
                e.TryGetCamAvDistMinAsync()
            ).ReturnsAsync((true, distance));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, distance.ToString()),
            };

            Assert.True(await _rlv.ProcessMessage("@getcam_avdistmin=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetCamAvDistMin_Default()
        {
            var actual = _callbacks.RecordReplies();

            var distance = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamAvDistMinAsync()
            ).ReturnsAsync((false, distance));

            Assert.False(await _rlv.ProcessMessage("@getcam_avdistmin=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion

        #region @getcam_avdistmax=<channel_number>
        [Fact]
        public async Task GetCamAvDistMax()
        {
            var actual = _callbacks.RecordReplies();

            var distance = 12.34f;

            _callbacks.Setup(e =>
                e.TryGetCamAvDistMaxAsync()
            ).ReturnsAsync((true, distance));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, distance.ToString()),
            };

            Assert.True(await _rlv.ProcessMessage("@getcam_avdistmax=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetCamAvDistMax_Default()
        {
            var actual = _callbacks.RecordReplies();

            var distance = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamAvDistMaxAsync()
            ).ReturnsAsync((false, distance));

            Assert.False(await _rlv.ProcessMessage("@getcam_avdistmax=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion

        #region @getcam_fovmin=<channel_number>
        [Fact]
        public async Task GetCamFovMin()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 15.25f;

            _callbacks.Setup(e =>
                e.TryGetCamFovMinAsync()
            ).ReturnsAsync((true, fov));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, fov.ToString()),
            };

            Assert.True(await _rlv.ProcessMessage("@getcam_fovmin=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetCamFovMin_Default()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamFovMinAsync()
            ).ReturnsAsync((false, fov));

            Assert.False(await _rlv.ProcessMessage("@getcam_fovmin=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion

        #region @getcam_fovmax=<channel_number>
        [Fact]
        public async Task GetCamFovMax()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 45.75f;
            _callbacks.Setup(e =>
                e.TryGetCamFovMaxAsync()
            ).ReturnsAsync((true, fov));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, fov.ToString()),
            };

            Assert.True(await _rlv.ProcessMessage("@getcam_fovmax=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetCamFovMax_Default()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamFovMaxAsync()
            ).ReturnsAsync((false, fov));

            Assert.False(await _rlv.ProcessMessage("@getcam_fovmax=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion

        #region @getcam_zoommin=<channel_number>
        [Fact]
        public async Task GetCamZoomMin()
        {
            var actual = _callbacks.RecordReplies();

            var zoom = 0.75f;

            _callbacks.Setup(e =>
                e.TryGetCamZoomMinAsync()
            ).ReturnsAsync((true, zoom));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, zoom.ToString()),
            };

            Assert.True(await _rlv.ProcessMessage("@getcam_zoommin=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetCamZoomMin_Default()
        {
            var actual = _callbacks.RecordReplies();

            var zoom = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamZoomMinAsync()
            ).ReturnsAsync((false, zoom));

            Assert.False(await _rlv.ProcessMessage("@getcam_zoommin=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion

        #region @getcam_fov=<channel_number>
        [Fact]
        public async Task GetCamFov()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 25.5f;

            _callbacks.Setup(e =>
                e.TryGetCamFovAsync()
            ).ReturnsAsync((true, fov));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, fov.ToString()),
            };

            Assert.True(await _rlv.ProcessMessage("@getcam_fov=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetCamFov_Default()
        {
            var actual = _callbacks.RecordReplies();

            var fov = 0.0f;
            _callbacks.Setup(e =>
                e.TryGetCamFovAsync()
            ).ReturnsAsync((false, fov));

            Assert.False(await _rlv.ProcessMessage("@getcam_fov=1234", _sender.Id, _sender.Name));
            Assert.Empty(actual);
        }
        #endregion
    }
}
