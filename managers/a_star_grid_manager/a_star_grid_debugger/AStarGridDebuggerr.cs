using Godot;
using System;

public partial class AStarGridDebuggerr : Node2D
{
    private MapData _mapData;
    private AStarGridManager _aStarGridManager;

    public override void _Ready()
    {
        _mapData = this.GetUnique<MapManager>().MapData;
        _aStarGridManager = this.GetUnique<AStarGridManager>();


    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_aStarGridManager == null || _aStarGridManager.AStarGrid == null)
            return;

        for(int y = 0; y < _mapData.MapSize.Y; y++)
        {
            for (int x = 0; x < _mapData.MapSize.X; x++)
            {
                var cell = new Vector2I(x, y);
                if (_aStarGridManager.AStarGrid.IsPointSolid(cell))
                {
                    DrawCircle(cell * _mapData.CellSize + _mapData.CellSize / 2, 4, Colors.Red);
                }
            }
        }
    }

}
