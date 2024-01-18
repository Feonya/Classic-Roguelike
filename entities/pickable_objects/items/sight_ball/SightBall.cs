using Godot;

public partial class SightBall : Item, IImmediateEffectItem
{
    [Export]
    private int _sightIncrement = 3;

    public override void Initialize()
    {
        base.Initialize();

        _description = "拾取后立即增加" + _sightIncrement + "点视野。";
    }

    public void DoImmediateEffect()
    {
        _player.CharacterData.Sight += _sightIncrement;
    }

    public void UndoImmediateEffect()
    {
        _player.CharacterData.Sight -= _sightIncrement;
    }
}
