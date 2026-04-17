namespace RunicMagic.Controller.RuneParsing
{
    internal class TokenStream
    {
        private int index;
        private readonly List<string> tokens;

        public TokenStream(string spell)
        {
            tokens = spell.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            index = -1;
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

            return tokens[index];
        }

        public int Index => index;
    }
}
