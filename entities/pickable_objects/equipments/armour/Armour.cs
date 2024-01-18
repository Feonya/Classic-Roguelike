using Godot;

public partial class Armour : Equipment, IBodyWearEquipment
{
    [Export]
    private float _minDefendIncrement = 3f;
    [Export]
    private float _maxDefendIncrement = 10f;

    private float _actualDefendIncrement;

    public override void Initialize()
    {
        base.Initialize();

        _actualDefendIncrement = (float)GD.RandRange(_minDefendIncrement, _maxDefendIncrement);

        _description = "装备后增加" + _actualDefendIncrement.ToString("0.0") + "防御。";
    }

    public override void Equip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.BodyWearEquipment != null)
        {
            (playerData.BodyWearEquipment as Equipment).Unequip();
        }

        playerData.Defend += _actualDefendIncrement;

        playerData.BodyWearEquipment = this;
    }

    public override void EquipWithoutEffect()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.BodyWearEquipment != null)
        {
            (playerData.BodyWearEquipment as Equipment).Unequip();
        }

        playerData.BodyWearEquipment = this;
    }

    public override void Unequip()
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
