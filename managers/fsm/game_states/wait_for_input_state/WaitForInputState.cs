using Godot;
using System;

/// <summary>
/// 本状态用于响应玩家输入处理器(InputHandler)发送来的输入事件，在没有接收到新输入事件时，本状态会持续监听并阻断状态循环。 
/// </summary>
public partial class WaitForInputState : Node, IGameState
{
    public event Action Updated;

    private InputHandler _inputHandler;
    private StairManager _stairManager;
    
    private InventoryWindow _inventoryWindow;

    public void Initialize()
    {
        _inputHandler = this.GetUnique<InputHandler>();
        _stairManager = this.GetUnique<StairManager>();
        _inventoryWindow = this.GetUnique<InventoryWindow>();
        _inputHandler.MovementInputHandled += On_InputHandler_MovementInputHandled;
        _inputHandler.PickUpInputHandled += On_InputHandler_PickUpInputHandled;
        _inputHandler.ToggleInventoryWindowInputHandled += On_InputHandler_ToggleInventoryWindowInputHandled;

        _inputHandler.UseInventoryObjectInputHandled += On_InputHandler_UseInventoryObjectInputHandled;
        _inputHandler.PutAwayInventoryObjectInputHandled += On_InputHandler_PutAwayInventoryObjectInputHandled;
        _inputHandler.GoUpStairInputHandled += On_InputHandler_GoUpStairInputHandled;
        _inputHandler.GoDownStairInputHandled += On_InputHandler_GoDownStairInputHandled;
    }

    private void On_InputHandler_GoDownStairInputHandled()
    {
        _stairManager.TryGoToNextScene();
    }

    private void On_InputHandler_GoUpStairInputHandled()
    {
        _stairManager.TryGoToPreviourScene();
    }

    private void On_InputHandler_ToggleInventoryWindowInputHandled()
    {
        _inventoryWindow.Toggle();
    }

    public void Update()
    {
        //GD.Print($"[{Engine.GetPhysicsFrames()}] 等待输入");
        _inputHandler.Update();
    }

    private void On_InputHandler_MovementInputHandled(Vector2I direction)
    {
        Updated.Invoke();
    }

    private void On_InputHandler_PickUpInputHandled()
    {
        Updated.Invoke();
    }

    private void On_InputHandler_PutAwayInventoryObjectInputHandled()
    {
        _inventoryWindow.PutAwayInventoryObject();
        Updated.Invoke();
    }

    private void On_InputHandler_UseInventoryObjectInputHandled()
    {
        _inventoryWindow.UseInventoryObject();
        Updated.Invoke();
    }
}

