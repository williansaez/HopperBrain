using HopperBrain;

namespace HopperBrain.Tests;

public class GroupPartitionerTests
{
    /*********
    ** Helpers
    *********/
    private static Node N(NodeKind kind, int x, int y, int w = 1, int h = 1) => new(kind, new TileArea(x, y, w, h));

    /// <summary>Build nodes from a grid where B=chest, H=hopper, M=machine, C=connector, '.'=empty; rows separated by '/'. Indices are row-major.</summary>
    private static List<Node> Grid(string rows)
    {
        List<Node> nodes = [];
        string[] lines = rows.Split('/');
        for (int y = 0; y < lines.Length; y++)
        {
            for (int x = 0; x < lines[y].Length; x++)
            {
                NodeKind? kind = lines[y][x] switch
                {
                    'B' => NodeKind.Chest,
                    'H' => NodeKind.Hopper,
                    'M' => NodeKind.Machine,
                    'C' => NodeKind.Connector,
                    _ => null
                };
                if (kind != null)
                    nodes.Add(N(kind.Value, x, y));
            }
        }
        return nodes;
    }

    /// <summary>Assert the partition equals the expected groups, ignoring order.</summary>
    private static void AssertGroups(IReadOnlyList<Node> nodes, params int[][] expected)
    {
        var actual = GroupPartitioner.Partition(nodes)
            .Select(g => string.Join(",", g.OrderBy(i => i)))
            .OrderBy(s => s)
            .ToList();
        var wanted = expected
            .Select(g => string.Join(",", g.OrderBy(i => i)))
            .OrderBy(s => s)
            .ToList();
        Assert.Equal(wanted, actual);
    }

    private static int[] All(int count) => Enumerable.Range(0, count).ToArray();


    /*********
    ** Spec examples
    *********/
    [Fact]
    public void ChestMachine_NoHopper_AllSeparate() => AssertGroups(Grid("BMM"), [0], [1, 2]);

    [Fact]
    public void ChestHopperMachines_OneGroup() => AssertGroups(Grid("BHMM"), [0, 1, 2, 3]);

    [Fact]
    public void ChestMachineHopper_ChestOut() => AssertGroups(Grid("BMH"), [0], [1, 2]);

    [Fact]
    public void ChestChestHopperMachine_FarChestOut() => AssertGroups(Grid("BBHM"), [0], [1, 2, 3]);

    [Fact]
    public void Grid3x3_DiagonalChestsJoin() => AssertGroups(Grid("BBB/MHM/MMM"), All(9));

    [Fact]
    public void TwoHoppersLinkedThroughChests_OneGroup() => AssertGroups(Grid("BBB/BHB/BBB/MHM/MMM"), All(15));

    [Fact]
    public void HoppersDirectlyAdjacent_Connect() => AssertGroups(Grid("HH"), [0, 1]);

    [Fact]
    public void ConnectorChain_HopperToMachine_Connects() => AssertGroups(Grid("HCCM"), [0, 1, 2, 3]);

    [Fact]
    public void ConnectorDoesNotLinkChest()
    {
        AssertGroups(Grid("BCM"), [0], [1, 2]);
        AssertGroups(Grid("BCH"), [0], [1, 2]);
    }

    [Fact]
    public void HopperAloneWithMachines_OneGroup() => AssertGroups(Grid("HMM"), [0, 1, 2]);


    /*********
    ** Review focus
    *********/
    [Fact]
    public void MultiTileMachine_TouchingHopperOnAnyTile_Connects()
    {
        // 1x2 machine at (0,0)-(0,1); hopper at (1,1) touches only its lower tile
        var nodes = new List<Node> { N(NodeKind.Machine, 0, 0, 1, 2), N(NodeKind.Hopper, 1, 1) };
        AssertGroups(nodes, [0, 1]);
    }

    [Fact]
    public void MultiTileMachine_FarFromHopper_DoesNotConnect()
    {
        var nodes = new List<Node> { N(NodeKind.Machine, 0, 0, 1, 2), N(NodeKind.Hopper, 2, 1) };
        AssertGroups(nodes, [0], [1]);
    }

    [Fact]
    public void OverlappingAreas_Connect()
    {
        // connector and machine on the same tile, hopper next to them
        var nodes = new List<Node> { N(NodeKind.Connector, 0, 0), N(NodeKind.Machine, 0, 0), N(NodeKind.Hopper, 1, 0) };
        AssertGroups(nodes, [0, 1, 2]);
    }


    /*********
    ** Rule table
    *********/
    [Theory]
    [InlineData(NodeKind.Hopper, NodeKind.Machine, true)]
    [InlineData(NodeKind.Hopper, NodeKind.Chest, true)]
    [InlineData(NodeKind.Hopper, NodeKind.Hopper, true)]
    [InlineData(NodeKind.Hopper, NodeKind.Connector, true)]
    [InlineData(NodeKind.Machine, NodeKind.Machine, true)]
    [InlineData(NodeKind.Machine, NodeKind.Connector, true)]
    [InlineData(NodeKind.Connector, NodeKind.Connector, true)]
    [InlineData(NodeKind.Chest, NodeKind.Machine, false)]
    [InlineData(NodeKind.Chest, NodeKind.Chest, false)]
    [InlineData(NodeKind.Chest, NodeKind.Connector, false)]
    public void CanConnect_MatchesRuleTable(NodeKind a, NodeKind b, bool expected)
    {
        Assert.Equal(expected, GroupPartitioner.CanConnect(a, b));
        Assert.Equal(expected, GroupPartitioner.CanConnect(b, a));
    }
}
