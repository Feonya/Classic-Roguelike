using System;
using Godot;

public partial class Fsm : Node, IManager
{
    private StartState _startState;
    private WaitForInputState _waitForInputState;
    private ActionState _actionState;
    private CombatState _combatState;

    private IGameState _currentGameState;

    public void Initialize()
    {
        _startState = GetNode<StartState>("StartState");
        _waitForInputState = GetNode<WaitForInputState>("WaitForInputState");
        _actionState = GetNode<ActionState>("ActionState");
        _combatState = GetNode<CombatState>("CombatState");

        _startState.Updated += On_InitializeState_Updated;
        _waitForInputState.Updated += On_WaitForInputState_Updated;
        _actionState.Updated += On_ActionState_Updated;
        _combatState.Updated += On_CombatState_Updated;

        _startState.Initialize();
        _waitForInputState.Initialize();
        _actionState.Initialize();
        _combatState.Initialize();

        _currentGameState = _startState;
    }

    public void Update(double delta)
    {
        _currentGameState.Update(delta);
    }

    private void On_InitializeState_Updated()
    {
        _currentGameState = _waitForInputState;
        // GD.Print("进入WaitForInputState");
    }

    private void On_WaitForInputState_Updated()
    {
        _currentGameState = _actionState;
        // GD.Print("进入ActionState");
    }

    private void On_ActionState_Updated()
    {
        _currentGameState = _combatState;
        // GD.Print("进入CombatState");
    }

    private void On_CombatState_Updated()
    {
        _currentGameState = _waitForInputState;
        // GD.Print("进入WaitForInputState");
    }
}
