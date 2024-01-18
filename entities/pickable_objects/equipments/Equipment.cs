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
                playerData.NeckWearEquipment == this ||
                playerData.FingerWearEquipment == this)
            {
                return true;
            }

            return false;
        }
    }

    public virtual void Equip()
    {
        throw new System.Exception("不能直接调用本基类方法！");
    }

    public virtual void EquipWithoutEffect()
    {
        throw new System.Exception("不能直接调用本基类方法！");
    }

    public virtual void Unequip()
    {
        throw new System.Exception("不能直接调用本基类方法！");
    }
}
