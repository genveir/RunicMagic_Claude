namespace RunicMagic.World;

public readonly record struct EntityId(Guid Value)
{
    public static EntityId New()
    {
        var id = new EntityId(Guid.NewGuid());
        return id;
    }

    public override string ToString()
    {
        var value = Value.ToString();
        return value;
    }
}
