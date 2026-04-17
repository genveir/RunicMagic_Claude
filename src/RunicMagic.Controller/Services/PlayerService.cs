using RunicMagic.Controller.Abstractions;
using RunicMagic.Controller.Models;

namespace RunicMagic.Controller.Services;

internal class PlayerService(WorldRenderingService worldRendering, SpellCastingService spellCasting) : IPlayerViewInterface, IPlayerOutputSink
{
    private readonly List<string> _pendingText = [];
    private readonly List<EntityRenderingModel> _pendingEntities = [];

    public string Prompt => ">";

    public async Task<CommandResult> RegisterInput(string input)
    {
        var responseLines = spellCasting.Cast(input);
        foreach (var line in responseLines)
        {
            await SendText(line);
        }

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
