using Godot;
using System;

public partial class Fsm : Node , IManager
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

        _startState.Updated += On_StartState_Updated;
        _waitForInputState.Updated += On_WaitForInputState_Updated;
        _actionState.Updated += On_ActionState_Updated;
        _combatState.Updated += On_CombatState_Updated;

        _startState.Initialize();
        _waitForInputState.Initialize();
        _actionState.Initialize();
        _combatState.Initialize();

        _currentGameState = _startState;
    }

    public void Update()
    {
        _currentGameState.Update();
    }

    private void On_CombatState_Updated()
    {
        _currentGameState = _waitForInputState;
        //GD.Print($"[{Engine.GetPhysicsFrames()}] 切换至WaitForInputState");
    }

    private void On_ActionState_Updated()
    {
        _currentGameState = _combatState;
        //GD.Print($"[{Engine.GetPhysicsFrames()}] 切换至CombatState");
    }

    private void On_WaitForInputState_Updated()
    {
        _currentGameState = _actionState;
        //GD.Print($"[{Engine.GetPhysicsFrames()}] 切换至ActionState");
    }

    private void On_StartState_Updated()
    {
        _currentGameState = _waitForInputState;
        //GD.Print($"[{Engine.GetPhysicsFrames()}] 切换至WaitForInputState");
    }
}
