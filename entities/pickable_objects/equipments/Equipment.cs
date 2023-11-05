using Godot;

public partial class Equipment : PickableObject
{
    public bool IsEquipped
    {
        get
        {
            var playerData = _player.CharacterData as PlayerData;

            if (playerData.LeftHandHoldEquipment == this ||
                playerData.RightHandHoldEquipment == this ||
                playerData.BodyWearEquipment == this ||
                playerData.FingerWearEquipment == this ||
                playerData.NeckWearEquipment == this)
            {
                return true;
            }

            return false;
        }
    }
}
