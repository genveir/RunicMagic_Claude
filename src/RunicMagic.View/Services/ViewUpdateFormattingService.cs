using RunicMagic.View.Abstractions;
using RunicMagic.View.Models;
using System.Text;

namespace RunicMagic.View.Services;

public class ViewUpdateFormattingService : IWorldTickSink
{
    private readonly SseConnectionManager sseConnectionManager;

    public ViewUpdateFormattingService(SseConnectionManager sseConnectionManager)
    {
        this.sseConnectionManager = sseConnectionManager;
    }

    public void Push(TickResult result)
    {
        var prompt = FormatPrompt(result.CasterData);
        var bars = FormatBars(result.CasterData);
        var viewUpdate = new ViewUpdateModel(result.Text, result.Entities, prompt, bars);
        sseConnectionManager.Push(viewUpdate);
    }

    private static CasterBarsModel? FormatBars(CasterDataModel? casterData)
    {
        if (casterData == null)
            return null;

        var bars = new CasterBarsModel(
            casterData.CurrentHitPoints,
            casterData.MaxHitPoints,
            casterData.CurrentPower,
            casterData.MaxPower);
        return bars;
    }

    private static string FormatPrompt(CasterDataModel? casterData)
    {
        if (casterData == null)
            return "[no caster] >";

        var sb = new StringBuilder();

        if (casterData.CurrentHitPoints != null)
        {
            if (casterData.CurrentHitPoints == 0)
            {
                return "[dead caster] >";
            }

            sb.Append($"({casterData.CurrentHitPoints}/{casterData.MaxHitPoints}H) ");
        }

        sb.Append($"({casterData.CurrentIntegrity}/{casterData.MaxIntegrity}I) ");

        if (casterData.CurrentPower != null)
            sb.Append($"({casterData.CurrentPower}/{casterData.MaxPower}P) ");

        sb.Append('>');

        var prompt = sb.ToString();
        return prompt;
    }
}
