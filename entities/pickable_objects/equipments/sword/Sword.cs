using Godot;

public partial class Sword : Equipment, IRightHandHoldEqiupment
{
    [Export]
    private float _minAttackIncrement = 3f;
    [Export]
    private float _maxAttackIncrement = 20f;

    private float _actualAttackIncrement;

    public override void Initialize()
    {
        base.Initialize();

        _actualAttackIncrement = (float)GD.RandRange(_minAttackIncrement, _maxAttackIncrement);

        _description = "装备后增加" + _actualAttackIncrement.ToString("0.0") + "攻击。";
    }

    public override void Equip()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.RightHandHoldEquipment != null)
        {
            (playerData.RightHandHoldEquipment as Equipment).Unequip();
        }

        playerData.Attack += _actualAttackIncrement;

        playerData.RightHandHoldEquipment = this;
    }

    public override void EquipWithoutEffect()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.RightHandHoldEquipment != null)
        {
            (playerData.RightHandHoldEquipment as Equipment).Unequip();
        }

        playerData.RightHandHoldEquipment = this;
    }

    public override void Unequip()
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
