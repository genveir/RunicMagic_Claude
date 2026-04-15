using RunicMagic.World;
using Xunit;

namespace RunicMagic.Tests;

public class WorldModelTests
{
    private static Entity MakeEntity(int x, int y, int width, int height) =>
        new(EntityId.New(), EntityType.Object, "test") { Bounds = new Rectangle(x, y, width, height) };

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
        var entity = MakeEntity(x: 50, y: 0, width: 10, height: 10);
        world.Add(entity);

        // Area ends at x=50; entity starts at x=50 — edge-adjacent, no overlap.
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
        var other = MakeEntity(x: 20, y: 0, width: 10, height: 10); // shares right edge of source
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
