/// <summary>
/// 拾起物品调用
/// </summary>
public interface IImmediateEffectItem
{
    /// <summary>
    /// 拾起物品时调用
    /// </summary>
    public void DoImmediateEffect();
    /// <summary>
    /// 丢弃物品时调用
    /// </summary>
    public void UnImmediateEffect();
}