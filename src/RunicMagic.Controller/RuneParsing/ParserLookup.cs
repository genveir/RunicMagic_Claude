using RunicMagic.Controller.RuneParsing.ArithmeticRunes;
using RunicMagic.Controller.RuneParsing.EffectRunes;
using RunicMagic.Controller.RuneParsing.EntityReferenceRunes;
using RunicMagic.Controller.RuneParsing.EntitySetRunes;
using RunicMagic.Controller.RuneParsing.ExecutionRunes;
using RunicMagic.Controller.RuneParsing.FilterRunes;
using RunicMagic.Controller.RuneParsing.InvocationRunes;
using RunicMagic.Controller.RuneParsing.LocationRunes;
using RunicMagic.Controller.RuneParsing.NumberRunes;
using RunicMagic.Controller.RuneParsing.PowerSourceRunes;
using RunicMagic.Controller.RuneParsing.SetOperationRunes;
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
            FillSetOperationRuneParsers();
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
            statementRuneParsers["GWYAH"] = new GWYAHParser(); // invoke
            statementRuneParsers["CRIYR"] = new CRIYRParser(); // read inscription
        }

        private static void FillEntitySetRuneParsers()
        {
            entitySetRuneParsers["A"] = new AParser(); // me, caster
            entitySetRuneParsers["OH"] = new OHParser(); // this, executor
            entitySetRuneParsers["LA"] = new LAParser(); // scope of
            entitySetRuneParsers["PA"] = new PAParser(); // intersection of scopes
            entitySetRuneParsers["DAN"] = new DANParser(); // pointing at
            entitySetRuneParsers["KAL"] = new KALParser(); // indicating
            entitySetRuneParsers["HORO"] = new HOROParser(); // near
            entitySetRuneParsers["ZYIL"] = new ZYILParser(); // weight range filter
            entitySetRuneParsers["ZYHE"] = new ZYHEParser(); // lightest
            entitySetRuneParsers["ZYSE"] = new ZYSEParser(); // heaviest
            entitySetRuneParsers["FUIL"] = new FUILParser(); // power range filter
            entitySetRuneParsers["FUHE"] = new FUHEParser(); // least powerful
            entitySetRuneParsers["FUSE"] = new FUSEParser(); // most powerful
            entitySetRuneParsers["HORIL"] = new HORILParser(); // distance range filter
            entitySetRuneParsers["HORHE"] = new HORHEParser(); // closest
            entitySetRuneParsers["HORSE"] = new HORSEParser(); // farthest
            entitySetRuneParsers["DRYAL"] = new DRYALParser(); // is alive
        }

        private static void FillSetOperationRuneParsers()
        {
            entitySetRuneParsers["AN"] = new ANParser(); // union
            entitySetRuneParsers["DU"] = new DUParser(); // intersection
            entitySetRuneParsers["RAL"] = new RALParser(); // difference
        }

        private static void FillNumberRuneParsers()
        {
            numberRuneParsers["JON"] = new JONParser(); // zero
            numberRuneParsers["HET"] = new HETParser(); // one
            numberRuneParsers["DET"] = new DETParser(); // two
            numberRuneParsers["TET"] = new TETParser(); // three
            numberRuneParsers["FET"] = new FETParser(); // five
            numberRuneParsers["SET"] = new SETParser(); // seven
            numberRuneParsers["HOT"] = new HOTParser(); // 14^1
            numberRuneParsers["DOT"] = new DOTParser(); // 14^2
            numberRuneParsers["TOT"] = new TOTParser(); // 14^3
            numberRuneParsers["FOT"] = new FOTParser(); // 14^5
            numberRuneParsers["SOT"] = new SOTParser(); // 14^7
            numberRuneParsers["IR"] = new IRParser(); // multiply
            numberRuneParsers["MO"] = new MOParser(); // add
            numberRuneParsers["UIT"] = new UITParser(); // modulo
            numberRuneParsers["EID"] = new EIDParser(); // integer divide
            numberRuneParsers["DEID"] = new DEIDParser(); // halve
            numberRuneParsers["MOST"] = new MOSTParser(); // one and a half
        }

        private static void FillLocationRuneParsers()
        {
            locationRuneParsers["PAR"] = new PARParser(); // centroid of
            locationRuneParsers["GER"] = new GERParser(); // weighted centroid of
        }
    }
}
