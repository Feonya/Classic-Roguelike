using Godot;

public partial class InventoryWindow : CanvasLayer, IUi
{
    private PackedScene _inventoryObjectScene;

    private VBoxContainer _inventoryObjectContainer;
    private Label _descriptionLabel;

    private Player _player;

    private int _selectedInventoryObjectIndex;

    public void Initialize()
    {
        _inventoryObjectScene = GD.Load<PackedScene>(
            "res://ui/inventory_window/inventory_object/inventory_object.tscn"
        );

        _inventoryObjectContainer = GetNode<VBoxContainer>("%InventoryObjectContainer");
        _descriptionLabel = GetNode<Label>("%DescriptionLabel");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");
    }

    public void Update(double delta)
    {
    }

    public void Toggle()
    {
        Visible = !Visible;

        if (Visible)
        {
            GenerateInventoryObjects();
        }
        else
        {
            ClearInventoryObjects();
        }
    }

    public void GenerateInventoryObjects()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.Inventory.Count == 0) { return; }

        for (int i = 0; i < playerData.Inventory.Count; i++)
        {
            var inventoryObject = _inventoryObjectScene.Instantiate<InventoryObject>();
            inventoryObject.Text =
                (i + 1).ToString() + ". " + playerData.Inventory[i].Name_;

            // if (playerData.LeftHandHoldEquipment == playerData.Inventory[i] ||
            //     playerData.RightHandHoldEquipment == playerData.Inventory[i] ||
            //     playerData.BodyWearEquipment == playerData.Inventory[i] ||
            //     playerData.FingerWearEquipment == playerData.Inventory[i] ||
            //     playerData.NeckWearEquipment == playerData.Inventory[i])
            if (playerData.Inventory[i] is Equipment &&
                (playerData.Inventory[i] as Equipment).IsEquipped)
            {
                inventoryObject.Text += " [已装备]";
            }

            _inventoryObjectContainer.AddChild(inventoryObject);

            inventoryObject.Initialize();
            inventoryObject.Selected += On_InventoryObject_Selected;
        }

        _inventoryObjectContainer.GetChild<Button>(0).GrabFocus();
    }

    public void ClearInventoryObjects()
    {
        for (int i = 0; i < _inventoryObjectContainer.GetChildCount(); i++)
        {
            _inventoryObjectContainer.GetChild(i).QueueFree();
        }

        _descriptionLabel.Text = "";
    }

    public void UseInventoryObject()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.Inventory.Count == 0) { return; }

        var pickableObject = playerData.Inventory[_selectedInventoryObjectIndex];

        if (pickableObject is not IConsumableItem &&
            pickableObject is not IEquipableEquipment)
        {
            return;
        }

        if (pickableObject is IConsumableItem)
        {
            (pickableObject as IConsumableItem).Consume();

            playerData.Inventory.Remove(pickableObject);
        }
        else if (pickableObject is IEquipableEquipment)
        {
            (pickableObject as IEquipableEquipment).Equip();
        }

        Toggle();
    }

    public void PutAwayInventoryObject()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.Inventory.Count == 0) { return; }

        var selectedPickableObject = playerData.Inventory[_selectedInventoryObjectIndex];

        if (selectedPickableObject is IEquipableEquipment)
        {
            (selectedPickableObject as IEquipableEquipment).Unequip();
        }

        if (selectedPickableObject is IImmediateEffectItem)
        {
            (selectedPickableObject as IImmediateEffectItem).UndoImmediateEffect();
        }

        playerData.Inventory.Remove(selectedPickableObject);

        Toggle();
    }

    private void On_InventoryObject_Selected(InventoryObject focusedInventoryObject)
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.Inventory.Count == 0) { return; }

        _selectedInventoryObjectIndex = focusedInventoryObject.GetIndex();

        var pickableObject = playerData.Inventory[_selectedInventoryObjectIndex];

        var postFix = "\n\n";
        if (pickableObject is IConsumableItem)
        {
            postFix += "(回车键使用物品)";
        }
        else if (pickableObject is IEquipableEquipment)
        {
            postFix += "(回车键替换装备)";
        }

        _descriptionLabel.Text = pickableObject.Description + postFix;
    }
}
