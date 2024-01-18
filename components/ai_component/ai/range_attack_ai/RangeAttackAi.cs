using Godot;

public partial class RangeAttackAi : Node, IAi
{
    private MapData _mapData;

    private CombatManager _combatManager;

    private Player _player;
    private Enemy _enemy;

    private Line2D _line;

    public void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _combatManager = GetTree().CurrentScene.GetNode<CombatManager>("%CombatManager");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");
        _enemy = GetParent().GetParent<Enemy>();

        _line = GetNode<Line2D>("Line2D");
    }

    public bool Execute()
    {
        var distanceToPlayer = _enemy.GetDistanceTo(
            (Vector2I)(_player.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize
        );

        if (distanceToPlayer > _enemy.CharacterData.Sight) { return false; }

        _combatManager.AddToCombatList(_enemy, _player);
        GD.Print(_enemy.CharacterData.Name + "远程攻击玩家！");

        ShowRangeAttackLine(_enemy.GlobalPosition, _player.GlobalPosition);

        return true;
    }

    private async void ShowRangeAttackLine(Vector2 start, Vector2 target)
    {
        _line.SetPointPosition(0, start);
        _line.SetPointPosition(1, target);

        await ToSignal(GetTree(), "process_frame");

        _line.SetPointPosition(0, Vector2.Zero);
        _line.SetPointPosition(1, Vector2.Zero);
    }
}
