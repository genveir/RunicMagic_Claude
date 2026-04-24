using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests;

public class EntityTests
{
    [Fact]
    public void PointingDirection_IsNullByDefault()
    {
        var entity = new EntityBuilder().Build();

        entity.PointingDirection.Should().BeNull();
    }

    [Fact]
    public void PointingDirection_CanBeSet()
    {
        var entity = new EntityBuilder().Build();
        var direction = new Direction(1.0, 0.0);

        entity.PointingDirection = direction;

        entity.PointingDirection.Should().Be(direction);
    }

    [Fact]
    public void PointingDirection_CanBeClearedBackToNull()
    {
        var entity = new EntityBuilder().Build();
        entity.PointingDirection = new Direction(0.0, 1.0);

        entity.PointingDirection = null;

        entity.PointingDirection.Should().BeNull();
    }
}
