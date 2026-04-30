using FluentAssertions;
using RunicMagic.Controller.Models;
using RunicMagic.Controller.RuneParsing;
using RunicMagic.Controller.Services;
using RunicMagic.Tests.Builders;
using RunicMagic.World;
using RunicMagic.World.Execution;
using Xunit;

namespace RunicMagic.Tests;

public class EventDescriberTests
{
    private static readonly Entity _entity = new EntityBuilder().WithLabel("Wick").Build();

    // WorldEvent

    [Fact]
    public void Describe_EntityPushedEvent_IncludesLabelAndDistance()
    {
        var result = EventDescriber.Describe(new EntityPushedEvent(_entity, 300));

        result.Should().Be("Wick pushed 300mm.");
    }

    [Fact]
    public void Describe_EntityPulledEvent_IncludesLabelAndDistance()
    {
        var result = EventDescriber.Describe(new EntityPulledEvent(_entity, 150));

        result.Should().Be("Wick pulled 150mm.");
    }

    [Fact]
    public void Describe_PowerDrawnEvent_IncludesLabelAndAmount()
    {
        var result = EventDescriber.Describe(new PowerDrawnEvent(_entity, 42));

        result.Should().Be("Wick lost 42 power.");
    }

    [Fact]
    public void Describe_EntityDrainedEvent_IncludesLabel()
    {
        var result = EventDescriber.Describe(new EntityDrainedEvent(_entity));

        result.Should().Be("Wick was drained.");
    }

    [Fact]
    public void Describe_PowerFilledEvent_IncludesLabelAndAmount()
    {
        var result = EventDescriber.Describe(new PowerFilledEvent(_entity, 10));

        result.Should().Be("Wick gained 10 power.");
    }

    [Fact]
    public void Describe_EntityFullEvent_IncludesLabel()
    {
        var result = EventDescriber.Describe(new EntityFullEvent(_entity));

        result.Should().Be("Wick is full.");
    }

    [Fact]
    public void Describe_EffectNotFiredEvent_IncludesEffectAndReason()
    {
        var result = EventDescriber.Describe(new EffectNotFiredEvent("VUN", "no power"));

        result.Should().Be("Effect 'VUN' did not fire: no power.");
    }

    [Fact]
    public void Describe_EntityDisintegratedEvent_IncludesLabel()
    {
        var result = EventDescriber.Describe(new EntityDisintegratedEvent(_entity));

        result.Should().Be("Wick disintegrated.");
    }

    [Fact]
    public void Describe_SelectionCostNotMetEvent_IncludesRequiredAndDrawn()
    {
        var result = EventDescriber.Describe(new SelectionCostNotMetEvent(Required: 100, Drawn: 40));

        result.Should().Be("Selection failed: needed 100 power, drew 40.");
    }

    [Fact]
    public void Describe_InscriptionReadEvent_IncludesLabelAndText()
    {
        var result = EventDescriber.Describe(new InscriptionReadEvent(_entity, "ZU VUN"));

        result.Should().Be("Wick: ZU VUN");
    }

    [Fact]
    public void Describe_EntityRotatedEvent_IncludesLabelAndAngle()
    {
        var result = EventDescriber.Describe(new EntityRotatedEvent(_entity, 90));

        result.Should().Be("Wick rotated 90 degrees.");
    }

    [Fact]
    public void Describe_DebugOutputEvent_ReturnsText()
    {
        var result = EventDescriber.Describe(new DebugOutputEvent("raw debug text"));

        result.Should().Be("raw debug text");
    }

    // ControllerEvent

    [Fact]
    public void Describe_NoCasterSelectedEvent_ReturnsMessage()
    {
        var result = EventDescriber.Describe(new NoCasterSelectedEvent());

        result.Should().Be("No caster selected.");
    }

    [Fact]
    public void Describe_CasterNotFoundEvent_ReturnsMessage()
    {
        var result = EventDescriber.Describe(new CasterNotFoundEvent());

        result.Should().Be("Caster not found in world.");
    }

    [Fact]
    public void Describe_CasterDeadEvent_ReturnsMessage()
    {
        var result = EventDescriber.Describe(new CasterDeadEvent());

        result.Should().Be("Caster is dead.");
    }

    [Fact]
    public void Describe_NoEntitiesWithAgencyFoundEvent_ReturnsMessage()
    {
        var result = EventDescriber.Describe(new NoEntitiesWithAgencyFoundEvent());

        result.Should().Be("No entities with agency found at that position.");
    }

    [Fact]
    public void Describe_MultipleEntitiesWithAgencyFoundEvent_ReturnsMessage()
    {
        var result = EventDescriber.Describe(new MultipleEntitiesWithAgencyFoundEvent());

        result.Should().Be("Multiple entities with agency found at that position — unable to resolve a caster.");
    }

    [Fact]
    public void Describe_CasterSetEvent_IncludesLabel()
    {
        var result = EventDescriber.Describe(new CasterSetEvent(_entity));

        result.Should().Be("Caster set to Wick.");
    }

    [Fact]
    public void Describe_CasterMovedEvent_ReturnsMessage()
    {
        var result = EventDescriber.Describe(new CasterMovedEvent());

        result.Should().Be("Caster moved.");
    }

    [Fact]
    public void Describe_PointingDirectionSetEvent_ReturnsMessage()
    {
        var result = EventDescriber.Describe(new PointingDirectionSetEvent());

        result.Should().Be("Pointing direction set.");
    }

    [Fact]
    public void Describe_NothingToIndicateEvent_ReturnsMessage()
    {
        var result = EventDescriber.Describe(new NothingToIndicateEvent());

        result.Should().Be("Nothing to indicate at that position.");
    }

    [Fact]
    public void Describe_IndicatingEvent_IncludesLabel()
    {
        var result = EventDescriber.Describe(new IndicatingEvent(_entity));

        result.Should().Be("Indicating Wick.");
    }

    [Fact]
    public void Describe_IndicateTargetBlockedEvent_ReturnsMessage()
    {
        var result = EventDescriber.Describe(new IndicateTargetBlockedEvent());

        result.Should().Be("Cannot reach that — something is in the way.");
    }

    [Fact]
    public void Describe_IndicateTargetOutOfReachEvent_IncludesLabel()
    {
        var result = EventDescriber.Describe(new IndicateTargetOutOfReachEvent(_entity));

        result.Should().Be("Wick is out of reach.");
    }

    // ParseEvent

    [Fact]
    public void Describe_RanOutOfTokensEvent_ReturnsMessage()
    {
        var result = EventDescriber.Describe(new RanOutOfTokensEvent());

        result.Should().Be("Spell is incomplete — ran out of runes.");
    }

    [Fact]
    public void Describe_UnexpectedTokenEvent_IncludesTokenAndExpectedType()
    {
        var result = EventDescriber.Describe(new UnexpectedTokenEvent(Token: "VUN", ExpectedType: "Number"));

        result.Should().Be("Unexpected rune 'VUN' where Number was expected.");
    }
}
