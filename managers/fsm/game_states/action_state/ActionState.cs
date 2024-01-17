using Godot;
using System;

/// <summary>
/// 处理玩家、各类敌人等相关行为逻辑。
/// </summary>
public partial class ActionState : Node, IGameState
{
    public event Action Updated;
    private FogPainter _fogPainter;
    private Player _player;

    private Node _enemyContainer;

    public void Initialize()
    {
        _fogPainter = this.GetUnique<FogPainter>();
        _player = this.GetUnique<Player>();
        _enemyContainer = this.GetUnique("%EnemyContainer");
    }

    public void Update()
    {
        _player.Update();
        for (int i = 0; i < _enemyContainer.GetChildCount(); i++)
        {
            var enemy = _enemyContainer.GetChild<Enemy>(i);
            enemy.Update();
        }

        _fogPainter.Update();
        Updated.Invoke();
    }
}
