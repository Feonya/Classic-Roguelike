using Godot;

public partial class SmallPotion : Item, IConsumableItem
{
    [Export]
    private float _healthIncrement = 10f;

    public override void Initialize()
    {
        base.Initialize();

        _description = "使用后增加" + _healthIncrement + "点血量。";
    }

    public void Consume()
    {
        _player.CharacterData.Health += _healthIncrement;
        if (_player.CharacterData.Health > _player.CharacterData.MaxHealth)
        {
            _player.CharacterData.Health = _player.CharacterData.MaxHealth;
        }

        (_player.CharacterData as PlayerData).Inventory.Remove(this);
    }
}
