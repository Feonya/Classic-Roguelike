using Godot;
using System;

public partial class Ring : Equipment,IFingerWearEquipment
{
    [Export] private float _minCritIncrement = 0.05f;
    [Export] private float _maxCritIncrement = 0.3f;

    private float _actualCritIncrement;

    public override void Initialize()
    {
        base.Initialize();

        _actualCritIncrement = (float)GD.RandRange(_minCritIncrement, _maxCritIncrement);
        _description = $"装备后增加{_actualCritIncrement}%暴击";
    }
    public override void Equip()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.FingerWearEquipment != null)
            {
                (playerData.FingerWearEquipment as Equipment).Unequip();
            }

            playerData.FingerWearEquipment = this;

            playerData.Crit += _actualCritIncrement;
        }
    }

    public override void EquipWithoutEffect()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.FingerWearEquipment != null)
            {
                (playerData.FingerWearEquipment as Equipment).Unequip();
            }

            playerData.FingerWearEquipment = this;
        }
    }

    public override void Unequip()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.FingerWearEquipment != this)
                return;

            playerData.FingerWearEquipment = null;

            playerData.Crit -= _actualCritIncrement;
        }
    }
}
