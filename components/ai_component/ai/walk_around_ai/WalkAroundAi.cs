using System;
using Godot;

public partial class WalkAroundAi : Node, IAi
{
    public event Action<Vector2I> Executed;

    private MapData _mapData;

    private AStarGridManager _aStarGridManager;

    private Enemy _enemy;
    private Player _player;

    public void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _aStarGridManager = GetTree().CurrentScene.GetNode<AStarGridManager>("%AStarGridManager");

        _enemy = GetParent().GetParent<Enemy>();
        _player = GetTree().CurrentScene.GetNode<Player>("%Player");
    }

    public bool Execute()
    {
        var distanceToPlayer = _enemy.GetDistanceTo(
            (Vector2I)(_player.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize
        );

        if (distanceToPlayer <= _enemy.CharacterData.Sight) { return false; }

        _aStarGridManager.AStarGrid.SetPointSolid(
            (Vector2I)(_enemy.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize,
            false
        );

        var direction = new Vector2I(GD.RandRange(-1, 1), GD.RandRange(-1, 1));

        Executed?.Invoke(direction);

        return true;
    }
}
