using Godot;
using System;

public partial class Equipment : PickableObject
{
	public bool IsEquipped 
	{
		get 
		{
			var playerData = _player.CharacterData as PlayerData;
			if (playerData.LeftHandHoldEquipment == this ||
				playerData.RightHandHoldEquipment == this ||
				playerData.FingerWearEquipment == this ||
				playerData.BodyWearEquipment == this ||
				playerData.NeckWearEquipment == this)
				return true;
			return false;
		}
	
	}

	public virtual void Equip()
	{
		throw new NotSupportedException("Equipment does not support direct calls to Equip. Please use the overridden method.");
	}

	public virtual void EquipWithoutEffect()
	{
        throw new NotSupportedException("Equipment does not support direct calls to EquipWithoutEffect. Please use the overridden method.");
    }

    public virtual void Unequip()
	{
        throw new NotSupportedException("Equipment does not support direct calls to Unequip. Please use the overridden method.");
    }

}
