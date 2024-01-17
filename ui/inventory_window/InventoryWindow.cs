using Godot;
using System;

public partial class InventoryWindow : CanvasLayer, IUi
{
    private PackedScene _inventoryObjectScene;
    
    private VBoxContainer _inventoryContainer;
    private Label _descriptionLabel;

    private Player _player;

    private int _selectedInventoryObjectIndex;

    public void Initialize()
    {
        _inventoryObjectScene = GD.Load<PackedScene>("res://ui/inventory_window/inventory_object/inventory_object.tscn");

        _inventoryContainer = GetNode<VBoxContainer>("%InventoryContainer");
        _descriptionLabel = GetNode<Label>("%DescriptionLabel");

        _player = this.GetUnique<Player>();
    }

    public void Update()
    {

    }

    /// <summary>
    /// 使用选中的装备
    /// </summary>
    public void UseInventoryObject()
    {
        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.Inventory.Count == 0)
                return;

            var pickableObject = playerData.Inventory[_selectedInventoryObjectIndex];
            if(pickableObject is not IConsumableItem &&
               pickableObject is not Equipment)
            {
                return;
            }

            if(pickableObject is IConsumableItem consumable)
            {
                consumable.Consume();
            }
            else if(pickableObject is Equipment equipment)
            {
                equipment.Equip();
            }

            Toggle();
        }
    }

    /// <summary>
    /// 丢弃物品
    /// </summary>
    public void PutAwayInventoryObject()
    {

        if (_player.CharacterData is PlayerData playerData)
        {
            if (playerData.Inventory.Count == 0)
                return;

            var pickableObject = playerData.Inventory[_selectedInventoryObjectIndex];
            

            if (pickableObject is IImmediateEffectItem immediate)
            {
                immediate.UnImmediateEffect();
            }
            else if (pickableObject is Equipment equipment)
            {
                equipment.Unequip();
            }

            playerData.Inventory.Remove(pickableObject);
            Toggle();
        }
    }


    public void Toggle()
    {
        Visible = !Visible;
        if(Visible)
            GenerateInventoryObjects();
        else
            ClearInventoryObjects();
    }

    private void GenerateInventoryObjects()
    {
        var playerData = _player.CharacterData as PlayerData;
        if (playerData.Inventory.Count == 0)
            return;

        for (int i = 0; i < playerData.Inventory.Count; i++)
        {
            var inventoryObject = _inventoryObjectScene.Instantiate<InventoryObject>();
            inventoryObject.Text = $"{i+1}. {playerData.Inventory[i].Name_}";

            if (playerData.Inventory[i] is Equipment equipment)
            {
                if (equipment.IsEquipped)
                {
                    inventoryObject.Text += " [已装备]";
                }
            }

            inventoryObject.Selected += On_InventoryObject_Selected;

            _inventoryContainer.AddChild(inventoryObject);
            inventoryObject.Initialize();
        }

        _inventoryContainer.GetChild<Button>(0).GrabFocus();
    }

    private void On_InventoryObject_Selected(InventoryObject focusedInventoryObject)
    {
        var playerData = _player.CharacterData as PlayerData;
        if (playerData.Inventory.Count == 0)
            return;

        _selectedInventoryObjectIndex = focusedInventoryObject.GetIndex();

        var pickableObject = playerData.Inventory[_selectedInventoryObjectIndex];
        _descriptionLabel.Text = pickableObject.Description;

    }

    private void ClearInventoryObjects()
    {
        for (int i = 0; i < _inventoryContainer.GetChildCount(); i++)
        {
            _inventoryContainer.GetChild(i).QueueFree();
        }
        _descriptionLabel.Text = "";
    }
}
