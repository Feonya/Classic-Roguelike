using System.Threading.Tasks;

/// <summary>
/// 拥有者死亡时调用
/// </summary>
public interface IDeadEffectItem
{
    /// <summary>
    /// 拥有者死亡时调用
    /// </summary>
    public Task DoDeadEffect();
}