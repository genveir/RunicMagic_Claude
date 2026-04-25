using FluentAssertions;
using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using Xunit;

namespace RunicMagic.Tests;

public class SpellCastingServiceTests
{
    private static SpellCastingService MakeService(WorldModel world)
    {
        return new SpellCastingService(world, new SpellExecutor(world));
    }

    private static Entity AddCaster(WorldModel world)
    {
        var caster = new EntityBuilder().WithLocation(x: 0, y: 0).WithWeight(1).Build();
        world.Add(caster);
        return caster;
    }

    [Fact]
    public void Cast_EmptyInput_ReturnsRanOutOfTokensMessage()
    {
        var world = new WorldModel();
        var caster = AddCaster(world);
        var service = MakeService(world);

        var lines = service.Cast("", casterId: caster.Id);

        lines.Should().ContainSingle().Which.Should().Contain("ran out of runes");
    }

    [Fact]
    public void Cast_UnrecognisedRune_ReturnsUnexpectedTokenMessage()
    {
        var world = new WorldModel();
        var caster = AddCaster(world);
        var service = MakeService(world);

        var lines = service.Cast("NOTARUNE", casterId: caster.Id);

        lines.Should().ContainSingle().Which.Should().Contain("NOTARUNE");
    }

    [Fact]
    public void Cast_IncompleteSpell_ReturnsRanOutOfTokensMessage()
    {
        var world = new WorldModel();
        var caster = AddCaster(world);
        var service = MakeService(world);

        // ZU VUN A — missing Number argument for VUN
        var lines = service.Cast("ZU VUN A", casterId: caster.Id);

        lines.Should().ContainSingle().Which.Should().Contain("ran out of runes");
    }

    [Fact]
    public void Cast_ValidSpell_NoCasterSelected_ReturnsNoCasterSelectedMessage()
    {
        var world = new WorldModel();
        var service = MakeService(world);

        var lines = service.Cast("ZU VUN LA IR HOT IR HOT HOT", casterId: null);

        lines.Should().ContainSingle().Which.Should().Be("No caster selected.");
    }

    [Fact]
    public void Cast_ValidSpell_CasterIdNotInWorld_ReturnsCasterNotFoundMessage()
    {
        var world = new WorldModel();
        var service = MakeService(world);

        var lines = service.Cast("ZU VUN LA IR HOT IR HOT HOT", casterId: EntityId.New());

        lines.Should().ContainSingle().Which.Should().Be("Caster not found in world.");
    }

    [Fact]
    public void Cast_MilestoneSpell_ReturnsPushedEvent()
    {
        var world = new WorldModel();

        var casterEntity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithWeight(1)
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        casterEntity.Label = "Caster";

        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        target.Label = "target";
        casterEntity.Scope = () => [target];

        world.Add(casterEntity);
        world.Add(target);

        var service = MakeService(world);

        var lines = service.Cast("ZU VUN LA IR HOT IR HOT HOT", casterId: casterEntity.Id);

        lines.Should().Contain(l => l.Contains("target") && l.Contains("pushed"));
    }

    [Fact]
    public void Cast_MilestoneSpell_ReturnsPowerDrawnEvent()
    {
        var world = new WorldModel();

        var casterEntity = new EntityBuilder()
            .WithLocation(x: 0, y: 0)
            .WithWeight(1)
            .WithReservoir(draw: amount => new ReservoirDraw(amount, false))
            .Build();
        casterEntity.Label = "Caster";

        var target = new EntityBuilder().WithLocation(x: 1000, y: 0).WithWeight(1).Build();
        target.Label = "target";
        casterEntity.Scope = () => [target];

        world.Add(casterEntity);
        world.Add(target);

        var service = MakeService(world);

        var lines = service.Cast("ZU VUN LA IR HOT IR HOT HOT", casterId: casterEntity.Id);

        lines.Should().Contain(l => l.Contains("Caster") && l.Contains("lost") && l.Contains("power"));
    }
}
