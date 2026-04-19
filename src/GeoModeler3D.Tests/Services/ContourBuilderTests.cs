using System.Numerics;
using GeoModeler3D.Core.Services;
using Xunit;

namespace GeoModeler3D.Tests.Services;

public class ContourBuilderTests
{
    [Fact]
    public void EmptyInput_ReturnsEmptyList()
    {
        var chains = ContourBuilder.Build([]);
        Assert.Empty(chains);
    }

    [Fact]
    public void ThreeSegmentsFormingTriangle_ReturnsOneClosedChain()
    {
        var a = new Vector3(0, 0, 0);
        var b = new Vector3(1, 0, 0);
        var c = new Vector3(0, 1, 0);

        var segments = new List<(Vector3 A, Vector3 B)>
        {
            (a, b), (b, c), (c, a)
        };

        var chains = ContourBuilder.Build(segments);

        Assert.Single(chains);
        Assert.True(chains[0].IsClosed);
        Assert.Equal(3, chains[0].Points.Count);
    }

    [Fact]
    public void TwoDisconnectedSegments_ReturnsTwoOpenChains()
    {
        var segments = new List<(Vector3 A, Vector3 B)>
        {
            (new Vector3(0, 0, 0), new Vector3(1, 0, 0)),
            (new Vector3(5, 0, 0), new Vector3(6, 0, 0))
        };

        var chains = ContourBuilder.Build(segments);

        Assert.Equal(2, chains.Count);
        Assert.All(chains, c => Assert.False(c.IsClosed));
    }

    [Fact]
    public void SingleSegment_ReturnsOneOpenChain()
    {
        var segments = new List<(Vector3 A, Vector3 B)>
        {
            (new Vector3(0, 0, 0), new Vector3(1, 0, 0))
        };

        var chains = ContourBuilder.Build(segments);

        Assert.Single(chains);
        Assert.False(chains[0].IsClosed);
        Assert.Equal(2, chains[0].Points.Count);
    }

    [Fact]
    public void FourSegmentSquare_ReturnsOneClosedChainFourPoints()
    {
        var a = new Vector3(0, 0, 0);
        var b = new Vector3(1, 0, 0);
        var c = new Vector3(1, 1, 0);
        var d = new Vector3(0, 1, 0);

        var segments = new List<(Vector3 A, Vector3 B)>
        {
            (a, b), (b, c), (c, d), (d, a)
        };

        var chains = ContourBuilder.Build(segments);

        Assert.Single(chains);
        Assert.True(chains[0].IsClosed);
        Assert.Equal(4, chains[0].Points.Count);
    }
}
