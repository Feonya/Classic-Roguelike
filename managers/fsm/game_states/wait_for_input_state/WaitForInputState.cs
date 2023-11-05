using System;
using Godot;

public partial class WaitForInputState : Node, IGameState
{
    public event Action Updated;

    private InputHandler _inputHandler;

    private InventoryWindow _inventoryWindow;

    public void Initialize()
    {
        _inputHandler = GetTree().CurrentScene.GetNode<InputHandler>("%InputHandler");

        _inventoryWindow = GetTree().CurrentScene.GetNode<InventoryWindow>("%InventoryWindow");

        _inputHandler.IdleInputHandled += On_InputHandler_IdleInputHandled;
        _inputHandler.MovementInputHandled += On_InputHandler_MovementInputHandled;
        _inputHandler.PickUpInputHandled += On_InputHandler_PickUpInputHandled;
        _inputHandler.PutAwayInventoryObjectInputHandled += On_InputHandler_PutAwayInventoryObjectInputHandled;
        _inputHandler.ToggleInventoryWindowInputHandled += On_InputHandler_OpenInventoryWindowInputHandled;
        _inputHandler.UseInventoryObjectInputHandled += On_InputHandler_UseInventoryObjectInputHandled;
        _inputHandler.RestartGameInputHandled += On_InputHandler_RestartGameInputHandled;
    }

    public void Update(double delta)
    {
        // GD.Print("等待输入");
        _inputHandler.Update(delta);
    }

    private void On_InputHandler_IdleInputHandled()
    {
        Updated.Invoke();
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

    private void On_InputHandler_OpenInventoryWindowInputHandled()
    {
        _inventoryWindow.Toggle();
    }

    private void On_InputHandler_UseInventoryObjectInputHandled()
    {
        _inventoryWindow.UseInventoryObject();

        Updated.Invoke();
    }

    private void On_InputHandler_RestartGameInputHandled()
    {
        GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager").TryDeletePersistentFile(); ;
        GetTree().ChangeSceneToFile("res://scenes/forest/forest.tscn");
    }
}