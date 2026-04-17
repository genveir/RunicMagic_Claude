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
            for (int i = toInsert.Length - 1; i >= 0; i--)
            {
                tokens.Insert(index, toInsert[i]);
            }
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
