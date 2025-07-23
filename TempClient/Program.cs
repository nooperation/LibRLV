using LibRLV;
using LibRLV.EventArguments;
using OpenMetaverse;
using System.Collections.Generic;
using System.Linq;

namespace TempClient
{
    internal class Program
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

        public class MyRLVCallbacks : RLVCallbacksDefault
        {
            public InventoryTree SharedRoot { get; set; } = default!;

            public override Task<string> ProvideDataAsync(RLVDataRequest request, List<object> list, CancellationToken cancellationToken)
            {
                Console.WriteLine($"DataProvider: {request} {string.Join(',', list)}");

                return Task.FromResult(new OpenMetaverse.UUID(Guid.NewGuid()).ToString());
            }

            public override Task SendReplyAsync(int channel, string message, CancellationToken cancellationToken)
            {
                Console.WriteLine($"[{channel}] {message}");
                return Task.CompletedTask;
            }

            public override Task<bool> TryGetRlvInventoryTree(out InventoryTree sharedFolder)
            {
                sharedFolder = SharedRoot;
                return Task.FromResult(true);
            }
        }

        private void PrintTree(RLV rlv, InventoryTree tree, int indent = 0)
        {
            var indentString = string.Join("", Enumerable.Repeat("  ", indent));

            if (rlv.Restrictions.TryGetLockedFolder(tree.Id, out var lockedFolder))
            {
                Console.WriteLine($"{indentString}{tree.Name} [LOCKED] {(lockedFolder.CanDetach ? "" : "(No-Detach)")}{(lockedFolder.CanAttach ? "" : "(No-Attach)")}");
            }
            else
            {
                Console.WriteLine($"{indentString}{tree.Name}");
            }

            foreach (var item in tree.Children)
            {
                PrintTree(rlv, item, indent + 1);
            }
        }

        public Program()
        {
            var tree = new InventoryTree()
            {
                Id = new UUID(Guid.NewGuid()),
                Name = "#RLV",
                Parent = null,
                Children = new List<InventoryTree>()
            };

            var sub1 = new InventoryTree()
            {
                Id = new UUID(Guid.NewGuid()),
                Name = "Sub1",
                Parent = tree,
                Children = new List<InventoryTree>()
            }; 
            tree.Children.Add(sub1);

            var foo = new InventoryTree
            {
                Id = new UUID(Guid.NewGuid()),
                Name = "Foo",
                Parent = tree,
                Children = new List<InventoryTree>()
            };
            tree.Children.Add(foo);

            var bar = new InventoryTree
            {
                Id = new UUID(Guid.NewGuid()),
                Name = "Bar",
                Parent = foo,
                Children = new List<InventoryTree>()
            };
            foo.Children.Add(bar);

            var rlv = new RLV(new MyRLVCallbacks() { SharedRoot = tree }, true);

            var object1 = new RlvObject("First");
            var object2 = new RlvObject("Second");

            rlv.ProcessMessage("@detachallthis:Foo=n", object1.Id, "Locker9000");
            if(!rlv.Restrictions.TryGetLockedFolder(foo.Id, out var lockedFolder))
            {
                Console.WriteLine("Failed");
            }

            Console.WriteLine();

            PrintTree(rlv, tree, 0);

            //rlv.Actions.TpTo += Rlv_TpTo;
            //rlv.Restrictions.RestrictionUpdated += Restrictions_RestrictionUpdated;
            //
            //
            //
            //
            //rlv.ProcessMessage("@notify:5678=add", object1.Id, object1.Name);
            //rlv.ProcessMessage("@accepttprequest=add", object1.Id, object1.Name);
            //rlv.ProcessMessage($"@accepttprequest:{object1.Id}=add", object1.Id, object1.Name);
            //rlv.ProcessMessage("@getstatusall=123", object1.Id, object1.Name);
            //
            //rlv.ProcessMessage("@getsitid=123", object1.Id, object1.Name);
            //
            ////rlv.ProcessMessage("@tpto:1/2/3=force", object1Id.Id,object1.Name);
            ////rlv.ProcessMessage("@tpto:My Land/1/2/3=force", object1Id.Id,object1.Name);
            //rlv.ProcessMessage("@tpto:My Land/1/2/3;3.1415=force", object1.Id, object1.Name);
            //
            //rlv.Blacklist.BlacklistCommand("getstatusall");
            //rlv.ProcessMessage("@getstatusall=1234", object1.Id, object1.Name);
            //
            //rlv.ProcessMessage("@accepttprequest=rem", object1.Id, object1.Name);
            //
            //rlv.RLVManager.GetCamDrawColor
        } 

        private void Restrictions_RestrictionUpdated(object? sender, RestrictionUpdatedEventArgs e)
        {
            Console.WriteLine($"Restriction update: {(e.IsDeleted ? "REM" : "ADD")} {e.Restriction}");
        }

        private void Rlv_TpTo(object? sender, TpToEventArgs e)
        {
            Console.WriteLine($"Tp To: {e.X}/{e.Y}/{e.Z} Region={e.RegionName} Lookat={e.Lookat}");
        }

        static void Main()
        {
            _ = new Program();
        }
    }
}
