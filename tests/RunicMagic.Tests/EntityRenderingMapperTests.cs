using FluentAssertions;
using RunicMagic.Controller.Mappers;
using RunicMagic.Controller.Models;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Geometry;
using Xunit;

namespace RunicMagic.Tests;

public class EntityRenderingMapperTests
{
    private static Entity MakeEntity(EntityType type = EntityType.Object, bool hasAgency = false,
        LifeCapability? life = null, ChargeCapability? charge = null) =>
        new(EntityId.New(), type, "test")
        {
            Location = new Location(10, 20),
            Width = 30,
            Height = 40,
            HasAgency = hasAgency,
            Life = life,
            Charge = charge,
        };

    [Fact]
    public void Maps_LocationAndLabel()
    {
        var entity = new Entity(EntityId.New(), EntityType.Object, "rock")
        {
            Location = new Location(5, 15),
            Width = 25,
            Height = 35,
        };

        var model = EntityRenderingMapper.ToRenderingModel(entity, isCaster: false);

        model.X.Should().Be(5);
        model.Y.Should().Be(15);
        model.Width.Should().Be(25);
        model.Height.Should().Be(35);
        model.Label.Should().Be("rock");
    }

    [Fact]
    public void Object_WithNoCapabilities_HasNoFlags()
    {
        var model = EntityRenderingMapper.ToRenderingModel(MakeEntity(), isCaster: false);

        model.Flags.Should().Be(EntityRenderingFlags.None);
    }

    [Fact]
    public void Entity_WithLife_HasLifeFlag()
    {
        var model = EntityRenderingMapper.ToRenderingModel(
            MakeEntity(life: new LifeCapability(100, 100)), isCaster: false);

        model.Flags.Should().HaveFlag(EntityRenderingFlags.HasLife);
        model.Flags.Should().NotHaveFlag(EntityRenderingFlags.HasAgency);
    }

    [Fact]
    public void Entity_WithAgency_HasAgencyFlag()
    {
        var model = EntityRenderingMapper.ToRenderingModel(MakeEntity(hasAgency: true), isCaster: false);

        model.Flags.Should().HaveFlag(EntityRenderingFlags.HasAgency);
        model.Flags.Should().NotHaveFlag(EntityRenderingFlags.HasLife);
    }

    [Fact]
    public void Entity_MarkedAsCaster_HasIsCasterTrue()
    {
        var model = EntityRenderingMapper.ToRenderingModel(MakeEntity(), isCaster: true);

        model.IsCaster.Should().BeTrue();
    }

    [Fact]
    public void Entity_NotMarkedAsCaster_HasIsCasterFalse()
    {
        var model = EntityRenderingMapper.ToRenderingModel(MakeEntity(), isCaster: false);

        model.IsCaster.Should().BeFalse();
    }

    [Fact]
    public void Entity_WithPointingEnd_IncludesEndpointCoordinates()
    {
        var model = EntityRenderingMapper.ToRenderingModel(MakeEntity(), isCaster: false, pointingEnd: new WorldCoordinate(100, 200));

        model.PointingEndX.Should().Be(100);
        model.PointingEndY.Should().Be(200);
    }

    [Fact]
    public void Entity_WithoutPointingEnd_HasNullEndpoint()
    {
        var model = EntityRenderingMapper.ToRenderingModel(MakeEntity(), isCaster: false, pointingEnd: null);

        model.PointingEndX.Should().BeNull();
        model.PointingEndY.Should().BeNull();
    }

    [Fact]
    public void Entity_WithLifeAndAgency_HasBothFlags()
    {
        var model = EntityRenderingMapper.ToRenderingModel(
            MakeEntity(hasAgency: true, life: new LifeCapability(100, 100)), isCaster: false);

        model.Flags.Should().HaveFlag(EntityRenderingFlags.HasLife);
        model.Flags.Should().HaveFlag(EntityRenderingFlags.HasAgency);
    }

    [Fact]
    public void Entity_WhenTranslucent_HasTranslucentFlag()
    {
        var entity = MakeEntity();
        entity.IsTranslucent = true;

        var model = EntityRenderingMapper.ToRenderingModel(entity, isCaster: false);

        model.Flags.Should().HaveFlag(EntityRenderingFlags.IsTranslucent);
    }

    [Fact]
    public void Entity_WhenNotTranslucent_DoesNotHaveTranslucentFlag()
    {
        var model = EntityRenderingMapper.ToRenderingModel(MakeEntity(), isCaster: false);

        model.Flags.Should().NotHaveFlag(EntityRenderingFlags.IsTranslucent);
    }
}
