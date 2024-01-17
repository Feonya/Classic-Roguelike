using Godot;
using System;

public partial class Sword : Equipment,IRightHandHoldEquipment
{
	[Export]
	private float _minAttackIncrement = 3f; //最小攻击增量
	[Export]
	private float _maxAttackIncrement = 20f; //最大攻击增量


	private float _actualAttackIncrement;

    public override void Initialize()
    {
        base.Initialize();

		_actualAttackIncrement = (float)GD.RandRange(_minAttackIncrement,_maxAttackIncrement);
		_description = $"装备后增加{_actualAttackIncrement}点攻击力";
	}

    public override void Equip()
    {
        if(_player.CharacterData is PlayerData playerData)
		{
			if(playerData.RightHandHoldEquipment != null)
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
