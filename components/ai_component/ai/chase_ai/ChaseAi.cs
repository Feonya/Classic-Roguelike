using Godot;
using System;

public partial class ChaseAi : Node, IAi
{
    public event Action<Vector2I> Executed;
    private MapData _mapData;
    private AStarGridManager _aStarGridManager;
    private Player _player;
    private Enemy _enemy;

    public void Initialize()
    {
        _mapData = this.GetUnique<MapManager>().MapData;
        _aStarGridManager = this.GetUnique<AStarGridManager>();
        _player = this.GetUnique<Player>();
        _enemy = GetParent().GetParent<Enemy>();
    }

    public bool Execute()
    {
        var distancePlayer = _enemy.GetDistanceTo(_player.GetCell());
        if (distancePlayer > _enemy.CharacterData.Sight ||  distancePlayer <= 1)
        {
            return false;
        }

        _aStarGridManager.AStarGrid.SetPointSolid(_enemy.GetCell(), false);
        var enemyCell = _enemy.GetCell();
        var playerCell = _player.GetCell();
        var pathCells = _aStarGridManager.AStarGrid.GetIdPath(enemyCell,playerCell);
        if (pathCells.Count < 2) return false;
        var targetCell = pathCells[1];
        var direction = targetCell - enemyCell;
        Executed?.Invoke(direction);
        return true;
    }

   
}
