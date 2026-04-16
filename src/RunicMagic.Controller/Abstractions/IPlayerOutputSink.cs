namespace RunicMagic.Controller.Abstractions;

// The world model's side of the player interface.
// Call these during RegisterInput processing to accumulate output for the current command.
public interface IPlayerOutputSink
{
    Task SendText(string text);
    Task SendWorldEntities();
}
