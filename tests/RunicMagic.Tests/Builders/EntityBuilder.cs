using RunicMagic.World;
using RunicMagic.World.Capabilities;
using RunicMagic.World.Execution;
using RunicMagic.World.Geometry;
using RunicMagic.World.Runes.RuneTypes;

namespace RunicMagic.Tests.Builders
{
    internal class EntityBuilder
    {
        private EntityId _id = new EntityId(Guid.NewGuid());
        private string _label = "Test Entity";
        private Location _location = new Location(0, 0);
        private long _width = 100;
        private long _height = 100;
        private bool _hasAgency = false;
        private long _weight = 1000;
        private bool _isTranslucent = false;
        private double _angle = 0;
        private StructuralIntegrityCapability _structuralIntegrity = new StructuralIntegrityCapability(1000, 1000);

        private LifeCapability? _life;
        private ChargeCapability? _charge;
        private Func<Entity[]>? _scope;
        private ReservoirCapability? _reservoir;
        private Direction? _pointingDirection;
        private IndicateTarget? _indicateTarget;
        private string[] _rawInscriptions = [];
        private IStatement[] _parsedInscriptions = [];

        public EntityBuilder() { }

        public EntityBuilder WithId(EntityId id)
        {
            _id = id;
            return this;
        }

        public EntityBuilder WithLabel(string label)
        {
            _label = label;
            return this;
        }

        public EntityBuilder WithLocation(long x, long y)
        {
            _location = new Location(x, y);
            return this;
        }

        public EntityBuilder WithLocation(Location location)
        {
            _location = location;
            return this;
        }

        public EntityBuilder WithSize(long width, long height)
        {
            _width = width;
            _height = height;
            return this;
        }

        public EntityBuilder WithWeight(long weight)
        {
            _weight = weight;
            return this;
        }

        public EntityBuilder WithAgency()
        {
            _hasAgency = true;
            return this;
        }

        public EntityBuilder WithTranslucency()
        {
            _isTranslucent = true;
            return this;
        }

        public EntityBuilder WithAngle(double angle)
        {
            _angle = angle;
            return this;
        }

        public EntityBuilder WithStructuralIntegrity(long max, long current)
        {
            _structuralIntegrity = new StructuralIntegrityCapability(max, current);
            return this;
        }

        public EntityBuilder WithLife(long max, long current)
        {
            _life = new LifeCapability(max, current);
            return this;
        }

        public EntityBuilder WithCharge(long max, long current)
        {
            _charge = new ChargeCapability(max, current);
            return this;
        }

        public EntityBuilder WithReservoir(Func<long>? max = null, Func<long>? current = null, Func<long, ReservoirDraw>? draw = null, Func<long, ReservoirFill>? fill = null)
        {
            if (max == null) max = () => 1000;
            if (current == null) current = () => 1000;
            if (draw == null) draw = amount => new ReservoirDraw(amount, false);
            if (fill == null) fill = amount => new ReservoirFill(amount, false);

            _reservoir = new ReservoirCapability(max, current, draw, fill);
            return this;
        }

        public EntityBuilder WithScope(Func<Entity[]> scope)
        {
            _scope = scope;
            return this;
        }

        public EntityBuilder WithPointingDirection(Direction direction)
        {
            _pointingDirection = direction;
            return this;
        }

        public EntityBuilder WithIndicateTarget(IndicateTarget target)
        {
            _indicateTarget = target;
            return this;
        }

        public EntityBuilder WithInscriptions(string[] rawInscriptions, IStatement[] parsedInscriptions)
        {
            _rawInscriptions = rawInscriptions;
            _parsedInscriptions = parsedInscriptions;
            return this;
        }

        public Entity Build()
        {
            return new Entity(
                id: _id,
                label: _label,
                location: _location,
                width: _width,
                height: _height,
                hasAgency: _hasAgency,
                weight: _weight,
                isTranslucent: _isTranslucent,
                angle: _angle,
                structuralIntegrity: _structuralIntegrity)
            {
                Life = _life,
                Charge = _charge,
                Scope = _scope,
                Reservoir = _reservoir,
                PointingDirection = _pointingDirection,
                IndicateTarget = _indicateTarget,
                RawInscriptions = _rawInscriptions,
                ParsedInscriptions = _parsedInscriptions,
            };
        }
    }
}
