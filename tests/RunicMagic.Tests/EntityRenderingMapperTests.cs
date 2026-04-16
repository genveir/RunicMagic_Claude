using FluentAssertions;
using RunicMagic.Controller.Mappers;
using RunicMagic.Controller.Models;
using RunicMagic.World;
using RunicMagic.World.Capabilities;
using Xunit;

namespace RunicMagic.Tests;

public class EntityRenderingMapperTests
{
    private static Entity MakeEntity(EntityType type = EntityType.Object, bool hasAgency = false,
        LifeCapability? life = null, ChargeCapability? charge = null) =>
        new(EntityId.New(), type, "test")
        {
            X = 10,
            Y = 20,
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
            X = 5,
            Y = 15,
            Width = 25,
            Height = 35,
        };

        var model = EntityRenderingMapper.ToRenderingModel(entity);

        model.X.Should().Be(5);
        model.Y.Should().Be(15);
        model.Width.Should().Be(25);
        model.Height.Should().Be(35);
        model.Label.Should().Be("rock");
    }

    [Fact]
    public void Object_WithNoCapabilities_HasNoFlags()
    {
        var model = EntityRenderingMapper.ToRenderingModel(MakeEntity());

        model.Flags.Should().Be(EntityRenderingFlags.None);
    }

    [Fact]
    public void Entity_WithLife_HasLifeFlag()
    {
        var model = EntityRenderingMapper.ToRenderingModel(
            MakeEntity(life: new LifeCapability(100, 100)));

        model.Flags.Should().HaveFlag(EntityRenderingFlags.HasLife);
        model.Flags.Should().NotHaveFlag(EntityRenderingFlags.HasAgency);
    }

    [Fact]
    public void Entity_WithAgency_HasAgencyFlag()
    {
        var model = EntityRenderingMapper.ToRenderingModel(MakeEntity(hasAgency: true));

        model.Flags.Should().HaveFlag(EntityRenderingFlags.HasAgency);
        model.Flags.Should().NotHaveFlag(EntityRenderingFlags.HasLife);
    }

    [Fact]
    public void Entity_WithLifeAndAgency_HasBothFlags()
    {
        var model = EntityRenderingMapper.ToRenderingModel(
            MakeEntity(hasAgency: true, life: new LifeCapability(100, 100)));

        model.Flags.Should().HaveFlag(EntityRenderingFlags.HasLife);
        model.Flags.Should().HaveFlag(EntityRenderingFlags.HasAgency);
    }
}
