using FluentAssertions;
using RunicMagic.Tests.Builders;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EntityReferenceRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EntityReferenceRunes;

public class OHTests
{
    [Fact]
    public void Resolve_ReturnsExecutor()
    {
        var executorEntity = new EntityBuilder().Build();
        var executor = new EntitySet([executorEntity]);
        var context = TestFixtures.MakeContext(executor: executor);

        var result = new OH().Resolve(context);

        result.Should().BeSameAs(executor);
    }

    [Fact]
    public void Resolve_WindowOpen_AddsExecutorIdToResolutionCount()
    {
        var executorEntity = new EntityBuilder().Build();
        var context = TestFixtures.MakeContext(executor: new EntitySet([executorEntity]));
        context.OpenResolutionWindow();

        new OH().Resolve(context);

        context.EntityResolutionCount.Should().Contain(executorEntity.Id);
    }

    [Fact]
    public void Resolve_WindowNull_DoesNotThrow()
    {
        var executorEntity = new EntityBuilder().Build();
        var context = TestFixtures.MakeContext(executor: new EntitySet([executorEntity]));

        var act = () => new OH().Resolve(context);

        act.Should().NotThrow();
    }
}
