using RunicMagic.World.Geometry;
using RunicMagic.World.Motion.Shared;

namespace RunicMagic.Tests.World.Motion.Shared;

public class TeleportEntityServiceTests
{
    [Fact]
    public void Teleport_UpdatesEntityPosition()
    {
        var entity = new EntityBuilder().WithLocation(0, 0).Build();

        TeleportEntityService.Teleport(entity, new Location(400, 300));

        entity.Location.X.Should().Be(400);
        entity.Location.Y.Should().Be(300);
    }
}
