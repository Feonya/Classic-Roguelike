using Godot;
using System;

public partial class MeleeAi : Node, IAi
{
    private MapData _mapData;
    private Player _player;
    private Enemy _enemy;

    private CombatManager _combatManager;

    public void Initialize()
    {
        _mapData = this.GetUnique<MapManager>().MapData;
        _player = this.GetUnique<Player>();
        _enemy = GetParent().GetParent<Enemy>();
        _combatManager = this.GetUnique<CombatManager>();
    }

    public bool Execute()
    {
        var distanceToPlayer = _enemy.GetDistanceTo(_player.GetCell());
        if (distanceToPlayer > 1)
            return false;
        _combatManager.AddToCombatList(_enemy,_player);
        GD.Print(_enemy.CharacterData.Name+"攻击玩家！");
        return true;
    }

}
