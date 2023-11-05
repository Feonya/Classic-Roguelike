public partial class SmallPotion : Item, IConsumableItem
{
    public void Consume()
    {
        _player.CharacterData.Health += 10f;
        if (_player.CharacterData.Health > _player.CharacterData.MaxHealth)
        {
            _player.CharacterData.Health = _player.CharacterData.MaxHealth;
        }

        (_player.CharacterData as PlayerData).Inventory.Remove(this);
    }
}
