using Godot;

public partial class Shield : Equipment, ILeftHandHoldEquipment
{
    [Export]
    private float _minDefendIncrement = 5f;
    [Export]
    private float _maxDefendIncrement = 8f;

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

        if (playerData.LeftHandHoldEquipment != null)
        {
            (playerData.LeftHandHoldEquipment as Equipment).Unequip();
        }

        playerData.Defend += _actualDefendIncrement;

        playerData.LeftHandHoldEquipment = this;
    }

    public override void EquipWithoutEffect()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.LeftHandHoldEquipment != null)
        {
            (playerData.LeftHandHoldEquipment as Equipment).Unequip();
        }

        playerData.LeftHandHoldEquipment = this;
    }

    public override void Unequip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.LeftHandHoldEquipment == null ||
            playerData.LeftHandHoldEquipment != this)
        {
            return;
        }

        playerData.Defend -= _actualDefendIncrement;

        playerData.LeftHandHoldEquipment = null;
    }
}
