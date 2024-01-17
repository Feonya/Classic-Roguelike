using Godot;
using System;

public partial class InventoryObject : Button, IUi
{
    public event Action<InventoryObject> Selected;

    public void Initialize()
    {
        FocusEntered += () =>
        {
            Selected?.Invoke(this);
        };
    }

    public void Update()
    {
    }
}
