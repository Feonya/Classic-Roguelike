using System;
using Godot;

/// <summary>
/// 本状态用于响应输入处理器（InputHandler）发送来的输入事件，在没有接收到新输入事件时，本状态会持续监听并阻断状态循环。
/// </summary>
public partial class WaitForInputState : Node, IGameState
{
    public event Action Updated;

    private InputHandler _inputHandler;
    private StairManager _stairManager;
    private SaveLoadManager _saveLoadManager;

    private InventoryWindow _inventoryWindow;

    public void Initialize()
    {
        _inputHandler = GetTree().CurrentScene.GetNode<InputHandler>("%InputHandler");
        _stairManager = GetTree().CurrentScene.GetNode<StairManager>("%StairManager");
        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        _inventoryWindow = GetTree().CurrentScene.GetNode<InventoryWindow>("%InventoryWindow");

        _inputHandler.MovementInputHandled += On_InputHandler_MovementInputHandled;
        _inputHandler.PickUpInputHandled += On_InputHandler_PickUpInputHandled;
        _inputHandler.ToggleInventoryWindowInputHandled += On_InputHandler_ToggleInventoryWindowInputHandled;
        _inputHandler.UseInventoryObjectInputHandled += On_InputHandler_UseInventoryObjectInputHandled;
        _inputHandler.PutAwayInventoryObjectInputHandled += On_InputHandler_PutAwayInventoryObjectInputHandled;
        _inputHandler.GoUpStairInputHandled += On_InputHandler_GoUpStairInputHandled;
        _inputHandler.GoDownStairInputHandled += On_InputHandler_GoDownStairInputHandled;
        _inputHandler.RestartGameInputHandled += On_InputHandler_RestartGameInputHandled;
    }

    public void Update()
    {
        // GD.Print("等待输入");

        _inputHandler.Update();
    }

    private void On_InputHandler_MovementInputHandled(Vector2I _)
    {
        Updated.Invoke();
    }

    private void On_InputHandler_PickUpInputHandled()
    {
        Updated.Invoke();
    }

    private void On_InputHandler_ToggleInventoryWindowInputHandled()
    {
        _inventoryWindow.Toggle();
    }

    private void On_InputHandler_UseInventoryObjectInputHandled()
    {
        _inventoryWindow.UseInventoryObject();

        Updated.Invoke();
    }

    private void On_InputHandler_PutAwayInventoryObjectInputHandled()
    {
        _inventoryWindow.PutAwayInventoryObject();

        Updated.Invoke();
    }

    private void On_InputHandler_GoUpStairInputHandled()
    {
        _stairManager.TryGoToPreviousScene();
    }

    private void On_InputHandler_GoDownStairInputHandled()
    {
        _stairManager.TryGoToNextScene();
    }

    private void On_InputHandler_RestartGameInputHandled()
    {
        _saveLoadManager.TryDeleteSavedFile();

        GetTree().ChangeSceneToFile("res://scenes//forest/forest.tscn");
    }
}