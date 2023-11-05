using System.Threading.Tasks;

public partial class ReviveCross : Item, IDeadEffectItem
{
    public async Task DoDeadEffect()
    {
        await Task.Delay(2000);

        _player.CharacterData.Health = _player.CharacterData.MaxHealth;

        (_player.CharacterData as PlayerData).Inventory.Remove(this);
    }
}
