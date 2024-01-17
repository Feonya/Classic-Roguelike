using Godot;
using System;

public partial class DungeonData : MapData
{
    public Vector2I MinRoomSize = new(3, 3);
    public Vector2I MaxRoomSize = new(15, 15);
}
