using FluentAssertions;
using RunicMagic.World.Execution;
using RunicMagic.World.Runes.EntityReferenceRunes;
using Xunit;

namespace RunicMagic.Tests.Execution.EntityReferenceRunes;

public class OHTests
{
    [Fact]
    public void Resolve_ReturnsExecutor()
    {
        var executorEntity = TestFixtures.MakeEntity();
        var executor = new EntitySet([executorEntity]);
        var context = TestFixtures.MakeContext(executor: executor);

        var result = new OH().Resolve(context);

        result.Should().BeSameAs(executor);
    }
}
