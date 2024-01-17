using Godot;
using System;
using System.Threading.Tasks;

public partial class ReviveCross : Item, IDeadEffectItem
{
    [Export]
    private float _reviveDelaySeconds = 2f;

    public async Task DoDeadEffect()
    {
        await Task.Delay((int)(_reviveDelaySeconds * 1000f));
        _player.CharacterData.Health = _player.CharacterData.MaxHealth;
        (_player.CharacterData as PlayerData).Inventory.Remove(this);
    }

    public override void Initialize()
    {
        base.Initialize();
        _description = $"死亡{_reviveDelaySeconds.ToString("0.0")}秒后消耗本物品并复活";
    }
}
