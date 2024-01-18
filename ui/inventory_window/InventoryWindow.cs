using Godot;

public partial class InventoryWindow : CanvasLayer, IUi
{
    private PackedScene _inventoryObjectScene;

    private VBoxContainer _inventoryContainer;
    private Label _descriptionLabel;

    private Player _player;

    private int _selectedInventoryObjectIndex;

    public void Initialize()
    {
        _inventoryObjectScene = GD.Load<PackedScene>(
            "res://ui/inventory_window/inventory_object/inventory_object.tscn"
        );

        _inventoryContainer = GetNode<VBoxContainer>("%InventoryContainer");
        _descriptionLabel = GetNode<Label>("%DescriptionLabel");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");
    }

    public void Update()
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

    public void UseInventoryObject()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.Inventory.Count == 0) { return; }

        var pickableObject = playerData.Inventory[_selectedInventoryObjectIndex];

        if (pickableObject is not IConsumableItem && pickableObject is not Equipment)
        {
            return;
        }

        if (pickableObject is IConsumableItem)
        {
            (pickableObject as IConsumableItem).Consume();
        }
        else if (pickableObject is Equipment)
        {
            (pickableObject as Equipment).Equip();
        }

        Toggle();
    }

    public void PutAwayInventoryObject()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.Inventory.Count == 0) { return; }

        var pickableObject = playerData.Inventory[_selectedInventoryObjectIndex];

        if (pickableObject is IImmediateEffectItem)
        {
            (pickableObject as IImmediateEffectItem).UndoImmediateEffect();
        }
        else if (pickableObject is Equipment)
        {
            (pickableObject as Equipment).Unequip();
        }

        playerData.Inventory.Remove(pickableObject);

        Toggle();
    }

    private void GenerateInventoryObjects()
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.Inventory.Count == 0) { return; }

        for (int i = 0; i < playerData.Inventory.Count; i++)
        {
            var inventoryObject = _inventoryObjectScene.Instantiate<InventoryObject>();
            inventoryObject.Text = (i + 1).ToString() + ". " + playerData.Inventory[i].Name_;

            if (playerData.Inventory[i] is Equipment &&
                (playerData.Inventory[i] as Equipment).IsEquipped)
            {
                inventoryObject.Text += " [已装备]";
            }

            _inventoryContainer.AddChild(inventoryObject);
            inventoryObject.Initialize();

            inventoryObject.Selected += On_InventoryObject_Selected;
        }

        _inventoryContainer.GetChild<Button>(0).GrabFocus();
    }

    private void ClearInventoryObjects()
    {
        for (int i = 0; i < _inventoryContainer.GetChildCount(); i++)
        {
            _inventoryContainer.GetChild(i).QueueFree();
        }

        _descriptionLabel.Text = "";
    }

    private void On_InventoryObject_Selected(InventoryObject focusedInventoryObject)
    {
        var playerData = _player.CharacterData as PlayerData;

        if (playerData.Inventory.Count == 0) { return; }

        _selectedInventoryObjectIndex = focusedInventoryObject.GetIndex();

        var pickableObject = playerData.Inventory[_selectedInventoryObjectIndex];

        _descriptionLabel.Text = pickableObject.Description;
    }
}
