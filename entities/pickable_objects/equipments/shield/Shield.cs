using Godot;

public partial class Shield : Equipment, IEquipableEquipment, ILeftHandHoldEquipment
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

        if (playerData.LeftHandHoldEquipment != null)
        {
            (playerData.LeftHandHoldEquipment as IEquipableEquipment).Unequip();
        }

        playerData.Defend += _actualDefendIncrement;

        playerData.LeftHandHoldEquipment = this;
    }

    public void EquipWithoutEffects()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.LeftHandHoldEquipment != null)
        {
            (playerData.LeftHandHoldEquipment as IEquipableEquipment).Unequip();
        }

        playerData.LeftHandHoldEquipment = this;
    }

    public void Unequip()
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
