public partial class SightBall : Item, IImmediateEffectItem, IUniquePickableObject
{
    private static bool _isAppeared;

    public bool IsAppeared { get => _isAppeared; set => _isAppeared = value; }

    public void DoImmediateEffect()
    {
        _player.CharacterData.Sight += 3;
    }

    public void UndoImmediateEffect()
    {
        _player.CharacterData.Sight -= 3;
    }
}
