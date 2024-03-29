using System;
using Godot;

/// <summary>
/// 处理战斗逻辑。
/// </summary>
public partial class CombatState : Node, IGameState
{
    public event Action Updated;

    private CombatManager _combatManager;

    private Node _enemyContainer;

    public void Initialize()
    {
        _combatManager = GetTree().CurrentScene.GetNode<CombatManager>("%CombatManager");

        _enemyContainer = GetTree().CurrentScene.GetNode<Node>("%EnemyContainer");
    }

    public void Update()
    {
        _combatManager.Update();

        for (int i = 0; i < _enemyContainer.GetChildCount(); i++)
        {
            var enemy = _enemyContainer.GetChild<Enemy>(i);

            enemy.LateUpdate();
        }

        Updated.Invoke();
    }
}
