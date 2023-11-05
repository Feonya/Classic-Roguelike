using Godot;

public partial class Necklace : Equipment, IEquipableEquipment, INeckWearEquipment
{
    [Export]
    private float _minDodgeIncrement;
    [Export]
    private float _maxDodgeIncrement;

    private float _actualDodgeIncrement;

    public override void Initialize()
    {
        base.Initialize();

        _actualDodgeIncrement = (float)GD.RandRange(_minDodgeIncrement, _maxDodgeIncrement);

        _description = "装备后增加" + (_actualDodgeIncrement * 100f).ToString("0.00") + "%闪避。";
    }

    public void Equip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.NeckWearEquipment != null)
        {
            (playerData.NeckWearEquipment as IEquipableEquipment).Unequip();
        }

        playerData.Dodge += _actualDodgeIncrement;

        playerData.NeckWearEquipment = this;
    }

    public void EquipWithoutEffects()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.NeckWearEquipment != null)
        {
            (playerData.NeckWearEquipment as IEquipableEquipment).Unequip();
        }

        playerData.NeckWearEquipment = this;
    }

    public void Unequip()
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
