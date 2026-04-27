using RunicMagic.Controller.Models;

namespace RunicMagic.Controller.Abstractions;

public interface IWorldTickSink
{
    void Push(CommandResult result);
}
