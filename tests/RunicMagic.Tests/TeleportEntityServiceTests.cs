using FluentAssertions;
using RunicMagic.World;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests;

public class TeleportEntityServiceTests
{
    [Fact]
    public void Teleport_UpdatesEntityPosition()
    {
        var entity = new Entity(EntityId.New(), "test") { Location = new Location(0, 0) };
        var service = new TeleportEntityService();

        service.Teleport(entity, new Location(400, 300));

        entity.Location.X.Should().Be(400);
        entity.Location.Y.Should().Be(300);
    }
}
