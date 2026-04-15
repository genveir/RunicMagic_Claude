using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;

namespace RunicMagic.Controller.Services;

internal class PlayerService : IPlayerViewInterface, IPlayerOutputSink
{
    private readonly List<string> _pendingText = [];
    private readonly List<EntityRenderingModel> _pendingEntities = [];

    public string Prompt => ">";

    public async Task<CommandResult> RegisterInput(string input)
    {
        // Not yet wired to the spell processor — echo input back to confirm IO is working.
        await SendText($"you wrote: {input}");

        await SendEntity(new EntityRenderingModel(
            x: 100,
            y: 10,
            width: 50, height: 75,
            label: "a test entity",
            flags: EntityRenderingFlags.HasLife));
        await SendEntity(new EntityRenderingModel(
            x: -100,
            y: -80,
            width: 10, height: 20,
            label: "another test entity",
            flags: EntityRenderingFlags.HasLife | EntityRenderingFlags.HasAgency));

        var result = new CommandResult([.. _pendingText], [.. _pendingEntities]);
        _pendingText.Clear();
        _pendingEntities.Clear();
        return result;
    }

    public Task SendText(string text)
    {
        _pendingText.Add(text);
        return Task.CompletedTask;
    }

    public Task SendEntity(EntityRenderingModel entity)
    {
        _pendingEntities.Add(entity);
        return Task.CompletedTask;
    }
}
