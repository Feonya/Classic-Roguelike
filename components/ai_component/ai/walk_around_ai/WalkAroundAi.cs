using Godot;
using System;

public partial class WalkAroundAi : Node, IAi
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
        if(distancePlayer <= _enemy.CharacterData.Sight) 
        {
            return false;
        }

        _aStarGridManager.AStarGrid.SetPointSolid(_enemy.GetCell(),false);
        var direction = new Vector2I(GD.RandRange(-1, 1), GD.RandRange(-1, 1));
        Executed?.Invoke(direction);
        return true;

    }

   
}
