using Godot;
using System;

public partial class Mace : Equipment, IRightHandHoldEquipment
{
    [Export]
    private float _actualAttackIncrement = 18f;

    public override void Initialize()
    {
        base.Initialize();
		_description = $"装备后增加{_actualAttackIncrement}点攻击力";
    }

    public override void Equip()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.RightHandHoldEquipment != null)
            {
                (playerData.RightHandHoldEquipment as Equipment).Unequip();
            }

            playerData.RightHandHoldEquipment = this;

            playerData.Attack += _actualAttackIncrement;
        }
    }

    public override void EquipWithoutEffect()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.RightHandHoldEquipment != null)
            {
                (playerData.RightHandHoldEquipment as Equipment).Unequip();
            }

            playerData.RightHandHoldEquipment = this;
        }
    }

    public override void Unequip()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.RightHandHoldEquipment != this)
                return;

            playerData.RightHandHoldEquipment = null;

            playerData.Attack -= _actualAttackIncrement;
        }
    }
}
