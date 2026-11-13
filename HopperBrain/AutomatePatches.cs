using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate;
using Pathoschild.Stardew.Automate.Framework;
using StardewModdingAPI;

namespace HopperBrain;

/// <summary>Harmony patches which re-split Automate's machine groups using the hopper connection rules.</summary>
internal static class AutomatePatches
{
    /// <summary>The qualified item ID for the vanilla hopper.</summary>
    private const string HopperId = "(BC)275";

    /// <summary>Called if a patch fails at runtime, so automation can be stopped instead of falling back to vanilla Automate.</summary>
    private static Action<Exception> OnFailure = _ => { };

    /// <summary>Apply the patches. Throws if Automate's internals don't match.</summary>
    /// <param name="harmony">The Harmony instance.</param>
    /// <param name="onFailure">Called if the patch fails while the game runs.</param>
    public static void Apply(Harmony harmony, Action<Exception> onFailure)
    {
        OnFailure = onFailure;

        harmony.Patch(
            original: AccessTools.Method(typeof(MachineGroupFactory), nameof(MachineGroupFactory.GetMachineGroups))
                ?? throw new InvalidOperationException("Can't find Automate's MachineGroupFactory.GetMachineGroups."),
            postfix: new HarmonyMethod(typeof(AutomatePatches), nameof(After_GetMachineGroups))
        );
    }

    /// <summary>Replace Automate's groups with groups split by the hopper rules.</summary>
    private static void After_GetMachineGroups(ref IEnumerable<IMachineGroup> __result, Func<IContainer[], StorageManager> ___BuildStorage, IMonitor ___Monitor)
    {
        try
        {
            __result = Split(__result, ___BuildStorage, ___Monitor);
        }
        catch (Exception ex)
        {
            __result = [];
            OnFailure(ex);
        }
    }

    /// <summary>Split each Automate group into the parts connected under the hopper rules.</summary>
    private static List<IMachineGroup> Split(IEnumerable<IMachineGroup> groups, Func<IContainer[], StorageManager> buildStorage, IMonitor monitor)
    {
        List<IMachineGroup> result = [];

        foreach (IMachineGroup group in groups)
        {
            if (group.LocationKey is null)
            {
                result.Add(group);
                continue;
            }

            // collect nodes (machines, containers, then connector tiles not covered by either)
            List<Node> nodes = [];
            List<object> entities = [];
            HashSet<Vector2> covered = [];

            foreach (IMachine machine in group.Machines)
                AddEntity(machine, NodeKind.Machine, machine.TileArea);
            foreach (IContainer container in group.Containers)
                AddEntity(container, container.TypeId == HopperId ? NodeKind.Hopper : NodeKind.Chest, container.TileArea);

            foreach (Vector2 tile in group.GetTiles(group.LocationKey))
            {
                if (covered.Contains(tile))
                    continue;
                nodes.Add(new Node(NodeKind.Connector, new TileArea((int)tile.X, (int)tile.Y, 1, 1)));
                entities.Add(tile);
            }

            // rebuild one Automate group per part
            foreach (List<int> part in GroupPartitioner.Partition(nodes))
            {
                List<IMachine> machines = [];
                List<IContainer> containers = [];
                HashSet<Vector2> tiles = [];

                foreach (int i in part)
                {
                    switch (entities[i])
                    {
                        case IMachine machine:
                            machines.Add(machine);
                            break;
                        case IContainer container:
                            containers.Add(container);
                            break;
                    }

                    TileArea area = nodes[i].Area;
                    for (int x = area.X; x < area.X + area.Width; x++)
                    {
                        for (int y = area.Y; y < area.Y + area.Height; y++)
                            tiles.Add(new Vector2(x, y));
                    }
                }

                // part indices are ascending, so machines keep Automate's priority order
                result.Add(new MachineGroup(group.LocationKey, machines, containers, tiles, buildStorage, monitor));
            }

            void AddEntity(object entity, NodeKind kind, Rectangle area)
            {
                nodes.Add(new Node(kind, new TileArea(area.X, area.Y, area.Width, area.Height)));
                entities.Add(entity);
                for (int x = area.X; x < area.Right; x++)
                {
                    for (int y = area.Y; y < area.Bottom; y++)
                        covered.Add(new Vector2(x, y));
                }
            }
        }

        return result;
    }
}
