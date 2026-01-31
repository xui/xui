namespace Web4;

class KeyCache
{
    private readonly List<KeyCache?> children = [];
    public static KeyCache Root { get; } = new KeyCache(null!, []);
    public byte[] Key { get; private set; }
    public KeyCache Parent { get; private set; }

    public byte[]? this[int index]
    {
        get => index < children.Count ? children[index]?.Key : null;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            while (children.Count < index)
                children.Add(null);

            if (index == children.Count)
                children.Add(new KeyCache(this, value));
            else
                children[index] = new KeyCache(this, value);
        }
    }

    private KeyCache(KeyCache parent, byte[] key)
    {
        Parent = parent;
        Key = key;
    }

    public KeyCache NextGeneration(int index)
    {
        return children[index] ??= new KeyCache(this, Parent.Key);
    }
}