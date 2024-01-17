using Godot;
using System;

public partial class MapData : Resource
{
    public Vector2I MapSize = new(60, 40);
    public Vector2I CellSize = new(16, 16);
}
