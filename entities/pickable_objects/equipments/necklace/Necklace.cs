using Godot;

public partial class Necklace : Equipment, INeckWearEquipment
{
    [Export]
    private float _minDodgeIncrement = 0.1f;
    [Export]
    private float _maxDodgeIncrement = 0.5f;

    private float _actualDodgeIncrement;

    public override void Initialize()
    {
        base.Initialize();

        _actualDodgeIncrement = (float)GD.RandRange(_minDodgeIncrement, _maxDodgeIncrement);

        _description = "装备后增加" + (_actualDodgeIncrement * 100f).ToString("0.00") + "%闪避。";
    }

    public override void Equip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.NeckWearEquipment != null)
        {
            (playerData.NeckWearEquipment as Equipment).Unequip();
        }

        playerData.Dodge += _actualDodgeIncrement;

        playerData.NeckWearEquipment = this;
    }

    public override void EquipWithoutEffect()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.NeckWearEquipment != null)
        {
            (playerData.NeckWearEquipment as Equipment).Unequip();
        }

        playerData.NeckWearEquipment = this;
    }

    public override void Unequip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.NeckWearEquipment == null ||
            playerData.NeckWearEquipment != this)
        {
            return;
        }

        playerData.Dodge -= _actualDodgeIncrement;

        playerData.NeckWearEquipment = null;
    }
}
