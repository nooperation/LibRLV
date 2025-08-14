using Moq;

namespace LibRLV.Tests.Commands
{
    public class DetachMeCommandTests : RestrictionsBase
    {
        #region @detachme=force
        [Fact]
        public async Task DetachMeForce()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.DetachAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            var expected = new HashSet<Guid>()
            {
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
            };

            // Act
            await _rlv.ProcessMessage("@detachme=force", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.DetachAsync(
                    It.Is<IReadOnlyList<Guid>>(ids =>
                        ids != null &&
                        ids.Count == expected.Count &&
                        expected.SetEquals(ids)
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );

            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task DetachMeForce_IgnoreNoStrip()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "nostrip Party Hat";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.DetachAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            var expected = new HashSet<Guid>()
            {
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
            };

            // Act
            await _rlv.ProcessMessage("@detachme=force", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.DetachAsync(
                    It.Is<IReadOnlyList<Guid>>(ids =>
                        ids != null &&
                        ids.Count == expected.Count &&
                        expected.SetEquals(ids)
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );

            _actionCallbacks.VerifyNoOtherCalls();
        }
        #endregion

    }
}
