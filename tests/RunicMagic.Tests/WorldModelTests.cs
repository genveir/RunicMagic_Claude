using RunicMagic.World;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests;

public class WorldModelTests
{
    private static Entity MakeEntity(long x, long y, long width, long height) =>
        new(EntityId.New(), EntityType.Object, "test") { X = x, Y = y, Width = width, Height = height };

    // ── GetEntitiesInArea ─────────────────────────────────────────────────────

    [Fact]
    public void GetEntitiesInArea_ReturnsOverlappingEntity()
    {
        var world = new WorldModel();
        var entity = MakeEntity(x: 10, y: 10, width: 20, height: 20);
        world.Add(entity);

        var result = world.GetEntitiesInArea(new Rectangle(0, 0, 25, 25));

        Assert.Contains(entity, result);
    }

    [Fact]
    public void GetEntitiesInArea_DoesNotReturnEntityOutsideArea()
    {
        var world = new WorldModel();
        var entity = MakeEntity(x: 100, y: 100, width: 10, height: 10);
        world.Add(entity);

        var result = world.GetEntitiesInArea(new Rectangle(0, 0, 50, 50));

        Assert.DoesNotContain(entity, result);
    }

    [Fact]
    public void GetEntitiesInArea_DoesNotReturnEntityThatOnlyTouchesEdge()
    {
        // Adjacent entity shares an edge but does not overlap — not returned by IntersectsWith.
        var world = new WorldModel();
        var entity = MakeEntity(x: 30, y: 0, width: 10, height: 10);
        world.Add(entity);

        // Area right edge at x=25; entity left edge at x=25 — edge-adjacent, no overlap.
        var result = world.GetEntitiesInArea(new Rectangle(0, 0, 50, 50));

        Assert.DoesNotContain(entity, result);
    }

    // ── GetTouchingEntities ───────────────────────────────────────────────────

    [Fact]
    public void GetTouchingEntities_ReturnsOverlappingEntity()
    {
        var world = new WorldModel();
        var source = MakeEntity(x: 0, y: 0, width: 20, height: 20);
        var other = MakeEntity(x: 10, y: 10, width: 20, height: 20);
        world.Add(source);
        world.Add(other);

        var result = world.GetTouchingEntities(source);

        Assert.Contains(other, result);
    }

    [Fact]
    public void GetTouchingEntities_ReturnsAdjacentEntity()
    {
        var world = new WorldModel();
        var source = MakeEntity(x: 0, y: 0, width: 20, height: 20);
        var other = MakeEntity(x: 15, y: 0, width: 10, height: 10); // left edge (10) == source right edge (10)
        world.Add(source);
        world.Add(other);

        var result = world.GetTouchingEntities(source);

        Assert.Contains(other, result);
    }

    [Fact]
    public void GetTouchingEntities_DoesNotReturnSelf()
    {
        var world = new WorldModel();
        var source = MakeEntity(x: 0, y: 0, width: 20, height: 20);
        world.Add(source);

        var result = world.GetTouchingEntities(source);

        Assert.DoesNotContain(source, result);
    }

    [Fact]
    public void GetTouchingEntities_DoesNotReturnDistantEntity()
    {
        var world = new WorldModel();
        var source = MakeEntity(x: 0, y: 0, width: 10, height: 10);
        var other = MakeEntity(x: 100, y: 100, width: 10, height: 10);
        world.Add(source);
        world.Add(other);

        var result = world.GetTouchingEntities(source);

        Assert.DoesNotContain(other, result);
    }

    // ── GetEntitiesWithinDistance ─────────────────────────────────────────────

    [Fact]
    public void GetEntitiesWithinDistance_ReturnsEntityWithinThreshold()
    {
        var world = new WorldModel();
        // Source centre at (0,0); target bbox x:[495,505], y:[-5,5] → nearest point (495,0) → gap=495
        var source = MakeEntity(x: 0, y: 0, width: 10, height: 10);
        var target = MakeEntity(x: 500, y: 0, width: 10, height: 10);
        world.Add(source);
        world.Add(target);

        var result = world.GetEntitiesWithinDistance(source, distance: 500);

        Assert.Contains(target, result);
    }

    [Fact]
    public void GetEntitiesWithinDistance_DoesNotReturnEntityBeyondThreshold()
    {
        var world = new WorldModel();
        // Source centre at (0,0); target bbox x:[496,506], y:[-5,5] → nearest point (496,0) → gap=496 < 500 — shift further
        // target at (1010,0) width=10 → bbox x:[1005,1015] → gap=1005 > 500
        var source = MakeEntity(x: 0, y: 0, width: 10, height: 10);
        var target = MakeEntity(x: 1010, y: 0, width: 10, height: 10);
        world.Add(source);
        world.Add(target);

        var result = world.GetEntitiesWithinDistance(source, distance: 500);

        Assert.DoesNotContain(target, result);
    }

    [Fact]
    public void GetEntitiesWithinDistance_DoesNotReturnSelf()
    {
        var world = new WorldModel();
        var source = MakeEntity(x: 0, y: 0, width: 10, height: 10);
        world.Add(source);

        var result = world.GetEntitiesWithinDistance(source, distance: 500);

        Assert.DoesNotContain(source, result);
    }

    [Fact]
    public void GetEntitiesWithinDistance_ReturnsOverlappingEntity()
    {
        var world = new WorldModel();
        var source = MakeEntity(x: 0, y: 0, width: 20, height: 20);
        var other = MakeEntity(x: 5, y: 5, width: 20, height: 20);
        world.Add(source);
        world.Add(other);

        var result = world.GetEntitiesWithinDistance(source, distance: 500);

        Assert.Contains(other, result);
    }

    // ── GetContainedEntities ──────────────────────────────────────────────────

    [Fact]
    public void GetContainedEntities_ReturnsFullyContainedEntity()
    {
        var world = new WorldModel();
        var container = MakeEntity(x: 0, y: 0, width: 100, height: 100);
        var inner = MakeEntity(x: 10, y: 10, width: 20, height: 20);
        world.Add(container);
        world.Add(inner);

        var result = world.GetContainedEntities(container);

        Assert.Contains(inner, result);
    }

    [Fact]
    public void GetContainedEntities_DoesNotReturnPartiallyOverlappingEntity()
    {
        var world = new WorldModel();
        var container = MakeEntity(x: 0, y: 0, width: 50, height: 50);
        var partial = MakeEntity(x: 40, y: 40, width: 30, height: 30); // extends outside
        world.Add(container);
        world.Add(partial);

        var result = world.GetContainedEntities(container);

        Assert.DoesNotContain(partial, result);
    }

    [Fact]
    public void GetContainedEntities_DoesNotReturnSelf()
    {
        var world = new WorldModel();
        var container = MakeEntity(x: 0, y: 0, width: 100, height: 100);
        world.Add(container);

        var result = world.GetContainedEntities(container);

        Assert.DoesNotContain(container, result);
    }
}
