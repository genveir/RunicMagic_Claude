using RunicMagic.Controller;
using RunicMagic.Database;
using RunicMagic.World;
using Xunit;

namespace RunicMagic.Tests;

public class EntityFactoryTests
{
    private static EntityData CreatureData(int maxHp, int currentHp) => new(
        Id: Guid.NewGuid(),
        TypeId: (int)EntityType.Creature,
        Label: "creature",
        X: 0, Y: 0, Width: 10, Height: 10,
        HasAgency: false,
        MaxHitPoints: maxHp,
        CurrentHitPoints: currentHp);

    private static EntityData ManaSourceData(int maxCharge, int currentCharge) => new(
        Id: Guid.NewGuid(),
        TypeId: (int)EntityType.ManaSource,
        Label: "mana source",
        X: 0, Y: 0, Width: 10, Height: 10,
        HasAgency: false,
        MaxCharge: maxCharge,
        CurrentCharge: currentCharge);

    // ── Creature reservoir ────────────────────────────────────────────────────

    [Fact]
    public void Creature_Reservoir_DrawsFromLife()
    {
        var world = new WorldModel();
        var factory = new EntityFactory(world);
        var entity = factory.Create(CreatureData(maxHp: 100, currentHp: 100));
        world.Add(entity);

        var drawn = entity.Reservoir!(50);

        Assert.Equal(50, drawn);
        Assert.Equal(50, entity.Life!.CurrentHitPoints);
    }

    [Fact]
    public void Creature_Reservoir_CapsAtCurrentHp()
    {
        var world = new WorldModel();
        var factory = new EntityFactory(world);
        var entity = factory.Create(CreatureData(maxHp: 100, currentHp: 30));
        world.Add(entity);

        var drawn = entity.Reservoir!(50);

        Assert.Equal(30, drawn);
        Assert.Equal(0, entity.Life!.CurrentHitPoints);
    }

    [Fact]
    public void Creature_Reservoir_DrawsZeroWhenDead()
    {
        var world = new WorldModel();
        var factory = new EntityFactory(world);
        var entity = factory.Create(CreatureData(maxHp: 100, currentHp: 0));
        world.Add(entity);

        var drawn = entity.Reservoir!(50);

        Assert.Equal(0, drawn);
    }

    // ── ManaSource reservoir ──────────────────────────────────────────────────

    [Fact]
    public void ManaSource_Reservoir_DrawsFromCharge()
    {
        var world = new WorldModel();
        var factory = new EntityFactory(world);
        var entity = factory.Create(ManaSourceData(maxCharge: 200, currentCharge: 200));
        world.Add(entity);

        var drawn = entity.Reservoir!(75);

        Assert.Equal(75, drawn);
        Assert.Equal(125, entity.Charge!.CurrentCharge);
    }

    [Fact]
    public void ManaSource_Reservoir_CapsAtCurrentCharge()
    {
        var world = new WorldModel();
        var factory = new EntityFactory(world);
        var entity = factory.Create(ManaSourceData(maxCharge: 200, currentCharge: 40));
        world.Add(entity);

        var drawn = entity.Reservoir!(100);

        Assert.Equal(40, drawn);
        Assert.Equal(0, entity.Charge!.CurrentCharge);
    }

    // ── Creature scope ────────────────────────────────────────────────────────

    [Fact]
    public void Creature_Scope_ReturnsTouchingEntities()
    {
        var world = new WorldModel();
        var factory = new EntityFactory(world);

        var caster = factory.Create(CreatureData(maxHp: 100, currentHp: 100) with { X = 0, Y = 0 });
        var nearby = factory.Create(new EntityData(Guid.NewGuid(), (int)EntityType.Object, "nearby",
            X: 10, Y: 0, Width: 10, Height: 10, HasAgency: false));

        world.Add(caster);
        world.Add(nearby);

        var scope = caster.Scope!();

        Assert.Contains(nearby, scope);
    }

    [Fact]
    public void Creature_Scope_DoesNotReturnDistantEntities()
    {
        var world = new WorldModel();
        var factory = new EntityFactory(world);

        var caster = factory.Create(CreatureData(maxHp: 100, currentHp: 100) with { X = 0, Y = 0 });
        var distant = factory.Create(new EntityData(Guid.NewGuid(), (int)EntityType.Object, "distant",
            X: 500, Y: 500, Width: 10, Height: 10, HasAgency: false));

        world.Add(caster);
        world.Add(distant);

        var scope = caster.Scope!();

        Assert.DoesNotContain(distant, scope);
    }

    // ── Object ────────────────────────────────────────────────────────────────

    [Fact]
    public void Object_HasNoReservoir()
    {
        var world = new WorldModel();
        var factory = new EntityFactory(world);
        var entity = factory.Create(new EntityData(
            Guid.NewGuid(), (int)EntityType.Object, "rock",
            X: 0, Y: 0, Width: 5, Height: 5, HasAgency: false));

        Assert.Null(entity.Reservoir);
        Assert.Null(entity.Scope);
    }
}
