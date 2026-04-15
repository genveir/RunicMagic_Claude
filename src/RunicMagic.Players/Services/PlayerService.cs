using RunicMagic.Players.Abstractions;
using RunicMagic.Players.Models;

namespace RunicMagic.Players.Services
{
    internal class PlayerService : IPlayerViewInterface
    {
        public string Prompt => ">";

        public async Task RegisterInput(string input)
        {
            // we're not yet handling input, just echo it back to the player to show that the system is working.
            await SendTextOutput($"you wrote: {input}");
            await FlushOutput();
        }

        public void SetDataHandlers(Func<string, Task> onTextDataAvailable, Func<EntityRenderingModel, Task> onRenderingDataAvailable, Func<Task> onFlush)
        {
            _onTextDataAvailable = onTextDataAvailable;
            _onRenderingDataAvailable = onRenderingDataAvailable;
            _onFlush = onFlush;
        }

        public async Task SendTextOutput(string text)
        {
            if (_onTextDataAvailable != null)
            {
                await _onTextDataAvailable(text);
            }
        }

        // may not want to use the EntityRenderingModel here, but map to it from the entity when we make it.
        public async Task SendRenderingOutput(EntityRenderingModel renderingData)
        {
            if (_onRenderingDataAvailable != null)
            {
                await _onRenderingDataAvailable(renderingData);
            }
        }

        public async Task FlushOutput()
        {
            if (_onFlush != null)
            {
                await _onFlush();
            }
        }


        private Func<string, Task>? _onTextDataAvailable;
        private Func<EntityRenderingModel, Task>? _onRenderingDataAvailable;
        private Func<Task>? _onFlush;
    }
}
