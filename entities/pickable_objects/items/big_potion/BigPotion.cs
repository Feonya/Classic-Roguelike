public partial class BigPotion : Item, IConsumableItem
{
    public void Consume()
    {
        _player.CharacterData.Health += 20f;
        if (_player.CharacterData.Health > _player.CharacterData.MaxHealth)
        {
            _player.CharacterData.Health = _player.CharacterData.MaxHealth;
        }

        (_player.CharacterData as PlayerData).Inventory.Remove(this);
    }
}
