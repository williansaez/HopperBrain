using System.Collections.Generic;

namespace HopperBrain;

/// <summary>What an Automate entity is, for the hopper connection rules.</summary>
public enum NodeKind { Hopper, Chest, Machine, Connector }

/// <summary>A tile area (in tiles, not pixels).</summary>
public readonly record struct TileArea(int X, int Y, int Width, int Height);

/// <summary>An entity in an Automate group.</summary>
public readonly record struct Node(NodeKind Kind, TileArea Area);

/// <summary>Splits entities into groups using the Hopper Brain connection rules. Pure logic, no game types.</summary>
public static class GroupPartitioner
{
    /// <summary>Whether two touching entities connect. Chests only connect to hoppers; connectors act like machines.</summary>
    public static bool CanConnect(NodeKind a, NodeKind b)
    {
        if (a == NodeKind.Chest || b == NodeKind.Chest)
            return a == NodeKind.Hopper || b == NodeKind.Hopper;
        return true;
    }

    /// <summary>Whether two areas touch, including diagonally or by overlapping.</summary>
    public static bool Touches(TileArea a, TileArea b)
    {
        return a.X - 1 < b.X + b.Width
            && b.X < a.X + a.Width + 1
            && a.Y - 1 < b.Y + b.Height
            && b.Y < a.Y + a.Height + 1;
    }

    /// <summary>Group node indices into connected components.</summary>
    public static List<List<int>> Partition(IReadOnlyList<Node> nodes)
    {
        int[] parent = new int[nodes.Count];
        for (int i = 0; i < parent.Length; i++)
            parent[i] = i;

        int Find(int i)
        {
            while (parent[i] != i)
                i = parent[i] = parent[parent[i]];
            return i;
        }

        // shortcut: O(n²) pair scan, switch to a tile index if a group ever exceeds ~2000 entities
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                if (CanConnect(nodes[i].Kind, nodes[j].Kind) && Touches(nodes[i].Area, nodes[j].Area))
                    parent[Find(i)] = Find(j);
            }
        }

        var groups = new List<List<int>>();
        var byRoot = new Dictionary<int, List<int>>();
        for (int i = 0; i < nodes.Count; i++)
        {
            int root = Find(i);
            if (!byRoot.TryGetValue(root, out List<int>? group))
            {
                byRoot[root] = group = new List<int>();
                groups.Add(group);
            }
            group.Add(i);
        }
        return groups;
    }
}
