using RunicMagic.Tests.Builders;
using RunicMagic.World.Geometry;

namespace RunicMagic.Tests.Geometry;

public class RotationCostCalculatorTests
{
    private const double QuarterTurnRadians = Math.PI / 2;

    [Fact]
    public void ComputeEntityCost_ZeroWidth_ReturnsZero()
    {
        var entity = new EntityBuilder().WithSize(width: 0, height: 100).Build();
        var origin = new Location(0, 0);

        var cost = RotationCostCalculator.ComputeEntityCost(entity, origin, QuarterTurnRadians);

        cost.Should().Be(0);
    }

    [Fact]
    public void ComputeEntityCost_ZeroHeight_ReturnsZero()
    {
        var entity = new EntityBuilder().WithSize(width: 100, height: 0).Build();
        var origin = new Location(0, 0);

        var cost = RotationCostCalculator.ComputeEntityCost(entity, origin, QuarterTurnRadians);

        cost.Should().Be(0);
    }

    [Fact]
    public void ComputeEntityCost_NormalEntity_ReturnsPositiveCost()
    {
        // Entity at (500, 0), well away from origin, so cost is clearly non-zero.
        var entity = new EntityBuilder()
            .WithLocation(x: 500, y: 0)
            .WithSize(width: 200, height: 200)
            .WithWeight(1_000_000)
            .Build();
        var origin = new Location(0, 0);

        var cost = RotationCostCalculator.ComputeEntityCost(entity, origin, QuarterTurnRadians);

        cost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ComputeEntityCost_EntityWithCornerAtOrigin_DoesNotThrow()
    {
        // Entity 200x200 centred at (100, 100): its bottom-left corner in local space
        // lands exactly at the rotation origin, which routes one H call through H(0,0).
        var entity = new EntityBuilder()
            .WithLocation(x: 100, y: 100)
            .WithSize(width: 200, height: 200)
            .WithWeight(1_000_000)
            .Build();
        var origin = new Location(0, 0);

        var cost = RotationCostCalculator.ComputeEntityCost(entity, origin, QuarterTurnRadians);

        cost.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void ComputeEntityCost_ZeroTheta_ReturnsZero()
    {
        var entity = new EntityBuilder()
            .WithLocation(x: 500, y: 0)
            .WithSize(width: 200, height: 200)
            .WithWeight(1_000_000)
            .Build();
        var origin = new Location(0, 0);

        var cost = RotationCostCalculator.ComputeEntityCost(entity, origin, theta: 0);

        cost.Should().Be(0);
    }
}
