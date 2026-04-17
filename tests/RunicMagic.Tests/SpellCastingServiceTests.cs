using FluentAssertions;
using RunicMagic.Controller.Services;
using RunicMagic.Tests.Execution;
using RunicMagic.World;
using RunicMagic.World.Execution;
using Xunit;

namespace RunicMagic.Tests;

public class SpellCastingServiceTests
{
    private static SpellCastingService MakeService(WorldModel world)
    {
        return new SpellCastingService(world, new SpellExecutor(world));
    }

    [Fact]
    public void Cast_EmptyInput_ReturnsRanOutOfTokensMessage()
    {
        var world = new WorldModel();
        var service = MakeService(world);

        var lines = service.Cast("");

        lines.Should().ContainSingle().Which.Should().Contain("ran out of runes");
    }

    [Fact]
    public void Cast_UnrecognisedRune_ReturnsUnexpectedTokenMessage()
    {
        var world = new WorldModel();
        var service = MakeService(world);

        var lines = service.Cast("NOTARUNE");

        lines.Should().ContainSingle().Which.Should().Contain("NOTARUNE");
    }

    [Fact]
    public void Cast_IncompleteSpell_ReturnsRanOutOfTokensMessage()
    {
        var world = new WorldModel();
        var service = MakeService(world);

        // ZU VUN A — missing Number argument for VUN
        var lines = service.Cast("ZU VUN A");

        lines.Should().ContainSingle().Which.Should().Contain("ran out of runes");
    }

    [Fact]
    public void Cast_ValidSpell_NoCasterInWorld_ReturnsNoCasterMessage()
    {
        var world = new WorldModel();
        var service = MakeService(world);

        var lines = service.Cast("ZU VUN LA FOTIR FOTIR FOTIR HET");

        lines.Should().ContainSingle().Which.Should().Contain("No caster");
    }

    [Fact]
    public void Cast_MilestoneSpell_ReturnsPushedEvent()
    {
        var world = new WorldModel();

        var casterEntity = TestFixtures.MakeEntity(x: 0, y: 0, weight: 1);
        casterEntity.Label = "Caster";
        casterEntity.Reservoir = amount => new ReservoirDraw(amount, false);

        var target = TestFixtures.MakeEntity(x: 1000, y: 0, weight: 1);
        target.Label = "target";
        casterEntity.Scope = () => [target];

        world.Add(casterEntity);
        world.Add(target);

        var service = MakeService(world);

        var lines = service.Cast("ZU VUN LA FOTIR FOTIR FOTIR HET");

        lines.Should().Contain(l => l.Contains("target") && l.Contains("pushed"));
    }

    [Fact]
    public void Cast_MilestoneSpell_ReturnsPowerDrawnEvent()
    {
        var world = new WorldModel();

        var casterEntity = TestFixtures.MakeEntity(x: 0, y: 0, weight: 1);
        casterEntity.Label = "Caster";
        casterEntity.Reservoir = amount => new ReservoirDraw(amount, false);

        var target = TestFixtures.MakeEntity(x: 1000, y: 0, weight: 1);
        target.Label = "target";
        casterEntity.Scope = () => [target];

        world.Add(casterEntity);
        world.Add(target);

        var service = MakeService(world);

        var lines = service.Cast("ZU VUN LA FOTIR FOTIR FOTIR HET");

        lines.Should().Contain(l => l.Contains("Caster") && l.Contains("lost") && l.Contains("power"));
    }
}
