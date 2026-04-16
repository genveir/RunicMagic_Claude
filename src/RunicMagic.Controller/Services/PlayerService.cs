using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;

namespace RunicMagic.Controller.Services;

internal class PlayerService(WorldRenderingService worldRendering) : IPlayerViewInterface, IPlayerOutputSink
{
    private readonly List<string> _pendingText = [];
    private readonly List<EntityRenderingModel> _pendingEntities = [];

    public string Prompt => ">";

    public async Task<CommandResult> RegisterInput(string input)
    {
        // Not yet wired to the spell processor — echo input back to confirm IO is working.
        await SendText($"you wrote: {input}");
        await SendWorldEntities();

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

    public Task SendWorldEntities()
    {
        _pendingEntities.AddRange(worldRendering.GetAllRenderingModels());
        return Task.CompletedTask;
    }
}
