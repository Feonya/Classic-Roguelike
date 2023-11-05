using Godot;

public partial class Armour : Equipment, IEquipableEquipment, IBodyWearEquipment
{
    [Export]
    private float _minDefendIncrement;
    [Export]
    private float _maxDefendIncrement;

    private float _actualDefendIncrement;

    public override void Initialize()
    {
        base.Initialize();

        _actualDefendIncrement = (float)GD.RandRange(_minDefendIncrement, _maxDefendIncrement);

        _description = "装备后增加" + _actualDefendIncrement.ToString("0.0") + "防御。";
    }

    public void Equip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.BodyWearEquipment != null)
        {
            (playerData.BodyWearEquipment as IEquipableEquipment).Unequip();
        }

        playerData.Defend += _actualDefendIncrement;

        playerData.BodyWearEquipment = this;
    }

    public void EquipWithoutEffects()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.BodyWearEquipment != null)
        {
            (playerData.BodyWearEquipment as IEquipableEquipment).Unequip();
        }

        playerData.BodyWearEquipment = this;
    }

    public void Unequip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.BodyWearEquipment == null ||
            playerData.BodyWearEquipment != this)
        {
            return;
        }

        playerData.Defend -= _actualDefendIncrement;

        playerData.BodyWearEquipment = null;
    }
}
