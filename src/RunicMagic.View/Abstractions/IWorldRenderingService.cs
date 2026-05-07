using RunicMagic.View.Models;

namespace RunicMagic.View.Abstractions;

public interface IWorldRenderingService
{
    IReadOnlyList<EntityRenderingModel> GetAllRenderingModels(Guid? casterEntityId);
}
