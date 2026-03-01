namespace MinecraftSharp.Client;

internal enum BlockType : byte
{
    Air,
    Dirt,
    Grass,
    Stone,
    Water,
    Lava,
    Sand,
    Wood,
    Leaves,
    Glass,
}

internal readonly record struct Block
{
    public Block(BlockType type)
    {
        Type = type;
    }

    public BlockType Type { get; }
}