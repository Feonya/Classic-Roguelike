using System;
using Godot;

/// <summary>
/// 处理玩家、各类敌人等对象的相关行为逻辑。
/// </summary>
public partial class ActionState : Node, IGameState
{
    public event Action Updated;

    private FogPainter _fogPainter;

    private Player _player;

    private Node _enemyContainer;

    public void Initialize()
    {
        _fogPainter = GetTree().CurrentScene.GetNode<FogPainter>("%FogPainter");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");

        _enemyContainer = GetTree().CurrentScene.GetNode<Node>("%EnemyContainer");
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
