using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.InvocationRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.InvocationRunes;

public class CRIYRTests
{
    // ── No inscriptions ───────────────────────────────────────────────────────

    [Fact]
    public void Execute_EntityWithNoInscriptions_EmitsNoEvents()
    {
        var target = new EntityBuilder().Build();
        var context = TestFixtures.MakeContext();
        var criyr = new CRIYR(target: new FixedEntitySet(target));

        criyr.Execute(context);

        context.Result.Events.Should().BeEmpty();
    }

    // ── Single inscription ────────────────────────────────────────────────────

    [Fact]
    public void Execute_EntityWithOneInscription_EmitsOneEvent()
    {
        var target = new EntityBuilder().Build();
        target.RawInscriptions = ["ZU VUN LA TOT"];
        var context = TestFixtures.MakeContext();
        var criyr = new CRIYR(target: new FixedEntitySet(target));

        criyr.Execute(context);

        var @event = context.Result.Events.Should().ContainSingle().Which
            .Should().BeOfType<InscriptionReadEvent>().Subject;
        @event.Entity.Should().BeSameAs(target);
        @event.Text.Should().Be("ZU VUN LA TOT");
    }

    // ── Multiple inscriptions on one entity ───────────────────────────────────

    [Fact]
    public void Execute_MultipleInscriptionsOnOneEntity_EmitsOneEventPerInscription()
    {
        var target = new EntityBuilder().Build();
        target.RawInscriptions = ["ZU VUN LA TOT", "ZU GWYAH OH"];
        var context = TestFixtures.MakeContext();
        var criyr = new CRIYR(target: new FixedEntitySet(target));

        criyr.Execute(context);

        var events = context.Result.Events.Should().HaveCount(2).And
            .AllBeOfType<InscriptionReadEvent>().Subject.ToList();
        events[0].Text.Should().Be("ZU VUN LA TOT");
        events[1].Text.Should().Be("ZU GWYAH OH");
    }

    // ── Multiple entities ─────────────────────────────────────────────────────

    [Fact]
    public void Execute_MultipleEntities_EmitsEventsEntityFirst()
    {
        var first = new EntityBuilder().Build();
        first.RawInscriptions = ["ZU VUN LA TOT", "ZU GWYAH OH"];

        var second = new EntityBuilder().Build();
        second.RawInscriptions = ["ZU VAR DAN HET"];

        var context = TestFixtures.MakeContext();
        var criyr = new CRIYR(target: new FixedEntitySet(first, second));

        criyr.Execute(context);

        var events = context.Result.Events.Cast<InscriptionReadEvent>().ToList();
        events.Should().HaveCount(3);
        events[0].Entity.Should().BeSameAs(first);
        events[0].Text.Should().Be("ZU VUN LA TOT");
        events[1].Entity.Should().BeSameAs(first);
        events[1].Text.Should().Be("ZU GWYAH OH");
        events[2].Entity.Should().BeSameAs(second);
        events[2].Text.Should().Be("ZU VAR DAN HET");
    }

    // ── Mixed entities ────────────────────────────────────────────────────────

    [Fact]
    public void Execute_SomeEntitiesHaveNoInscriptions_OnlyInscribedEntitiesEmitEvents()
    {
        var withInscription = new EntityBuilder().Build();
        withInscription.RawInscriptions = ["ZU VUN LA TOT"];

        var withoutInscription = new EntityBuilder().Build();

        var context = TestFixtures.MakeContext();
        var criyr = new CRIYR(target: new FixedEntitySet(withInscription, withoutInscription));

        criyr.Execute(context);

        context.Result.Events.Should().ContainSingle()
            .Which.Should().BeOfType<InscriptionReadEvent>()
            .Which.Entity.Should().BeSameAs(withInscription);
    }
}
