using Godot;

public partial class Sword : Equipment, IEquipableEquipment, IRightHandHoldEquipment
{
    [Export]
    private float _minAttackIncrement;
    [Export]
    private float _maxAttackIncrement;

    private float _actualAttackIncrement;

    public override void Initialize()
    {
        base.Initialize();

        _actualAttackIncrement = (float)GD.RandRange(_minAttackIncrement, _maxAttackIncrement);

        _description = "装备后增加" + _actualAttackIncrement.ToString("0.0") + "攻击。";
    }

    public void Equip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.RightHandHoldEquipment != null)
        {
            (playerData.RightHandHoldEquipment as IEquipableEquipment).Unequip();
        }

        playerData.Attack += _actualAttackIncrement;

        playerData.RightHandHoldEquipment = this;
    }

    public void EquipWithoutEffects()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.RightHandHoldEquipment != null)
        {
            (playerData.RightHandHoldEquipment as IEquipableEquipment).Unequip();
        }

        playerData.RightHandHoldEquipment = this;
    }

    public void Unequip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.RightHandHoldEquipment == null ||
            playerData.RightHandHoldEquipment != this)
        {
            return;
        }

        playerData.Attack -= _actualAttackIncrement;

        playerData.RightHandHoldEquipment = null;
    }
}
