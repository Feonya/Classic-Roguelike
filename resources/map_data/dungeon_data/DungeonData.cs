using Godot;
using Godot.Collections;

public partial class DungeonData : MapData
{
    public Vector2I MinRoomSize = new(3, 3);
    public Vector2I MaxRoomSize = new(15, 15);

    public System.Collections.Generic.List<Rect2I> Rooms = new();
    public Array<Vector2I> CorridorCells = new();
}
