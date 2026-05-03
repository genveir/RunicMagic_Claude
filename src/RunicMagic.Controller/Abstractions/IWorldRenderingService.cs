using RunicMagic.Controller.Models;
using RunicMagic.World.Entities;

namespace RunicMagic.Controller.Abstractions;

public interface IWorldRenderingService
{
    IReadOnlyList<EntityRenderingModel> GetAllRenderingModels(EntityId? casterEntityId);
}