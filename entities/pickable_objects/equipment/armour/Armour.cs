using Godot;
using System;
using System.Diagnostics;

public partial class Armour : Equipment,IBodyWearEquipment
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
        _description = $"装备后增加{_actualDefendIncrement}点防御力";
    }

    public override void Equip()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.BodyWearEquipment != null)
            {
                (playerData.BodyWearEquipment as Equipment).Unequip();
            }

            playerData.BodyWearEquipment = this;
            playerData.Defend += _actualDefendIncrement;
        }
    }

    public override void EquipWithoutEffect()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.BodyWearEquipment != null)
            {
                (playerData.BodyWearEquipment as Equipment).Unequip();
            }

            playerData.BodyWearEquipment = this;
        }
    }

    public override void Unequip()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.BodyWearEquipment != this)
                return;

            playerData.BodyWearEquipment = null;
            playerData.Defend -= _actualDefendIncrement;
        }
    }
}

