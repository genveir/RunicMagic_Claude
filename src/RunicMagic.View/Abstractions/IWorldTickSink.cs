using RunicMagic.View.Models;

namespace RunicMagic.View.Abstractions;

public interface IWorldTickSink
{
    void Push(CommandResult result);
}
