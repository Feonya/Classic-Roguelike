using System;
using Godot;

public partial class ChaseAi : Node, IAi
{
    public event Action<Vector2I> Executed;

    private MapData _mapData;

    private AStarGridManager _aStarGridManager;

    private Player _player;
    private Enemy _enemy;

    public void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _aStarGridManager = GetTree().CurrentScene.GetNode<AStarGridManager>("%AStarGridManager");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");
        _enemy = GetParent().GetParent<Enemy>();
    }

    public bool Execute()
    {
        var distanceToPlayer = _enemy.GetDistanceTo(
            (Vector2I)(_player.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize
        );

        if (distanceToPlayer > _enemy.CharacterData.Sight || distanceToPlayer <= 1)
        {
            return false;
        }

        var enemyCell = (Vector2I)
            (_enemy.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize;
        var playerCell = (Vector2I)
            (_player.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize;

        _aStarGridManager.AStarGrid.SetPointSolid(
            (Vector2I)(_enemy.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize,
            false
        );

        var pathCells = _aStarGridManager.AStarGrid.GetIdPath(enemyCell, playerCell);

        if (pathCells.Count < 2) { return false; }

        var targetCell = pathCells[1];
        var direction = targetCell - enemyCell;

        Executed.Invoke(direction);

        return true;
    }
}
