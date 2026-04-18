using RunicMagic.Controller.RuneParsing.EffectRunes;
using RunicMagic.Controller.RuneParsing.EntityReferenceRunes;
using RunicMagic.Controller.RuneParsing.EntitySetRunes;
using RunicMagic.Controller.RuneParsing.ExecutionRunes;
using RunicMagic.Controller.RuneParsing.LocationRunes;
using RunicMagic.Controller.RuneParsing.NumberRunes;
using RunicMagic.Controller.RuneParsing.PowerSourceRunes;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Controller.RuneParsing
{
    internal static class ParserLookup
    {
        private static readonly Dictionary<string, IRuneParser<IExecutableStatement>> executableStatementRuneParsers = new();
        private static readonly Dictionary<string, IRuneParser<IStatement>> statementRuneParsers = new();
        private static readonly Dictionary<string, IRuneParser<IEntitySet>> entitySetRuneParsers = new();
        private static readonly Dictionary<string, IRuneParser<INumber>> numberRuneParsers = new();
        private static readonly Dictionary<string, IRuneParser<ILocation>> locationRuneParsers = new();

        static ParserLookup()
        {
            FillExecutableStatementRuneParsers();
            FillStatementRuneParsers();
            FillEntitySetRuneParsers();
            FillNumberRuneParsers();
            FillLocationRuneParsers();
        }

        public static void AddRuneParser<TRuneType>(string name, IRuneParser<TRuneType> parser)
        {
            switch (parser)
            {
                case IRuneParser<IExecutableStatement> executableStatementParser: executableStatementRuneParsers[name] = executableStatementParser; break;
                case IRuneParser<IStatement> statementParser: statementRuneParsers[name] = statementParser; break;
                case IRuneParser<IEntitySet> entitySetParser: entitySetRuneParsers[name] = entitySetParser; break;
                case IRuneParser<INumber> numberParser: numberRuneParsers[name] = numberParser; break;
                case IRuneParser<ILocation> locationParser: locationRuneParsers[name] = locationParser; break;
                default:
                    throw new InvalidOperationException($"Unsupported rune type: {typeof(TRuneType).Name}");
            }
        }

        public static IRuneParser<TRuneType>? FindRuneParserByName<TRuneType>(string name)
        {
            return typeof(TRuneType) switch
            {
                _ when typeof(TRuneType) == typeof(IExecutableStatement) => executableStatementRuneParsers.GetValueOrDefault(name) as IRuneParser<TRuneType>,
                _ when typeof(TRuneType) == typeof(IStatement) => statementRuneParsers.GetValueOrDefault(name) as IRuneParser<TRuneType>,
                _ when typeof(TRuneType) == typeof(IEntitySet) => entitySetRuneParsers.GetValueOrDefault(name) as IRuneParser<TRuneType>,
                _ when typeof(TRuneType) == typeof(INumber) => numberRuneParsers.GetValueOrDefault(name) as IRuneParser<TRuneType>,
                _ when typeof(TRuneType) == typeof(ILocation) => locationRuneParsers.GetValueOrDefault(name) as IRuneParser<TRuneType>,
                _ => null
            };
        }

        private static void FillExecutableStatementRuneParsers()
        {
            executableStatementRuneParsers["ZU"] = new ZUParser(); // execute
        }

        private static void FillStatementRuneParsers()
        {
            statementRuneParsers["VUN"] = new VUNParser(); // push
            statementRuneParsers["VAR"] = new VARParser(); // pull
            statementRuneParsers["SHU"] = new SHUParser(); // with power source
        }

        private static void FillEntitySetRuneParsers()
        {
            entitySetRuneParsers["A"] = new AParser(); // me, caster
            entitySetRuneParsers["OH"] = new OHParser(); // this, executor
            entitySetRuneParsers["LA"] = new LAParser(); // context of
            entitySetRuneParsers["DAN"] = new DANParser(); // pointing at
            entitySetRuneParsers["KAL"] = new KALParser(); // indicating
        }

        private static void FillNumberRuneParsers()
        {
            numberRuneParsers["HET"] = new HETParser(); // one
            numberRuneParsers["FOTIR"] = new FOTIRParser(); // times fourteen
        }

        private static void FillLocationRuneParsers()
        {
            locationRuneParsers["PAR"] = new PARParser(); // centroid of
            locationRuneParsers["GER"] = new GERParser(); // weighted centroid of
        }
    }
}
