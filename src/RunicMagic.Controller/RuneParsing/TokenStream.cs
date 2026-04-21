namespace RunicMagic.Controller.RuneParsing
{
    internal class TokenStream
    {
        private int index;
        private readonly List<string> tokens;

        private readonly List<List<string>> recordingWindows = [];

        public TokenStream(string spell)
        {
            tokens = spell.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            index = -1;
        }

        public void OpenRecordingWindow()
        {
            recordingWindows.Add([]);
        }

        public string[] CloseRecordingWindow()
        {
            if (recordingWindows.Count == 0)
            {
                throw new InvalidOperationException("No recording window to close.");
            }
            var currentWindow = recordingWindows.Last();
            recordingWindows.RemoveAt(recordingWindows.Count - 1);
            return currentWindow.ToArray();
        }

        public void InsertAtCursor(string[] toInsert)
        {
            var insertAt = Math.Min(index + 1, tokens.Count);
            for (int i = toInsert.Length - 1; i >= 0; i--)
            {
                tokens.Insert(insertAt, toInsert[i]);
            }
            index = insertAt - 1;
        }

        public void Backtrack()
        {
            foreach (var window in recordingWindows)
            {
                if (window.Count > 0)
                {
                    window.RemoveAt(window.Count - 1);
                }
            }

            index--;
        }

        public string? Next()
        {
            index++;
            if (index >= tokens.Count)
            {
                index = tokens.Count;
                return null;
            }

            foreach (var window in recordingWindows)
            {
                window.Add(tokens[index]);
            }

            return tokens[index];
        }

        public int Index => index;
    }
}
