namespace LibRLV.Example
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var queryCallbacks = new ExampleQueryCallbacks();
            var actionCallbacks = new ExampleActionCallbacks();

            var rlvService = new RlvService(queryCallbacks, actionCallbacks, enabled: true)
            {
                EnableInstantMessageProcessing = true
            };

            var collarName = "Collar";
            var collarInventoryId = new Guid("11111111-0002-4aaa-8aaa-000000000000");
            var collarPrimId = new Guid("11111111-0002-4aaa-8aaa-ffffffffffff");

            await rlvService.ProcessMessage("@detach=n", collarPrimId, collarName);
            await rlvService.ProcessMessage("@detach=n", collarPrimId, collarName);
            await rlvService.ProcessMessage("@versionnew=293847", collarPrimId, collarName);
            await rlvService.ProcessMessage("@clear", collarPrimId, collarName);
            await rlvService.ProcessMessage("@detach=n", collarPrimId, collarName);
            await rlvService.ProcessMessage("@setgroup=y", collarPrimId, collarName);
            await rlvService.ProcessMessage("@detach=n", collarPrimId, collarName);
            await rlvService.ProcessMessage("@detach=n", collarPrimId, collarName);
            await rlvService.ProcessMessage("@detach=n", collarPrimId, collarName);
            await rlvService.ProcessMessage("@detach=n", collarPrimId, collarName);
            await rlvService.ProcessMessage("@sendim:24036859-e20e-40c4-8088-be6b934c3891=add,recvim:24036859-e20e-40c4-8088-be6b934c3891=add,recvchat:24036859-e20e-40c4-8088-be6b934c3891=add,recvemote:24036859-e20e-40c4-8088-be6b934c3891=add,tplure:24036859-e20e-40c4-8088-be6b934c3891=add,accepttp:24036859-e20e-40c4-8088-be6b934c3891=add,startim:24036859-e20e-40c4-8088-be6b934c3891=add", collarPrimId, collarName);
            await rlvService.ProcessMessage("@sendim:d080c53e-d10d-4975-805c-4002a5eb467f=add,recvim:d080c53e-d10d-4975-805c-4002a5eb467f=add,recvchat:d080c53e-d10d-4975-805c-4002a5eb467f=add,recvemote:d080c53e-d10d-4975-805c-4002a5eb467f=add,tplure:d080c53e-d10d-4975-805c-4002a5eb467f=add,accepttp:d080c53e-d10d-4975-805c-4002a5eb467f=add,startim:d080c53e-d10d-4975-805c-4002a5eb467f=add", collarPrimId, collarName);
            await rlvService.ProcessMessage("@detachallthis:.outfits/.core=y", collarPrimId, collarName);
            await rlvService.ProcessMessage("@detachallthis:.outfits/.core=n", collarPrimId, collarName);

            if (!rlvService.Permissions.CanDetach(collarPrimId, collarPrimId, null, false, RlvAttachmentPoint.Neck, null))
            {
                Console.WriteLine("Collar cannot be removed");
            }

            await rlvService.ProcessMessage("@detachall:.outfits/First Outfit=force", collarPrimId, collarName);
            await rlvService.ProcessMessage("@tpto:Hippo Hollow/181/207/50=force", collarPrimId, collarName);
        }
    }

    public class ExampleQueryCallbacks : IRlvQueryCallbacks
    {
        public Task<bool> ObjectExistsAsync(Guid objectId, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsSittingAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<(bool Success, string EnvironmentSettingValue)> TryGetEnvironmentSettingValueAsync(string settingName, CancellationToken cancellationToken)
        {
            return Task.FromResult((true, "default_value"));
        }

        public Task<(bool Success, string DebugSettingValue)> TryGetDebugSettingValueAsync(string settingName, CancellationToken cancellationToken)
        {
            return Task.FromResult((true, "debug_value"));
        }

        public Task<(bool Success, Guid SitId)> TryGetSitIdAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult((false, Guid.Empty));
        }

        public Task<(bool Success, RlvSharedFolder? SharedFolder)> TryGetSharedFolderAsync(CancellationToken cancellationToken)
        {
            var root = new RlvSharedFolder(new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"), "#RLV");
            var outfitsFolder = root.AddChild(new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), ".outfits");
            var coreFolder = outfitsFolder.AddChild(new Guid("dddddddd-dddd-4ddd-8ddd-dddddddddddd"), ".core");
            var firstOutfitFolder = outfitsFolder.AddChild(new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"), "First Outfit");

            var clothing_businessPants_pelvis = firstOutfitFolder.AddItem(Guid.NewGuid(), "Business Pants (Pelvis)", RlvAttachmentPoint.Pelvis, Guid.NewGuid(), null);
            var clothing_happyShirt = firstOutfitFolder.AddItem(Guid.NewGuid(), "Happy Shirt", null, null, RlvWearableType.Shirt);
            var clothing_retroPants = firstOutfitFolder.AddItem(Guid.NewGuid(), "Retro Pants", null, null, RlvWearableType.Pants);

            return Task.FromResult((true, (RlvSharedFolder?)root));
        }

        public Task<(bool Success, CameraSettings? CameraSettings)> TryGetCameraSettingsAsync(CancellationToken cancellationToken)
        {
            var cameraSettings = new CameraSettings(1.0f, 100.0f, 10.0f, 120.0f, 1.0f, 45.0f);
            return Task.FromResult((true, (CameraSettings?)cameraSettings));
        }

        public Task<(bool Success, string ActiveGroupName)> TryGetActiveGroupNameAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult((true, "Example Group"));
        }

        public Task<(bool Success, IReadOnlyList<RlvInventoryItem>? CurrentOutfit)> TryGetCurrentOutfitAsync(CancellationToken cancellationToken)
        {
            var outfit = new List<RlvInventoryItem>
            {
                new(Guid.NewGuid(), "Example Shirt", Guid.NewGuid(), null,  null, RlvWearableType.Shirt),
                new(Guid.NewGuid(), "Example Pants", Guid.NewGuid(), null, null, RlvWearableType.Pants)
            };
            return Task.FromResult((true, (IReadOnlyList<RlvInventoryItem>?)outfit));
        }
    }

    public class ExampleActionCallbacks : IRlvActionCallbacks
    {
        public Task SendReplyAsync(int channel, string message, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Action: Send reply on channel {channel}: '{message}'");
            return Task.CompletedTask;
        }

        public Task SendInstantMessageAsync(Guid targetUser, string message, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Action: Send IM to {targetUser}: '{message}'");
            return Task.CompletedTask;
        }

        public Task SetRotAsync(float angleInRadians, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Action: Set rotation to {angleInRadians})");
            return Task.CompletedTask;
        }

        public Task AdjustHeightAsync(float distance, float factor, float delta, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Action: Adjusting height - distance: {distance}, factor: {factor}, delta: {delta}");
            return Task.CompletedTask;
        }

        public Task SetCamFOVAsync(float fovInRadians, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Action: Set camera FOV to {fovInRadians} radians");
            return Task.CompletedTask;
        }

        public Task TpToAsync(float x, float y, float z, string? regionName, float? lookat, CancellationToken cancellationToken)
        {
            if (regionName != null)
            {
                Console.WriteLine($"Action: Teleport to ({x}, {y}, {z}) in region {regionName}");
            }
            else
            {
                Console.WriteLine($"Action: Teleport to global coordinates ({x}, {y}, {z}) ");
            }
            return Task.CompletedTask;
        }

        public Task SitAsync(Guid target, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Action: sit on {target}");
            return Task.CompletedTask;
        }

        public Task UnsitAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Action: stand up");
            return Task.CompletedTask;
        }

        public Task SitGroundAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Action: ground sit");
            return Task.CompletedTask;
        }

        public Task RemOutfitAsync(IReadOnlyList<Guid> itemIds, CancellationToken cancellationToken)
        {
            foreach (var itemId in itemIds)
            {
                Console.WriteLine($"Action: Remove item {itemId}");
            }
            return Task.CompletedTask;
        }

        public Task AttachAsync(IReadOnlyList<AttachmentRequest> itemsToAttach, CancellationToken cancellationToken)
        {
            foreach (var item in itemsToAttach)
            {
                Console.WriteLine($"Action: Attach {item.ItemId} to {item.AttachmentPoint}");
            }
            return Task.CompletedTask;
        }

        public Task DetachAsync(IReadOnlyList<Guid> itemIds, CancellationToken cancellationToken)
        {
            foreach (var itemId in itemIds)
            {
                Console.WriteLine($"Action: Detach item {itemId}");
            }

            return Task.CompletedTask;
        }

        public Task SetGroupAsync(Guid groupId, string? roleName, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Action: Setting active group to {groupId} with role '{roleName ?? "default role"}'");
            return Task.CompletedTask;
        }

        public Task SetGroupAsync(string groupName, string? roleName, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Action: Setting active group to '{groupName}' with role '{roleName ?? "default role"}'");
            return Task.CompletedTask;
        }

        public Task SetEnvAsync(string settingName, string settingValue, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Action: Setting environment '{settingName}' = '{settingValue}'");
            return Task.CompletedTask;
        }

        public Task SetDebugAsync(string settingName, string settingValue, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Action: Setting debug setting '{settingName}' = '{settingValue}'");
            return Task.CompletedTask;
        }
    }
}
