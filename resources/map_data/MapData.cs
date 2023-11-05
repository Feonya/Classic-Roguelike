using Godot;

public partial class MapData : Resource
{
    public Vector2I MapSize = new(30, 20);
    public Vector2I CellSize = new(16, 16);
}
