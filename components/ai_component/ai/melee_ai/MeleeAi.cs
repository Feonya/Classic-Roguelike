using Godot;

public partial class MeleeAi : Node, IAi
{
    private MapData _mapData;

    private CombatManager _combatManager;

    private Player _player;
    private Enemy _enemy;

    public void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _combatManager = GetTree().CurrentScene.GetNode<CombatManager>("%CombatManager");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");
        _enemy = GetParent().GetParent<Enemy>();
    }

    public bool Execute()
    {
        var distanceToPlayer = _enemy.GetDistanceTo(
            (Vector2I)(_player.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize
        );

        if (distanceToPlayer > 1) { return false; }

        _combatManager.AddToCombatList(_enemy, _player);
        GD.Print(_enemy.CharacterData.Name + "攻击玩家！");

        return true;
    }
}
