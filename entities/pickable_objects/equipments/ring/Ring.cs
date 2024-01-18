using Godot;

public partial class Ring : Equipment, IFingerWearEquipment
{
    [Export]
    private float _minCritIncrement = 0.05f;
    [Export]
    private float _maxCritIncrement = 0.3f;

    private float _actualCritIncrement;

    public override void Initialize()
    {
        base.Initialize();

        _actualCritIncrement = (float)GD.RandRange(_minCritIncrement, _maxCritIncrement);

        _description = "装备后增加" + (_actualCritIncrement * 100f).ToString("0.00") + "%暴击。";
    }

    public override void Equip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.FingerWearEquipment != null)
        {
            (playerData.FingerWearEquipment as Equipment).Unequip();
        }

        playerData.Crit += _actualCritIncrement;

        playerData.FingerWearEquipment = this;
    }

    public override void EquipWithoutEffect()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.FingerWearEquipment != null)
        {
            (playerData.FingerWearEquipment as Equipment).Unequip();
        }

        playerData.FingerWearEquipment = this;
    }

    public override void Unequip()
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
