using FluentAssertions;
using RunicMagic.World;
using Xunit;

namespace RunicMagic.Tests;

public class TeleportEntityServiceTests
{
    [Fact]
    public void Teleport_UpdatesEntityPosition()
    {
        var entity = new Entity(EntityId.New(), EntityType.Object, "test") { X = 0, Y = 0 };
        var service = new TeleportEntityService();

        service.Teleport(entity, x: 400, y: 300);

        entity.X.Should().Be(400);
        entity.Y.Should().Be(300);
    }
}
