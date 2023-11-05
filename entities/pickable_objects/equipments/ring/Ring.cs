using Godot;

public partial class Ring : Equipment, IEquipableEquipment, IFingerWearEquipment
{
    [Export]
    private float _minCritIncrement;
    [Export]
    private float _maxCritIncrement;

    private float _actualCritIncrement;

    public override void Initialize()
    {
        base.Initialize();

        _actualCritIncrement = (float)GD.RandRange(_minCritIncrement, _maxCritIncrement);

        _description = "装备后增加" + (_actualCritIncrement * 100f).ToString("0.00") + "%暴击。";
    }

    public void Equip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.FingerWearEquipment != null)
        {
            (playerData.FingerWearEquipment as IEquipableEquipment).Unequip();
        }

        playerData.Crit += _actualCritIncrement;

        playerData.FingerWearEquipment = this;
    }

    public void EquipWithoutEffects()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.FingerWearEquipment != null)
        {
            (playerData.FingerWearEquipment as IEquipableEquipment).Unequip();
        }

        playerData.FingerWearEquipment = this;
    }

    public void Unequip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.FingerWearEquipment == null ||
            playerData.FingerWearEquipment != this)
        {
            return;
        }

        playerData.Crit -= _actualCritIncrement;

        playerData.FingerWearEquipment = null;
    }
}
