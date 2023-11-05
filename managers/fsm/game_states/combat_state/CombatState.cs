using System;
using Godot;

public partial class CombatState : Node, IGameState
{
    public event Action Updated;

    private CombatManager _combatManager;

    public void Initialize()
    {
        _combatManager = GetTree().CurrentScene.GetNode<CombatManager>("%CombatManager");
    }

    public void Update(double delta)
    {
        _combatManager.Update(delta);

        Updated.Invoke();
    }
}