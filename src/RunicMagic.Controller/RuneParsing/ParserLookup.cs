using RunicMagic.Controller.RuneParsing.EffectRunes;
using RunicMagic.Controller.RuneParsing.EntityReferenceRunes;
using RunicMagic.Controller.RuneParsing.EntitySetRunes;
using RunicMagic.Controller.RuneParsing.ExecutionRunes;
using RunicMagic.Controller.RuneParsing.LocationRunes;
using RunicMagic.Controller.RuneParsing.NumberRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing
{
    internal static class ParserLookup
    {
        public static IRuneParser<TRuneType>? FindRuneParserByName<TRuneType>(string name)
        {
            return typeof(TRuneType) switch
            {
                _ when typeof(TRuneType) == typeof(IExecutableStatement) => FindExecutableStatementRuneParserByName(name) as IRuneParser<TRuneType>,
                _ when typeof(TRuneType) == typeof(IStatement) => FindStatementRuneParserByName(name) as IRuneParser<TRuneType>,
                _ when typeof(TRuneType) == typeof(IEntitySet) => FindEntitySetRuneParserByName(name) as IRuneParser<TRuneType>,
                _ when typeof(TRuneType) == typeof(INumber) => FindNumberRuneParserByName(name) as IRuneParser<TRuneType>,
                _ when typeof(TRuneType) == typeof(ILocation) => FindLocationRuneParserByName(name) as IRuneParser<TRuneType>,
                _ => null
            };
        }

        private static IRuneParser<IExecutableStatement>? FindExecutableStatementRuneParserByName(string name)
        {
            return name switch
            {
                "ZU" => new ZUParser(),
                _ => null
            };
        }

        private static IRuneParser<IStatement>? FindStatementRuneParserByName(string name)
        {
            return name switch
            {
                "VUN" => new VUNParser(),
                _ => null
            };
        }

        private static IRuneParser<IEntitySet>? FindEntitySetRuneParserByName(string name)
        {
            return name switch
            {
                "A" => new AParser(),
                "OH" => new OHParser(),
                "LA" => new LAParser(),
                _ => null
            };
        }

        private static IRuneParser<INumber>? FindNumberRuneParserByName(string name)
        {
            return name switch
            {
                "HET" => new HETParser(),
                "FOTIR" => new FOTIRParser(),
                _ => null
            };
        }

        private static IRuneParser<ILocation>? FindLocationRuneParserByName(string name)
        {
            return name switch
            {
                "PAR" => new PARParser(),
                _ => null
            };

        }
    }
}
