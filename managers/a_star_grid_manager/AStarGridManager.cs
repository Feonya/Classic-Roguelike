using Godot;
using Godot.Collections;

public partial class AStarGridManager : Node, IManager
{
    private MapData _mapData;

    private AStarGrid2D _aStarGrid;

    public AStarGrid2D AStarGrid { get => _aStarGrid; }

    public void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _aStarGrid = new AStarGrid2D
        {
            Region = new Rect2I(0, 0, _mapData.MapSize),
            CellSize = _mapData.CellSize
        };

        _aStarGrid.Update();

        for (int y = 0; y < _mapData.MapSize.Y; y++)
        {
            for (int x = 0; x < _mapData.MapSize.X; x++)
            {
                var cell = new Vector2I(x, y);
                if (IsCellShouldSetSolid(cell))
                {
                    _aStarGrid.SetPointSolid(cell, true);
                }
            }
        }
    }

    private bool IsCellShouldSetSolid(Vector2I cell)
    {
        var targetPosition = cell * _mapData.CellSize + _mapData.CellSize / 2;

        var space = GetTree().CurrentScene
            .GetNode<Player>("%Player").GetWorld2D().DirectSpaceState;
        var parameters = new PhysicsPointQueryParameters2D
        {
            Position = targetPosition,
            CollideWithAreas = true,
            CollisionMask = (int)PhysicsLayer.BlockMovement,
            Exclude = new Array<Rid>
            {
                GetTree().CurrentScene.GetNode<Area2D>("%Player/Area2D").GetRid()
            }
        };
        var results = space.IntersectPoint(parameters);

        if (results.Count > 0) { return true; }

        return false;
    }

    public void Update()
    {
    }
}
