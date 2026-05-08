using RunicMagic.World.Entities;

namespace RunicMagic.Tests.World.Entities.Capabilities;

public class ReservoirCapabilityTests
{
    private static Entity MakeEntityWithReservoir(long current, long max)
    {
        return new EntityBuilder()
            .WithReservoir(max: () => max, current: () => current)
            .Build();
    }

    private static Entity MakeEntityWithReservoirAndScope(long current, long max, Func<Entity[]> scope)
    {
        return new EntityBuilder()
            .WithReservoir(max: () => max, current: () => current)
            .WithScope(scope)
            .Build();
    }

    // GetMaxIncludingScope

    [Fact]
    public void GetMaxIncludingScope_NoScope_ReturnsOwnMax()
    {
        var entity = MakeEntityWithReservoir(max: 500, current: 200);

        var result = entity.Reservoir!.GetMaxIncludingScope(entity);

        result.Should().Be(500);
    }

    [Fact]
    public void GetMaxIncludingScope_EmptyScope_ReturnsOwnMax()
    {
        var entity = MakeEntityWithReservoirAndScope(max: 500, current: 200, scope: () => []);

        var result = entity.Reservoir!.GetMaxIncludingScope(entity);

        result.Should().Be(500);
    }

    [Fact]
    public void GetMaxIncludingScope_ScopeWithReservoirs_ReturnsSumOfAll()
    {
        var scopeA = MakeEntityWithReservoir(max: 300, current: 100);
        var scopeB = MakeEntityWithReservoir(max: 200, current: 50);
        var entity = MakeEntityWithReservoirAndScope(max: 500, current: 200, scope: () => [scopeA, scopeB]);

        var result = entity.Reservoir!.GetMaxIncludingScope(entity);

        result.Should().Be(1000);
    }

    [Fact]
    public void GetMaxIncludingScope_ScopeWithNoReservoir_SkipsThoseEntities()
    {
        var withReservoir = MakeEntityWithReservoir(max: 300, current: 100);
        var withoutReservoir = new EntityBuilder().Build();
        var entity = MakeEntityWithReservoirAndScope(max: 500, current: 200, scope: () => [withReservoir, withoutReservoir]);

        var result = entity.Reservoir!.GetMaxIncludingScope(entity);

        result.Should().Be(800);
    }

    // GetCurrentIncludingScope

    [Fact]
    public void GetCurrentIncludingScope_NoScope_ReturnsOwnCurrent()
    {
        var entity = MakeEntityWithReservoir(max: 500, current: 200);

        var result = entity.Reservoir!.GetCurrentIncludingScope(entity);

        result.Should().Be(200);
    }

    [Fact]
    public void GetCurrentIncludingScope_EmptyScope_ReturnsOwnCurrent()
    {
        var entity = MakeEntityWithReservoirAndScope(max: 500, current: 200, scope: () => []);

        var result = entity.Reservoir!.GetCurrentIncludingScope(entity);

        result.Should().Be(200);
    }

    [Fact]
    public void GetCurrentIncludingScope_ScopeWithReservoirs_ReturnsSumOfAll()
    {
        var scopeA = MakeEntityWithReservoir(max: 300, current: 100);
        var scopeB = MakeEntityWithReservoir(max: 200, current: 50);
        var entity = MakeEntityWithReservoirAndScope(max: 500, current: 200, scope: () => [scopeA, scopeB]);

        var result = entity.Reservoir!.GetCurrentIncludingScope(entity);

        result.Should().Be(350);
    }

    [Fact]
    public void GetCurrentIncludingScope_ScopeWithNoReservoir_SkipsThoseEntities()
    {
        var withReservoir = MakeEntityWithReservoir(max: 300, current: 100);
        var withoutReservoir = new EntityBuilder().Build();
        var entity = MakeEntityWithReservoirAndScope(max: 500, current: 200, scope: () => [withReservoir, withoutReservoir]);

        var result = entity.Reservoir!.GetCurrentIncludingScope(entity);

        result.Should().Be(300);
    }
}
