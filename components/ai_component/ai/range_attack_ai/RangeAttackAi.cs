using Godot;
using System;

public partial class RangeAttackAi : Node,IAi
{
    private MapData _mapData;
    private Player _player;
    private Enemy _enemy;
    private CombatManager _combatManager;

    private Line2D _line;

    public void Initialize()
    {
        _mapData = this.GetUnique<MapManager>().MapData;
        _player = this.GetUnique<Player>();
        _enemy = GetParent().GetParent<Enemy>();
        _line = GetNode<Line2D>("Line2D");
        _combatManager = this.GetUnique<CombatManager>();
    }

    public bool Execute()
    {
        var distanceToPlayer = _enemy.GetDistanceTo(_player.GetCell());
        if (distanceToPlayer > _enemy.CharacterData.Sight)
            return false;
        _combatManager.AddToCombatList(_enemy,_player);
        GD.Print(_enemy.CharacterData.Name + "远处攻击玩家！");
        ShowRangeAttackLine(_enemy.GlobalPosition,_player.GlobalPosition);
        return true;
    }

    private async void ShowRangeAttackLine(Vector2 start,Vector2 target)
    {
        _line.SetPointPosition(0, start);
        _line.SetPointPosition(1, target);
        
        await ToSignal(GetTree(), "process_frame");

        _line.SetPointPosition(0, Vector2.Zero);
        _line.SetPointPosition(1, Vector2.Zero);
    }
}
