using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests;

public class EntityTests
{
    [Fact]
    public void PointingDirection_IsNullByDefault()
    {
        var entity = new Entity(new EntityId(Guid.NewGuid()), "test");

        entity.PointingDirection.Should().BeNull();
    }

    [Fact]
    public void PointingDirection_CanBeSet()
    {
        var entity = new Entity(new EntityId(Guid.NewGuid()), "test");
        var direction = new Direction(1.0, 0.0);

        entity.PointingDirection = direction;

        entity.PointingDirection.Should().Be(direction);
    }

    [Fact]
    public void PointingDirection_CanBeClearedBackToNull()
    {
        var entity = new Entity(new EntityId(Guid.NewGuid()), "test");
        entity.PointingDirection = new Direction(0.0, 1.0);

        entity.PointingDirection = null;

        entity.PointingDirection.Should().BeNull();
    }
}
