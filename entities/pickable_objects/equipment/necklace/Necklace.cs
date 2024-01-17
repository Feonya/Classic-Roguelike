using Godot;
using System;

public partial class Necklace : Equipment,INeckWearEquipment
{
	[Export] private float _minDodgeIncrement = 0.1f;
	[Export] private float _maxDodgeIncrement = 0.5f;

    private float _actualDodgeIncrement;

    public override void Initialize()
    {
        base.Initialize();

        _actualDodgeIncrement = (float)GD.RandRange(_minDodgeIncrement,_maxDodgeIncrement);
        _description = $"装备后增加{_actualDodgeIncrement}%闪避";
    }

    public override void Equip()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.NeckWearEquipment != null)
            {
                (playerData.NeckWearEquipment as Equipment).Unequip();
            }

            playerData.NeckWearEquipment = this;

            playerData.Dodge += _actualDodgeIncrement;
        }
    }

    public override void EquipWithoutEffect()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.NeckWearEquipment != null)
            {
                (playerData.NeckWearEquipment as Equipment).Unequip();
            }

            playerData.NeckWearEquipment = this;
        }
    }

    public override void Unequip()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.NeckWearEquipment != this)
                return;

            playerData.NeckWearEquipment = null;

            playerData.Dodge -= _actualDodgeIncrement;
        }
    }

}
