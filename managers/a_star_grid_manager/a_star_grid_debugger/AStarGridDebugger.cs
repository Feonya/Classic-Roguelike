using Godot;

public partial class AStarGridDebugger : Node2D
{
    private MapData _mapData;

    private AStarGridManager _aStarGridManager;

    public override void _Ready()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _aStarGridManager = GetTree().CurrentScene.GetNode<AStarGridManager>("%AStarGridManager");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("toggle_a_star_debug_info"))
        {
            Visible = !Visible;
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_aStarGridManager == null || _aStarGridManager.AStarGrid == null)
        {
            return;
        }

        for (int y = 0; y < _mapData.MapSize.Y; y++)
        {
            for (int x = 0; x < _mapData.MapSize.X; x++)
            {
                var cell = new Vector2I(x, y);
                if (_aStarGridManager.AStarGrid.IsPointSolid(cell))
                {
                    DrawCircle(
                        cell * _mapData.CellSize + _mapData.CellSize / 2,
                        4,
                        Colors.Red
                    );
                }
            }
        }
    }
}