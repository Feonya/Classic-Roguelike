using Godot;
using System;

public partial class Shield : Equipment,ILeftHandHoldEquipment
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
        _description = $"装备后增加{_actualDefendIncrement}点防御力";
    }

    public override void Equip()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.LeftHandHoldEquipment != null)
            {
                (playerData.LeftHandHoldEquipment as Equipment).Unequip();
            }

            playerData.LeftHandHoldEquipment = this;
            playerData.Defend += _actualDefendIncrement;
        }
    }

    public override void EquipWithoutEffect()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.LeftHandHoldEquipment != null)
            {
                (playerData.LeftHandHoldEquipment as Equipment).Unequip();
            }

            playerData.LeftHandHoldEquipment = this;
        }
    }

    public override void Unequip()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.LeftHandHoldEquipment != this)
                return;

            playerData.LeftHandHoldEquipment = null;
            playerData.Defend -= _actualDefendIncrement;
        }
    }
}
